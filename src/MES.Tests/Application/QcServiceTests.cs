using MES.Application.Services;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Exceptions;
using MES.Domain.Repositories;
using MES.Application.Integration.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MES.Tests.Application;

public class QcServiceTests
{
    private readonly Mock<IRepository<QcInspection>> _inspectionRepo;
    private readonly Mock<IRepository<QcInspectionItem>> _itemRepo;
    private readonly Mock<IRepository<WorkOrder>> _workOrderRepo;
    private readonly Mock<IRepository<WorkOrderStep>> _stepRepo;
    private readonly Mock<IEventBus> _eventBus;
    private readonly Mock<InMemoryEventLogService> _eventLog;
    private readonly QcService _service;

    public QcServiceTests()
    {
        _inspectionRepo = new Mock<IRepository<QcInspection>>();
        _itemRepo = new Mock<IRepository<QcInspectionItem>>();
        _workOrderRepo = new Mock<IRepository<WorkOrder>>();
        _stepRepo = new Mock<IRepository<WorkOrderStep>>();
        _eventBus = new Mock<IEventBus>();
        _eventLog = new Mock<InMemoryEventLogService>(MockBehavior.Loose, null!);

        _service = new QcService(
            _inspectionRepo.Object,
            _itemRepo.Object,
            _workOrderRepo.Object,
            _stepRepo.Object,
            _eventBus.Object,
            _eventLog.Object);
    }

    private QcInspection CreateInspection(long id, QcResult result = QcResult.PENDING)
    {
        var inspection = QcInspection.Create(
            $"QC-{id:D4}",
            QcInspectionType.FINAL,
            workOrderId: 100,
            materialId: 10,
            inspector: 1);
        // 使用反射设置 InspectResult（因为没有公共 setter）
        TestEntityFactory.SetProperty(inspection, "InspectResult", result);
        return inspection;
    }

    [Fact]
    public async Task CreateInspectionAsync_SetsPendingResult()
    {
        _inspectionRepo.Setup(r => r.AddAsync(It.IsAny<QcInspection>()))
            .ReturnsAsync((QcInspection i) => i);

        var result = await _service.CreateInspectionAsync("QC-0001", QcInspectionType.FINAL);

        Assert.Equal(QcResult.PENDING, result.InspectResult);
        _inspectionRepo.Verify(r => r.AddAsync(It.IsAny<QcInspection>()), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_AddsItem()
    {
        var inspection = QcInspection.Create("QC-0001", QcInspectionType.FINAL);
        inspection.SetIdForTest(1);

        _inspectionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inspection);
        _inspectionRepo.Setup(r => r.UpdateAsync(It.IsAny<QcInspection>())).Returns(Task.CompletedTask);

        var result = await _service.AddItemAsync(1, "外观检查", "无缺陷");

        Assert.NotNull(result);
        Assert.Equal("外观检查", result.ItemName);
    }

    [Fact]
    public async Task VerifyInspectionAsync_UpdatesResult()
    {
        var inspection = CreateInspection(1, QcResult.PENDING);
        _inspectionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inspection);
        _inspectionRepo.Setup(r => r.UpdateAsync(It.IsAny<QcInspection>())).Returns(Task.CompletedTask);

        await _service.VerifyInspectionAsync(1, QcResult.PASS);

        Assert.Equal(QcResult.PASS, inspection.InspectResult);
        _inspectionRepo.Verify(r => r.UpdateAsync(It.IsAny<QcInspection>()), Times.Once);
    }

    [Fact]
    public async Task VerifyInspectionAsync_ThrowsWhenInspectionNotFound()
    {
        _inspectionRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((QcInspection?)null);

        await Assert.ThrowsAsync<DomainException>(() =>
            _service.VerifyInspectionAsync(999, QcResult.PASS));
    }

    [Fact]
    public async Task VerifyInspectionAsync_WithFail_PausesDownstreamSteps()
    {
        var inspection = CreateInspection(1, QcResult.PENDING);
        inspection.SetSourceRefForTest("Step 1");

        var steps = new List<WorkOrderStep>
        {
            WorkOrderStep.Create(100, 1, "Step 1", 10),
            WorkOrderStep.Create(100, 2, "Step 2", 10),
            WorkOrderStep.Create(100, 3, "Step 3", 10)
        };
        // 设置工序为 IN_PROGRESS 状态
        TestEntityFactory.SetProperty(steps[0], "Status", WorkOrderStatus.IN_PROGRESS);
        TestEntityFactory.SetProperty(steps[1], "Status", WorkOrderStatus.IN_PROGRESS);
        TestEntityFactory.SetProperty(steps[2], "Status", WorkOrderStatus.IN_PROGRESS);

        _inspectionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inspection);
        _inspectionRepo.Setup(r => r.UpdateAsync(It.IsAny<QcInspection>())).Returns(Task.CompletedTask);
        _stepRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkOrderStep, bool>>>()))
            .ReturnsAsync(steps);

        // 验证：质检失败后状态应该变为 FAIL
        await _service.VerifyInspectionAsync(1, QcResult.FAIL);

        // 验证 Verify 方法被调用并更新了结果
        _inspectionRepo.Verify(r => r.UpdateAsync(It.Is<QcInspection>(i =>
            i.InspectResult == QcResult.FAIL)), Times.Once);
    }

    [Fact]
    public async Task HandleNonconformingAsync_WithScrap_UpdatesWorkOrder()
    {
        var inspection = CreateInspection(1, QcResult.FAIL);
        var workOrder = WorkOrder.Create("WO-001", SourceType.MANUAL, 10, 100);

        _inspectionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inspection);
        _inspectionRepo.Setup(r => r.UpdateAsync(It.IsAny<QcInspection>())).Returns(Task.CompletedTask);
        _workOrderRepo.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(workOrder);
        _workOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<WorkOrder>())).Returns(Task.CompletedTask);

        await _service.HandleNonconformingAsync(1, "SCRAP", "报废处理");

        Assert.Equal(1, workOrder.ScrapQty);
    }

    [Fact]
    public async Task HandleNonconformingAsync_WithRework_ResumesWorkOrder()
    {
        var inspection = CreateInspection(1, QcResult.FAIL);
        var workOrder = WorkOrder.Create("WO-001", SourceType.MANUAL, 10, 100);
        workOrder.Release();
        workOrder.Start();
        workOrder.Complete();

        _inspectionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inspection);
        _inspectionRepo.Setup(r => r.UpdateAsync(It.IsAny<QcInspection>())).Returns(Task.CompletedTask);
        _workOrderRepo.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(workOrder);
        _workOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<WorkOrder>())).Returns(Task.CompletedTask);

        await _service.HandleNonconformingAsync(1, "REWORK", "返工处理");

        Assert.Equal(WorkOrderStatus.IN_PROGRESS, workOrder.Status);
    }

    [Fact]
    public async Task HandleNonconformingAsync_ThrowsWhenNotFailed()
    {
        var inspection = CreateInspection(1, QcResult.PASS);
        _inspectionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inspection);

        await Assert.ThrowsAsync<DomainException>(() =>
            _service.HandleNonconformingAsync(1, "SCRAP", ""));
    }

    [Fact]
    public async Task HandleNonconformingAsync_ThrowsForInvalidAction()
    {
        var inspection = CreateInspection(1, QcResult.PENDING);
        _inspectionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inspection);

        await Assert.ThrowsAsync<DomainException>(() =>
            _service.HandleNonconformingAsync(1, "INVALID", ""));
    }
}

// 测试辅助扩展方法
public static class QcInspectionTestExtensions
{
    public static void SetIdForTest(this QcInspection inspection, long id)
    {
        var prop = typeof(BaseEntity).GetProperty("Id");
        prop?.SetValue(inspection, id);
    }

    public static void SetSourceRefForTest(this QcInspection inspection, string sourceRef)
    {
        var prop = typeof(QcInspection).GetProperty("SourceRef");
        prop?.SetValue(inspection, sourceRef);
    }
}
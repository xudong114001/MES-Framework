using MES.Application.Dtos;
using MES.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Repositories;
using Moq;
using Xunit;

namespace MES.Tests.Application;

public class TraceServiceTests
{
    private readonly Mock<IRepository<MaterialTrace>> _traceRepo;
    private readonly Mock<IRepository<Material>> _materialRepo;
    private readonly Mock<IRepository<WorkOrder>> _workOrderRepo;
    private readonly TraceService _service;

    public TraceServiceTests()
    {
        _traceRepo = new Mock<IRepository<MaterialTrace>>();
        _materialRepo = new Mock<IRepository<Material>>();
        _workOrderRepo = new Mock<IRepository<WorkOrder>>();
        _service = new TraceService(_traceRepo.Object, _materialRepo.Object, _workOrderRepo.Object);
    }

    #region TraceByBatchAsync

    [Fact]
    public async Task TraceByBatchAsync_ReturnsTraceResultDto()
    {
        var material = TestEntityFactory.CreateMaterial(id: 1, code: "MAT-001", name: "Test Material");
        var traces = new List<MaterialTrace>
        {
            TestEntityFactory.CreateMaterialTraceDirect(id: 1, materialId: 1, batchNo: "BATCH-001", workOrderId: 10, direction: "CONSUME", qty: 50),
            TestEntityFactory.CreateMaterialTraceDirect(id: 2, materialId: 1, batchNo: "BATCH-001", workOrderId: 11, direction: "PRODUCE", qty: 30),
        };

        _traceRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MaterialTrace, bool>>>()))
            .ReturnsAsync(traces);
        _materialRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(material);

        var result = await _service.TraceByBatchAsync("BATCH-001");

        Assert.NotNull(result);
        Assert.IsType<TraceResultDto>(result);
        Assert.Equal("Batch", result.TraceType);
        Assert.Equal("BATCH-001", result.BatchNo);
        Assert.Equal("MAT-001", result.MaterialCode);
        Assert.Equal("Test Material", result.MaterialName);
        Assert.Equal(2, result.Steps.Count);
    }

    [Fact]
    public async Task TraceByBatchAsync_ReturnsEmptySteps_WhenNoTracesFound()
    {
        _traceRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MaterialTrace, bool>>>()))
            .ReturnsAsync([]);

        var result = await _service.TraceByBatchAsync("NONEXISTENT");

        Assert.NotNull(result);
        Assert.Equal("Batch", result.TraceType);
        Assert.Equal("NONEXISTENT", result.BatchNo);
        Assert.Empty(result.Steps);
        Assert.Equal(string.Empty, result.MaterialCode);
    }

    #endregion

    #region TraceBySerialAsync

    [Fact]
    public async Task TraceBySerialAsync_ReturnsTraceResultDto()
    {
        var material = TestEntityFactory.CreateMaterial(id: 1, code: "MAT-001", name: "Test Material");
        var traces = new List<MaterialTrace>
        {
            TestEntityFactory.CreateMaterialTraceDirect(id: 1, materialId: 1, batchNo: "BATCH-001", workOrderId: 10, direction: "PRODUCE", qty: 1),
        };

        _traceRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MaterialTrace, bool>>>()))
            .ReturnsAsync(traces);
        _materialRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(material);

        var result = await _service.TraceBySerialAsync("SN-001");

        Assert.NotNull(result);
        Assert.IsType<TraceResultDto>(result);
        Assert.Equal("Serial", result.TraceType);
        Assert.Equal("SN-001", result.SerialNo);
        Assert.Equal("BATCH-001", result.BatchNo);
        Assert.Single(result.Steps);
    }

    [Fact]
    public async Task TraceBySerialAsync_ReturnsEmptySteps_WhenNoTracesFound()
    {
        _traceRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MaterialTrace, bool>>>()))
            .ReturnsAsync([]);

        var result = await _service.TraceBySerialAsync("NONEXISTENT");

        Assert.NotNull(result);
        Assert.Equal("Serial", result.TraceType);
        Assert.Equal("NONEXISTENT", result.SerialNo);
        Assert.Empty(result.Steps);
    }

    #endregion

    #region TraceForwardAsync

    [Fact]
    public async Task TraceForwardAsync_ReturnsTraceResultDto()
    {
        var material = TestEntityFactory.CreateMaterial(id: 1, code: "MAT-001", name: "Raw Material");
        var traces = new List<MaterialTrace>
        {
            TestEntityFactory.CreateMaterialTraceDirect(id: 1, materialId: 1, batchNo: "BATCH-001", workOrderId: 10, direction: "CONSUME", qty: 50),
        };

        _traceRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MaterialTrace, bool>>>()))
            .ReturnsAsync(traces);
        _materialRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(material);

        var result = await _service.TraceForwardAsync(1, "BATCH-001");

        Assert.NotNull(result);
        Assert.IsType<TraceResultDto>(result);
        Assert.Equal("Forward", result.TraceType);
        Assert.Equal("BATCH-001", result.BatchNo);
        Assert.Equal("MAT-001", result.MaterialCode);
        Assert.Single(result.Steps);
    }

    #endregion

    #region TraceBackwardAsync

    [Fact]
    public async Task TraceBackwardAsync_ReturnsTraceResultDto()
    {
        var material = TestEntityFactory.CreateMaterial(id: 1, code: "MAT-001", name: "Finished Good");
        var traces = new List<MaterialTrace>
        {
            TestEntityFactory.CreateMaterialTraceDirect(id: 1, materialId: 1, batchNo: "BATCH-001", workOrderId: 10, direction: "CONSUME", qty: 20),
            TestEntityFactory.CreateMaterialTraceDirect(id: 2, materialId: 1, batchNo: "BATCH-002", workOrderId: 10, direction: "CONSUME", qty: 30),
        };

        _traceRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MaterialTrace, bool>>>()))
            .ReturnsAsync(traces);
        _materialRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(material);

        var result = await _service.TraceBackwardAsync("SN-001");

        Assert.NotNull(result);
        Assert.IsType<TraceResultDto>(result);
        Assert.Equal("Backward", result.TraceType);
        Assert.Equal("SN-001", result.SerialNo);
        Assert.Equal(2, result.Steps.Count);
    }

    #endregion
}

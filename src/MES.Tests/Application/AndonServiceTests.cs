using MES.Application.Interfaces;
using MES.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Repositories;
using MES.Tests;
using Moq;
using Xunit;

namespace MES.Tests.Application;

public class AndonServiceTests
{
    private readonly Mock<IRepository<AndonEvent>> _repository;
    private readonly AndonService _service;

    public AndonServiceTests()
    {
        _repository = new Mock<IRepository<AndonEvent>>();
        _service = new AndonService(_repository.Object);
    }

    private AndonEvent CreateEvent(long id, bool resolved = false)
    {
        var evt = TestEntityFactory.CreateAndonEventDirect(
            id: id,
            eventType: AndonEventType.EQUIPMENT_FAULT,
            level: AndonEventLevel.Warning,
            title: $"异常 {id}",
            triggeredAt: DateTime.UtcNow.AddHours(-id),
            resolvedAt: resolved ? DateTime.UtcNow : null,
            resolvedById: resolved ? 1 : null,
            resolvedByName: resolved ? "张三" : null
        );
        return evt;
    }

    [Fact]
    public async Task GetActiveEventsAsync_ReturnsOnlyUnresolved()
    {
        var events = new List<AndonEvent>
        {
            CreateEvent(1, resolved: false),
            CreateEvent(2, resolved: true),
            CreateEvent(3, resolved: false)
        };

        _repository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AndonEvent, bool>>>()))
            .ReturnsAsync(events.Where(e => e.ResolvedAt == null).ToList());

        var result = (await _service.GetActiveEventsAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Null(e.ResolvedAt));
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsAllOrderedByTriggeredAt()
    {
        var events = new List<AndonEvent>
        {
            CreateEvent(1),
            CreateEvent(2),
            CreateEvent(3)
        };

        _repository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AndonEvent, bool>>>()))
            .ReturnsAsync(events);

        var result = (await _service.GetAllEventsAsync()).ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetEventsAsync_ReturnsPagedResults()
    {
        var allEvents = Enumerable.Range(1, 25).Select(i => CreateEvent(i)).ToList();

        _repository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AndonEvent, bool>>>()))
            .ReturnsAsync(allEvents);

        var (items, total) = await _service.GetEventsAsync(page: 2, pageSize: 10);

        Assert.Equal(25, total);
    }

    [Fact]
    public async Task GetEventsAsync_FiltersByResolved()
    {
        var events = new List<AndonEvent>
        {
            CreateEvent(1, resolved: false),
            CreateEvent(2, resolved: true),
            CreateEvent(3, resolved: false)
        };

        _repository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AndonEvent, bool>>>()))
            .ReturnsAsync(events.Where(e => e.ResolvedAt != null).ToList());

        var (items, total) = await _service.GetEventsAsync(isResolved: true);

        Assert.Equal(1, total);
    }

    [Fact]
    public async Task GetEventsAsync_FiltersByEventType()
    {
        var events = new List<AndonEvent>
        {
            TestEntityFactory.CreateAndonEventDirect(id: 1, eventType: AndonEventType.EQUIPMENT_FAULT, title: "设备故障"),
            TestEntityFactory.CreateAndonEventDirect(id: 2, eventType: AndonEventType.QUALITY_ALARM, title: "质量异常"),
            TestEntityFactory.CreateAndonEventDirect(id: 3, eventType: AndonEventType.EQUIPMENT_FAULT, title: "设备故障2")
        };

        _repository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AndonEvent, bool>>>()))
            .ReturnsAsync(events.Where(e => e.EventType == AndonEventType.EQUIPMENT_FAULT).ToList());

        var (items, total) = await _service.GetEventsAsync(eventType: AndonEventType.EQUIPMENT_FAULT);

        Assert.Equal(2, total);
    }

    [Fact]
    public async Task TriggerEventAsync_CreatesNewEvent()
    {
        _repository.Setup(r => r.AddAsync(It.IsAny<AndonEvent>()))
            .ReturnsAsync((AndonEvent e) => e);

        var result = await _service.TriggerEventAsync(
            AndonEventType.EQUIPMENT_FAULT,
            AndonEventLevel.Critical,
            "设备故障报警",
            "2号产线加工中心故障",
            workstationId: 10,
            workstationName: "加工中心",
            workOrderId: 100,
            workOrderNo: "WO-001",
            triggeredById: 1,
            triggeredByName: "张三");

        Assert.NotNull(result);
        Assert.Equal(AndonEventType.EQUIPMENT_FAULT, result.EventType);
        Assert.Equal(AndonEventLevel.Critical, result.Level);
        Assert.Equal("设备故障报警", result.Title);
        Assert.Equal(10, result.WorkstationId);
        Assert.Equal("WO-001", result.WorkOrderNo);
        _repository.Verify(r => r.AddAsync(It.IsAny<AndonEvent>()), Times.Once);
    }

    [Fact]
    public async Task ResolveEventAsync_ResolvesSuccessfully()
    {
        var evt = CreateEvent(1);
        _repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(evt);
        _repository.Setup(r => r.UpdateAsync(It.IsAny<AndonEvent>())).Returns(Task.CompletedTask);

        var result = await _service.ResolveEventAsync(1, 1, "李四");

        Assert.True(result);
        Assert.NotNull(evt.ResolvedAt);
        Assert.Equal(1, evt.ResolvedById);
        Assert.Equal("李四", evt.ResolvedByName);
        _repository.Verify(r => r.UpdateAsync(evt), Times.Once);
    }

    [Fact]
    public async Task ResolveEventAsync_ReturnsFalseWhenNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((AndonEvent?)null);

        var result = await _service.ResolveEventAsync(999, 1, "李四");

        Assert.False(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEvent()
    {
        var evt = CreateEvent(1);
        _repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(evt);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((AndonEvent?)null);

        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteEventAsync_DeletesSuccessfully()
    {
        var evt = CreateEvent(1);
        _repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(evt);
        _repository.Setup(r => r.UpdateAsync(It.IsAny<AndonEvent>())).Returns(Task.CompletedTask);

        var result = await _service.DeleteEventAsync(1);

        Assert.True(result);
        Assert.True(evt.IsDeleted);
        _repository.Verify(r => r.UpdateAsync(evt), Times.Once);
    }

    [Fact]
    public async Task DeleteEventAsync_ReturnsFalseWhenNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((AndonEvent?)null);

        var result = await _service.DeleteEventAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task GetActiveCountAsync_ReturnsCount()
    {
        var events = new List<AndonEvent>
        {
            CreateEvent(1, resolved: false),
            CreateEvent(2, resolved: false),
            CreateEvent(3, resolved: true)
        };

        _repository.Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AndonEvent, bool>>>()))
            .ReturnsAsync(events.Count(e => e.ResolvedAt == null));

        var result = await _service.GetActiveCountAsync();

        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetActiveCountByTypeAsync_ReturnsGroupedCount()
    {
        var events = new List<AndonEvent>
        {
            TestEntityFactory.CreateAndonEventDirect(id: 1, eventType: AndonEventType.EQUIPMENT_FAULT),
            TestEntityFactory.CreateAndonEventDirect(id: 2, eventType: AndonEventType.EQUIPMENT_FAULT),
            TestEntityFactory.CreateAndonEventDirect(id: 3, eventType: AndonEventType.QUALITY_ALARM)
        };

        _repository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AndonEvent, bool>>>()))
            .ReturnsAsync(events);

        var result = await _service.GetActiveCountByTypeAsync();

        Assert.Equal(2, result[AndonEventType.EQUIPMENT_FAULT]);
        Assert.Equal(1, result[AndonEventType.QUALITY_ALARM]);
    }
}

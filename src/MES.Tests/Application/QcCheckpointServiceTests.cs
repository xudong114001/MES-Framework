using MES.Application.Interfaces;
using MES.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Exceptions;
using MES.Domain.Repositories;
using MES.Tests;
using Moq;
using Xunit;

namespace MES.Tests.Application;

public class QcCheckpointServiceTests
{
    private readonly Mock<IRepository<QcCheckpoint>> _checkpointRepo;
    private readonly QcCheckpointService _service;

    public QcCheckpointServiceTests()
    {
        _checkpointRepo = new Mock<IRepository<QcCheckpoint>>();
        _service = new QcCheckpointService(_checkpointRepo.Object);
    }

    private QcCheckpoint CreateCheckpoint(long id, long stepId, QcInspectionType checkType, bool isMandatory = true)
    {
        return TestEntityFactory.CreateQcCheckpointDirect(
            id: id,
            stepId: stepId,
            checkType: checkType,
            isMandatory: isMandatory,
            remark: $"质检点 {id}"
        );
    }

    [Fact]
    public async Task GetAllCheckpointsAsync_ReturnsAll()
    {
        var checkpoints = new List<QcCheckpoint>
        {
            CreateCheckpoint(1, 10, QcInspectionType.FIRST),
            CreateCheckpoint(2, 20, QcInspectionType.FINAL),
            CreateCheckpoint(3, 30, QcInspectionType.INCOMING)
        };

        _checkpointRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(checkpoints);

        var result = (await _service.GetAllCheckpointsAsync()).ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetCheckpointByIdAsync_ReturnsCheckpoint()
    {
        var checkpoint = CreateCheckpoint(1, 10, QcInspectionType.FIRST);
        _checkpointRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(checkpoint);

        var result = await _service.GetCheckpointByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetCheckpointByIdAsync_ReturnsNullWhenNotFound()
    {
        _checkpointRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((QcCheckpoint?)null);

        var result = await _service.GetCheckpointByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCheckpointsByStepAsync_ReturnsMatchingCheckpoints()
    {
        var stepId = 10L;
        var checkpoints = new List<QcCheckpoint>
        {
            CreateCheckpoint(1, stepId, QcInspectionType.FIRST),
            CreateCheckpoint(2, stepId, QcInspectionType.FINAL),
            CreateCheckpoint(3, 20, QcInspectionType.INCOMING)
        };

        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(checkpoints.Where(c => c.StepId == stepId).ToList());

        var result = (await _service.GetCheckpointsByStepAsync(stepId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(stepId, c.StepId));
    }

    [Fact]
    public async Task ConfigureCheckpointAsync_AddsNewCheckpoint()
    {
        var checkpoint = CreateCheckpoint(0, 10, QcInspectionType.FIRST);

        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<QcCheckpoint>());
        _checkpointRepo.Setup(r => r.AddAsync(It.IsAny<QcCheckpoint>()))
            .ReturnsAsync((QcCheckpoint c) => c);

        var result = await _service.ConfigureCheckpointAsync(checkpoint);

        Assert.NotNull(result);
        _checkpointRepo.Verify(r => r.AddAsync(checkpoint), Times.Once);
    }

    [Fact]
    public async Task ConfigureCheckpointAsync_ThrowsWhenDuplicate()
    {
        var existing = CreateCheckpoint(1, 10, QcInspectionType.FIRST);
        var newCheckpoint = CreateCheckpoint(0, 10, QcInspectionType.FIRST);

        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(new List<QcCheckpoint> { existing });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ConfigureCheckpointAsync(newCheckpoint));
    }

    [Fact]
    public async Task UpdateCheckpointAsync_UpdatesExisting()
    {
        var existing = CreateCheckpoint(1, 10, QcInspectionType.FIRST, isMandatory: true);
        var updated = CreateCheckpoint(1, 20, QcInspectionType.FINAL, isMandatory: false);

        _checkpointRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _checkpointRepo.Setup(r => r.UpdateAsync(It.IsAny<QcCheckpoint>())).Returns(Task.CompletedTask);

        await _service.UpdateCheckpointAsync(1, updated);

        _checkpointRepo.Verify(r => r.UpdateAsync(It.Is<QcCheckpoint>(c =>
            c.StepId == 20 &&
            c.CheckType == QcInspectionType.FINAL &&
            c.IsMandatory == false)), Times.Once);
    }

    [Fact]
    public async Task UpdateCheckpointAsync_ThrowsWhenNotFound()
    {
        var checkpoint = CreateCheckpoint(999, 10, QcInspectionType.FIRST);

        _checkpointRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((QcCheckpoint?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateCheckpointAsync(999, checkpoint));
    }

    [Fact]
    public async Task RemoveCheckpointAsync_DeletesSuccessfully()
    {
        var checkpoint = CreateCheckpoint(1, 10, QcInspectionType.FIRST);

        _checkpointRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(checkpoint);
        _checkpointRepo.Setup(r => r.DeleteAsync(It.IsAny<QcCheckpoint>())).Returns(Task.CompletedTask);

        await _service.RemoveCheckpointAsync(1);

        _checkpointRepo.Verify(r => r.DeleteAsync(checkpoint), Times.Once);
    }

    [Fact]
    public async Task RemoveCheckpointAsync_ThrowsWhenNotFound()
    {
        _checkpointRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((QcCheckpoint?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RemoveCheckpointAsync(999));
    }

    [Fact]
    public async Task HasPendingMandatoryCheckpointAsync_ReturnsTrueWhenExists()
    {
        var checkpoints = new List<QcCheckpoint>
        {
            CreateCheckpoint(1, 10, QcInspectionType.FIRST, isMandatory: true),
            CreateCheckpoint(2, 10, QcInspectionType.FINAL, isMandatory: false)
        };

        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(checkpoints.Where(c => c.StepId == 10 && c.IsMandatory).ToList());

        var result = await _service.HasPendingMandatoryCheckpointAsync(10);

        Assert.True(result);
    }

    [Fact]
    public async Task HasPendingMandatoryCheckpointAsync_ReturnsFalseWhenNone()
    {
        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<QcCheckpoint>());

        var result = await _service.HasPendingMandatoryCheckpointAsync(10);

        Assert.False(result);
    }
}

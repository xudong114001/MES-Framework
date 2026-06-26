using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Exceptions;
using MES.Domain.Repositories;

namespace MES.Application.Services;

/// <summary>
/// 工序质检点配置服务
/// </summary>
public class QcCheckpointService : IQcCheckpointService
{
    private readonly IRepository<QcCheckpoint> _checkpointRepo;

    public QcCheckpointService(IRepository<QcCheckpoint> checkpointRepo)
    {
        _checkpointRepo = checkpointRepo;
    }

    private static QcCheckpointDto MapToDto(QcCheckpoint entity) => new()
    {
        Id = entity.Id,
        StepId = entity.StepId,
        CheckType = entity.CheckType,
        IsMandatory = entity.IsMandatory,
        Remark = entity.Remark,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    /// <summary>
    /// 获取所有质检点
    /// </summary>
    public async Task<IEnumerable<QcCheckpointDto>> GetAllCheckpointsAsync()
    {
        var list = await _checkpointRepo.GetAllAsync();
        return list.Select(MapToDto);
    }

    /// <summary>
    /// 根据 ID 获取质检点
    /// </summary>
    public async Task<QcCheckpointDto?> GetCheckpointByIdAsync(long id)
    {
        var entity = await _checkpointRepo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    /// <summary>
    /// 查询某工序配置的质检点
    /// </summary>
    public async Task<IEnumerable<QcCheckpointDto>> GetCheckpointsByStepAsync(long stepId)
    {
        var list = await _checkpointRepo.FindAsync(c => c.StepId == stepId);
        return list.Select(MapToDto);
    }

    /// <summary>
    /// 配置质检点
    /// </summary>
    public async Task<QcCheckpointDto> ConfigureCheckpointAsync(ConfigureQcCheckpointRequest request)
    {
        var existing = await _checkpointRepo.FindAsync(
            c => c.StepId == request.StepId && c.CheckType == request.CheckType);
        if (existing.Any())
            throw new DomainException("该工序已配置相同类型的质检点");

        var checkpoint = new QcCheckpoint
        {
            StepId = request.StepId,
            CheckType = request.CheckType,
            IsMandatory = request.IsMandatory,
            Remark = request.Remark
        };

        var created = await _checkpointRepo.AddAsync(checkpoint);
        return MapToDto(created);
    }

    /// <summary>
    /// 更新质检点
    /// </summary>
    public async Task UpdateCheckpointAsync(long id, UpdateQcCheckpointRequest request)
    {
        var existing = await _checkpointRepo.GetByIdAsync(id);
        if (existing == null)
            throw new DomainException("质检点配置不存在");

        existing.StepId = request.StepId;
        existing.CheckType = request.CheckType;
        existing.IsMandatory = request.IsMandatory;
        existing.Remark = request.Remark;

        await _checkpointRepo.UpdateAsync(existing);
    }

    /// <summary>
    /// 取消配置质检点（软删除）
    /// </summary>
    public async Task RemoveCheckpointAsync(long checkpointId)
    {
        var checkpoint = await _checkpointRepo.GetByIdAsync(checkpointId);
        if (checkpoint == null)
            throw new DomainException("质检点配置不存在");

        checkpoint.MarkAsDeleted();
        await _checkpointRepo.UpdateAsync(checkpoint);
    }

    /// <summary>
    /// 判断某工序是否有未完成的强制质检点
    /// </summary>
    public async Task<bool> HasPendingMandatoryCheckpointAsync(long stepId)
    {
        var mandatoryCheckpoints = await _checkpointRepo.FindAsync(
            c => c.StepId == stepId && c.IsMandatory);

        if (!mandatoryCheckpoints.Any())
            return false;

        // 检查逻辑：如果有强制质检点配置，即认为需要质检
        // 实际检查是在 WorkReportService 中查看是否有 PENDING 的质检单
        return true;
    }
}

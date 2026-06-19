using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Repositories;

namespace MES.Application.Services;

/// <summary>
/// 工序质检点配置服务
/// </summary>
public class QcCheckpointService : IQcCheckpointService
{
    private readonly IRepository<QcCheckpoint> _checkpointRepo0;

    public QcCheckpointService(
        IRepository<QcCheckpoint> checkpointRepo)
    {
        _checkpointRepo0 = checkpointRepo;
    }

    /// <summary>
    /// 获取所有质检点（兼容前端）
    /// </summary>
    public async Task<IEnumerable<QcCheckpoint>> GetAllCheckpointsAsync()
    {
        return await _checkpointRepo0.GetAllAsync();
    }

    /// <summary>
    /// 根据 ID 获取质检点（兼容前端）
    /// </summary>
    public async Task<QcCheckpoint?> GetCheckpointByIdAsync(long id)
    {
        return await _checkpointRepo0.GetByIdAsync(id);
    }

    /// <summary>
    /// 查询某工序配置的质检点
    /// </summary>
    public async Task<IEnumerable<QcCheckpoint>> GetCheckpointsByStepAsync(long stepId)
    {
        return await _checkpointRepo0.FindAsync(c => c.StepId == stepId);
    }

    /// <summary>
    /// 配置质检点
    /// </summary>
    public async Task<QcCheckpoint> ConfigureCheckpointAsync(QcCheckpoint checkpoint)
    {
        var existing = await _checkpointRepo0.FindAsync(
            c => c.StepId == checkpoint.StepId && c.CheckType == checkpoint.CheckType);
        if (existing.Any())
            throw new InvalidOperationException("该工序已配置相同类型的质检点");

        return await _checkpointRepo0.AddAsync(checkpoint);
    }

    /// <summary>
    /// 更新质检点（兼容前端）
    /// </summary>
    public async Task UpdateCheckpointAsync(QcCheckpoint checkpoint)
    {
        var existing = await _checkpointRepo0.GetByIdAsync(checkpoint.Id);
        if (existing == null)
            throw new InvalidOperationException("质检点配置不存在");

        existing.StepId = checkpoint.StepId;
        existing.CheckType = checkpoint.CheckType;
        existing.IsMandatory = checkpoint.IsMandatory;
        existing.Remark = checkpoint.Remark;

        await _checkpointRepo0.UpdateAsync(existing);
    }

    /// <summary>
    /// 取消配置质检点
    /// </summary>
    public async Task RemoveCheckpointAsync(long checkpointId)
    {
        var checkpoint = await _checkpointRepo0.GetByIdAsync(checkpointId);
        if (checkpoint == null)
            throw new InvalidOperationException("质检点配置不存在");

        await _checkpointRepo0.DeleteAsync(checkpoint);
    }

    /// <summary>
    /// 判断某工序是否有未完成的强制质检点
    /// </summary>
    public async Task<bool> HasPendingMandatoryCheckpointAsync(long stepId)
    {
        var mandatoryCheckpoints = await _checkpointRepo0.FindAsync(
            c => c.StepId == stepId && c.IsMandatory);

        if (!mandatoryCheckpoints.Any())
            return false;

        // 检查逻辑：如果有强制质检点配置，即认为需要质检
        // 实际检查是在 WorkReportService 中查看是否有 PENDING 的质检单
        return true;
    }
}

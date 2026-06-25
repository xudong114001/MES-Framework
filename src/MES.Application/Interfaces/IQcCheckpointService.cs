using MES.Domain.Entities;

namespace MES.Application.Interfaces;

/// <summary>
/// 工序质检点配置服务接口
/// </summary>
public interface IQcCheckpointService
{
    /// <summary>获取所有质检点（兼容前端）</summary>
    Task<IEnumerable<QcCheckpoint>> GetAllCheckpointsAsync();

    /// <summary>根据 ID 获取质检点（兼容前端）</summary>
    Task<QcCheckpoint?> GetCheckpointByIdAsync(long id);

    /// <summary>查询某工序配置的质检点</summary>
    Task<IEnumerable<QcCheckpoint>> GetCheckpointsByStepAsync(long stepId);

    /// <summary>配置质检点</summary>
    Task<QcCheckpoint> ConfigureCheckpointAsync(QcCheckpoint checkpoint);

    /// <summary>更新质检点（兼容前端）</summary>
    Task UpdateCheckpointAsync(long id, QcCheckpoint checkpoint);

    /// <summary>取消配置质检点</summary>
    Task RemoveCheckpointAsync(long checkpointId);

    /// <summary>判断某工序是否有未完成的强制质检点</summary>
    Task<bool> HasPendingMandatoryCheckpointAsync(long stepId);
}

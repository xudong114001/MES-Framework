using MES.Domain.Entities;

namespace MES.Domain.Repositories;

/// <summary>
/// 用户仓储接口 - 封装用户相关的复杂查询和角色分配
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// 检查用户名是否已存在
    /// </summary>
    Task<bool> ExistsByUsernameAsync(string username, long? excludeId = null);

    /// <summary>
    /// 分配用户角色（替换原有角色）
    /// </summary>
    Task AssignRolesAsync(long userId, List<string> roleNames);

    /// <summary>
    /// 获取用户及其角色列表
    /// </summary>
    Task<User?> GetWithRolesAsync(long id);
}

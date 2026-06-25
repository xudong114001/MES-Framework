using MES.Domain.AggregateRoots;
using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class User : BaseEntity, IAggregateRoot
{
    internal User() { }

    public static User Create(string username, string displayName, string? passwordHash = null, string? email = null, string? phone = null, bool status = true)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("用户名不能为空", "USER_USERNAME_REQUIRED");
        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("显示名称不能为空", "USER_DISPLAY_NAME_REQUIRED");

        return new User
        {
            Username = username,
            PasswordHash = passwordHash ?? string.Empty,
            DisplayName = displayName,
            Email = email,
            Phone = phone,
            Status = status
        };
    }

    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool Status { get; set; } = true;
    public DateTime? LastLoginTime { get; set; }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="oldHash">旧密码哈希</param>
    /// <param name="newHash">新密码哈希</param>
    public void ChangePassword(string oldHash, string newHash)
    {
        if (!Status)
            throw new DomainException("已禁用的账户不允许修改密码", "USER_DISABLED");
        if (string.IsNullOrWhiteSpace(oldHash))
            throw new DomainException("旧密码不能为空", "USER_OLD_PASSWORD_REQUIRED");
        if (string.IsNullOrWhiteSpace(newHash))
            throw new DomainException("新密码不能为空", "USER_NEW_PASSWORD_REQUIRED");
        if (PasswordHash != oldHash)
            throw new DomainException("旧密码不正确", "USER_OLD_PASSWORD_MISMATCH");

        PasswordHash = newHash;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 禁用账户
    /// </summary>
    public void Disable()
    {
        if (!Status)
            throw new DomainException("账户已经处于禁用状态", "USER_ALREADY_DISABLED");

        Status = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 启用账户
    /// </summary>
    public void Enable()
    {
        if (Status)
            throw new DomainException("账户已经处于启用状态", "USER_ALREADY_ENABLED");

        Status = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

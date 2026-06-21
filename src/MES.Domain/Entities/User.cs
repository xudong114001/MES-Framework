namespace MES.Domain.Entities;

public class User : BaseEntity
{
    protected internal User() { }

    public static User Create(string username, string displayName, string? passwordHash = null, string? email = null, string? phone = null, bool status = true)
    {
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
}

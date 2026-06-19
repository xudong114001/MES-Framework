namespace MES.Api.Dtos;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo UserInfo { get; set; } = new();
}

public class UserInfo
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

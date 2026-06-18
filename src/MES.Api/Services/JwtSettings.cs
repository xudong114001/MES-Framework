namespace MES.Api.Services;

public class JwtSettings
{
    public string SecretKey { get; set; } = "MES-SuperSecret-Key-Must-Be-At-Least-32-Characters!";
    public string Issuer { get; set; } = "MES.Api";
    public string Audience { get; set; } = "MES.Client";
    public int ExpireHours { get; set; } = 8;
}

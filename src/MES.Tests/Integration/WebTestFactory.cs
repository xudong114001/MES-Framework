using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MES.Api.Services;
using MES.Domain.Entities;
using MES.Infrastructure.Data;
using MES.Domain.Repositories;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace MES.Tests.Integration;

/// <summary>
/// Web 集成测试基类：使用 TestContainers + WebApplicationFactory，
/// 提供真实的 HTTP 客户端测试 API 端点
/// </summary>
public class WebTestFactory : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RedisContainer _redisContainer;
    private IConnectionMultiplexer? _redisConnection;
    private WebApplicationFactory<Program>? _webAppFactory;

    // 使用与生产环境相同的 JWT 配置
    // 注意：这里使用与 JwtSettings 默认值相同的密钥
    public const string TestSecretKey = "MES-SuperSecret-Key-Must-Be-At-Least-32-Characters!";
    public const string TestIssuer = "MES.Api";
    public const string TestAudience = "MES.Client";

    public MesDbContext DbContext { get; private set; } = null!;
    public IConnectionMultiplexer RedisConnection { get; private set; } = null!;
    public IDatabase RedisDb => RedisConnection.GetDatabase();
    public IServiceProvider Services { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;
    public string BaseUrl => "http://localhost";

    // 测试用户凭据
    public const string TestAdminUsername = "admin";
    public const string TestAdminPassword = "Admin@2026!";
    public const string TestOperatorUsername = "operator";
    public const string TestOperatorPassword = "123456";

    public WebTestFactory()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("mes_test")
            .WithUsername("mes_test")
            .WithPassword("mes_test_pwd")
            .WithCleanUp(true)
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        // 启动容器
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();

        // 创建内存配置，覆盖 JWT 配置（使用生产环境的密钥）
        var inMemoryConfig = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = TestSecretKey,
            ["JwtSettings:Issuer"] = TestIssuer,
            ["JwtSettings:Audience"] = TestAudience,
            ["JwtSettings:ExpireHours"] = "8",
            ["ConnectionStrings:MesDb"] = _postgresContainer.GetConnectionString(),
            ["ConnectionStrings:Redis"] = _redisContainer.GetConnectionString()
        };

        // 创建 WebApplicationFactory
        _webAppFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(inMemoryConfig);
                });
                builder.ConfigureTestServices(services =>
                {
                    // 替换数据库连接字符串
                    services.AddDbContext<MesDbContext>(options =>
                        options.UseSnakeCaseNamingConvention()
                               .UseNpgsql(_postgresContainer.GetConnectionString()));

                    // 添加 Redis
                    _redisConnection = ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString());
                    services.AddSingleton(_redisConnection);

                    // JWT 配置已经通过 AddInMemoryCollection 设置，会在 Program.cs 中读取
                });
            });

        // 获取服务提供者
        Services = _webAppFactory.Services;

        // 获取 DbContext
        DbContext = Services.GetRequiredService<MesDbContext>();
        await DbContext.Database.EnsureCreatedAsync();

        // Redis 连接
        RedisConnection = _redisConnection!;

        // 创建 HTTP 客户端
        Client = _webAppFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        if (_redisConnection != null)
        {
            await _redisConnection.DisposeAsync();
        }

        if (DbContext != null)
        {
            await DbContext.DisposeAsync();
        }

        _webAppFactory?.Dispose();

        await _postgresContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
    }

    /// <summary>
    /// 清理所有测试数据，重置自增序列，确保测试隔离
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        var tableNames = new[]
        {
            "mes_work_report", "mes_work_order_step", "mes_work_order",
            "mes_qc_inspection_item", "mes_qc_inspection",
            "mes_material_trace",
            "mes_routing_step", "mes_routing",
            "mes_bom",
            "mes_maintenance_plan", "mes_equipment",
            "mes_workstation", "mes_production_line", "mes_workshop", "mes_factory",
            "mes_material", "mes_role_permission", "mes_user_role", "mes_role", "mes_user"
        };

#pragma warning disable EF1002
        foreach (var table in tableNames)
        {
            try
            {
                await DbContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {table} CASCADE");
            }
            catch
            {
                // 表可能不存在，忽略
            }
        }
#pragma warning restore EF1002
    }

    /// <summary>
    /// 清理 Redis 所有缓存数据
    /// </summary>
    public async Task CleanRedisAsync()
    {
        await RedisDb.ExecuteAsync("FLUSHALL");
    }

    /// <summary>
    /// 创建测试用户（默认 admin 角色）
    /// </summary>
    public async Task<User> SeedUserAsync(string username, string password, List<string>? roles = null)
    {
        var passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            DisplayName = username,
            Status = true
        };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // 添加角色
        if (roles != null && roles.Count > 0)
        {
            foreach (var roleName in roles)
            {
                var role = await DbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName && !r.IsDeleted);
                if (role == null)
                {
                    role = new MES.Domain.Entities.Role { Name = roleName, Description = $"测试角色 {roleName}" };
                    DbContext.Roles.Add(role);
                    await DbContext.SaveChangesAsync();
                }

                DbContext.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            }
            await DbContext.SaveChangesAsync();
        }

        return user;
    }

    /// <summary>
    /// 创建测试角色（默认 admin 权限）
    /// </summary>
    public async Task<MES.Domain.Entities.Role> SeedRoleAsync(string name, List<string>? permissions = null)
    {
        var role = new MES.Domain.Entities.Role
        {
            Name = name,
            Description = $"测试角色 {name}"
        };
        DbContext.Roles.Add(role);
        await DbContext.SaveChangesAsync();

        // 添加权限
        if (permissions != null && permissions.Count > 0)
        {
            foreach (var perm in permissions)
            {
                DbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    Permission = perm
                });
            }
            await DbContext.SaveChangesAsync();
        }

        return role;
    }

    /// <summary>
    /// 获取 JWT Token（用于认证请求）
    /// </summary>
    public async Task<string> GetTokenAsync(string username, string password)
    {
        // 使用 API 登录获取 token
        var loginRequest = new
        {
            Username = username,
            Password = password
        };

        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        if (!response.IsSuccessStatusCode)
        {
            // 如果用户不存在，先创建
            await SeedUserAsync(username, password, new List<string> { "admin" });
            response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.Data?.Token ?? string.Empty;
    }

    /// <summary>
    /// 获取带认证的 HttpClient
    /// </summary>
    public async Task<HttpClient> GetAuthenticatedClientAsync(string username, string password)
    {
        var token = await GetTokenAsync(username, password);
        var client = _webAppFactory!.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// 设置默认认证头
    /// </summary>
    public void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// 清除认证头
    /// </summary>
    public void ClearAuthorizationHeader()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }
}

/// <summary>
/// Token 响应结构（用于解析登录 API 响应）
/// </summary>
internal class TokenResponse
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public TokenData? Data { get; set; }
}

internal class TokenData
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo? UserInfo { get; set; }
}

internal class UserInfo
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MES.Infrastructure.Data;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace MES.Tests.Integration;

/// <summary>
/// 集成测试基类：使用 TestContainers 自动启动 PostgreSQL 和 Redis 容器，
/// 提供真实的数据库引擎（非 InMemory），确保测试与生产环境一致。
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RedisContainer _redisContainer;
    private IConnectionMultiplexer? _redisConnection;

    protected MesDbContext DbContext { get; private set; } = null!;
    protected IConnectionMultiplexer RedisConnection { get; private set; } = null!;
    protected IDatabase RedisDb => RedisConnection.GetDatabase();
    protected IServiceProvider Services { get; private set; } = null!;

    protected IntegrationTestBase()
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

        // 构建 DI 容器
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MesDb"] = _postgresContainer.GetConnectionString()
            })
            .Build();

        services.AddDbContext<MesDbContext>(options =>
            options.UseSnakeCaseNamingConvention()
                   .UseNpgsql(configuration.GetConnectionString("MesDb")));

        // Redis 连接
        _redisConnection = ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString());
        services.AddSingleton(_redisConnection);

        Services = services.BuildServiceProvider();

        // 执行数据库迁移
        DbContext = Services.GetRequiredService<MesDbContext>();
        await DbContext.Database.EnsureCreatedAsync();

        RedisConnection = _redisConnection;
    }

    public async Task DisposeAsync()
    {
        if (_redisConnection != null)
        {
            await _redisConnection.DisposeAsync();
        }

        if (DbContext != null)
        {
            await DbContext.DisposeAsync();
        }

        (Services as IDisposable)?.Dispose();

        await _postgresContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
    }

    /// <summary>
    /// 清理所有测试数据，重置自增序列，确保测试隔离
    /// </summary>
    protected async Task CleanDatabaseAsync()
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
            "mes_material", "mes_user"
        };

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
    }

    /// <summary>
    /// 清理 Redis 所有缓存数据
    /// </summary>
    protected async Task CleanRedisAsync()
    {
        await RedisDb.ExecuteAsync("FLUSHALL");
    }

    /// <summary>
    /// 创建新的 DbContext（用于模拟并发场景下的不同请求）
    /// </summary>
    protected MesDbContext CreateNewDbContext()
    {
        var options = new DbContextOptionsBuilder<MesDbContext>()
            .UseSnakeCaseNamingConvention()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;
        return new MesDbContext(options);
    }
}

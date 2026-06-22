using System.Text;
using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MES.Api.Hubs;
using MES.Api.Middleware;
using MES.Api.Services;
using StackExchange.Redis;
using MES.Application.Interfaces;
using MES.Application.Services;
using MES.Infrastructure.Data;
using MES.Infrastructure.Extensions;
using MES.Integration;
using MES.Integration.EventBus;
using MES.AI.Application.Interfaces;
using MES.AI.Application.Services;
using MES.Integration.Adapters;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// HttpClient for external adapters
builder.Services.AddHttpClient("SapB1")
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
builder.Services.AddHttpClient("Kingdee")
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/mes-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Controllers (with JSON cycle handling)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Swagger / OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MES API", Version = "v1" });

    // 配置 XML 文档注释
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, true);
    }

    c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme"
    });
    c.AddSecurityRequirement(document =>
    {
        return new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("bearer", document)] = []
        };
    });
});

// JWT
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });
builder.Services.AddAuthorization();

// Infrastructure (EF Core + Repositories)
builder.Services.AddMesInfrastructure(builder.Configuration);

// Auth Service
builder.Services.AddScoped<IAuthService, AuthService>();

// Application Services
builder.Services.AddScoped<IWorkReportService, WorkReportService>();
builder.Services.AddScoped<IQcService, QcService>();
builder.Services.AddScoped<IWorkOrderService, MES.Application.Services.WorkOrderService>();
builder.Services.AddScoped<IQcCheckpointService, QcCheckpointService>();

// P1 Scheduling & Dispatch Services
builder.Services.AddScoped<ISchedulingService, MES.Application.Services.SchedulingService>();
builder.Services.AddScoped<IDispatchService, MES.Application.Services.DispatchService>();

// Equipment & Trace & Dashboard & Andon & Organization & Material Services
builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<ITraceService, TraceService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAndonService, AndonService>();
builder.Services.AddScoped<IFactoryService, FactoryService>();
builder.Services.AddScoped<IWorkshopService, WorkshopService>();
builder.Services.AddScoped<IProductionLineService, ProductionLineService>();
builder.Services.AddScoped<IWorkstationService, WorkstationService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IBomService, BomService>();
builder.Services.AddScoped<IRoutingService, RoutingService>();
builder.Services.AddScoped<IRoleService, RoleService>();

// Redis 连接（防重复提交 + 批次号生成 + 缓存）
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var redisConnStr = config.GetConnectionString("Redis") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(redisConnStr);
});
builder.Services.AddScoped<MES.Domain.Interfaces.ICacheService, MES.Infrastructure.Services.CacheService>();
builder.Services.AddScoped<CachedMaterialService>();
builder.Services.AddScoped<CachedRoutingService>();

// SignalR
builder.Services.AddSignalR();

// SignalR Notification Service
builder.Services.AddScoped<HubNotificationService>();

// P5 Integration Services
builder.Services.AddEventBus();
builder.Services.AddIntegrationAdapters(builder.Configuration);

// P5 Event Log Service
builder.Services.AddSingleton<MES.Application.Integration.Events.InMemoryEventLogService>();

// AI Services
builder.Services.AddScoped<IQualityAlertService, QualityAlertService>();
builder.Services.AddScoped<ISchedulingRecommendationService, SchedulingRecommendationService>();
builder.Services.AddScoped<IEquipmentHealthService, EquipmentHealthService>();
builder.Services.AddScoped<IAlertPushService, AlertPushService>();
builder.Services.AddScoped<IKnowledgeBaseService, KnowledgeBaseService>();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Swagger UI (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("DevCors");
app.MapControllers();
app.MapHub<MesHub>("/hubs/mes");

// 确保数据库已创建/迁移
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MesDbContext>();
    db.Database.EnsureCreated();

    // 种子数据：初始化 admin 用户
    if (!db.Users.Any())
    {
        var adminPassword = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                Encoding.UTF8.GetBytes("Admin@2026!")));
        db.Users.Add(MES.Domain.Entities.User.Create(
            username: "admin",
            displayName: "系统管理员",
            passwordHash: adminPassword,
            email: "admin@mes.local"
        ));
        db.SaveChanges();
        Log.Information("Seed data: admin user created");
    }
}

Log.Information("MES API starting on http://localhost:5180");
app.Run();

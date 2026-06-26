using System.Text;
using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MES.Api.Hubs;
using MES.Api.Middleware;
using MES.Api.Services;
using StackExchange.Redis;
using MES.Infrastructure.Data;
using MES.Infrastructure.Extensions;
using MES.Infrastructure.Services;
using MES.Api.Extensions;
using MES.Application.Interfaces;
using MES.Integration;
using MES.Integration.EventBus;
using MES.Integration.Adapters;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
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
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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

// Application Services (统一注册)
builder.Services.AddApplicationServices();

// Redis 连接（防重复提交 + 批次号生成 + 缓存）
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var redisConnStr = config.GetConnectionString("Redis") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(redisConnStr);
});
builder.Services.AddScoped<MES.Application.Interfaces.ICacheService, MES.Infrastructure.Services.CacheService>();
builder.Services.AddScoped<IBatchNumberService, BatchNumberService>();

// SignalR
builder.Services.AddSignalR();

// ForwardedHeaders 配置（nginx 代理场景）
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                               Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Health Checks
builder.Services.AddHealthChecks();

// P5 Integration Services
builder.Services.AddEventBus();
builder.Services.AddIntegrationAdapters(builder.Configuration);

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Forwarded Headers（nginx 代理后正确获取客户端 IP）
app.UseForwardedHeaders();

// HTTPS 重定向（生产环境）
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

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
app.MapHealthChecks("/health");

// 确保数据库已迁移
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MesDbContext>();
    db.Database.Migrate();

    // 种子数据：初始化 admin 用户
    if (!db.Users.Any())
    {
        var adminPassword = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                Encoding.UTF8.GetBytes("Admin@2026!")));
        var adminUser = MES.Domain.Entities.User.Create(
            username: "admin",
            displayName: "系统管理员",
            passwordHash: adminPassword,
            email: "admin@mes.local"
        );
        db.Users.Add(adminUser);
        db.SaveChanges();

        // 确保 Admin 角色存在并分配给 admin 用户
        var adminRole = db.Roles.FirstOrDefault(r => r.Name == "Admin");
        if (adminRole != null)
        {
            db.UserRoles.Add(new MES.Domain.Entities.UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            });
            db.SaveChanges();
        }

        Log.Information("Seed data: admin user created with Admin role");
    }
}

Log.Information("MES API starting on http://localhost:5180");
app.Run();

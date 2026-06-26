using MES.Application.Interfaces;
using MES.Application.Services;
using MES.Application.Integration.Events;
using MES.Api.Services;

namespace MES.Api.Extensions;

/// <summary>
/// 应用服务 DI 注册扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Auth（Api 层服务）
        services.AddScoped<IAuthService, AuthService>();

        // SignalR Notification Service
        services.AddScoped<HubNotificationService>();

        // Core Business
        services.AddScoped<IWorkReportService, WorkReportService>();
        services.AddScoped<IQcService, QcService>();
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        services.AddScoped<IQcCheckpointService, QcCheckpointService>();

        // Scheduling & Dispatch
        services.AddScoped<ISchedulingService, SchedulingService>();
        services.AddScoped<IDispatchService, DispatchService>();

        // Equipment & Trace & Dashboard & Andon
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<ITraceService, TraceService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAndonService, AndonService>();

        // Organization & Material
        services.AddScoped<IFactoryService, FactoryService>();
        services.AddScoped<IWorkshopService, WorkshopService>();
        services.AddScoped<IProductionLineService, ProductionLineService>();
        services.AddScoped<IWorkstationService, WorkstationService>();
        services.AddScoped<IMaterialService, MaterialService>();
        services.AddScoped<IBomService, BomService>();
        services.AddScoped<IRoutingService, RoutingService>();

        // RBAC
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();

        // Event Log
        services.AddScoped<InMemoryEventLogService>();

        // AI Services
        services.AddScoped<IQualityAlertService, QualityAlertService>();
        services.AddScoped<ISchedulingRecommendationService, SchedulingRecommendationService>();
        services.AddScoped<IEquipmentHealthService, EquipmentHealthService>();
        services.AddScoped<IAlertPushService, AlertPushService>();
        services.AddScoped<IKnowledgeBaseService, KnowledgeBaseService>();

        return services;
    }
}

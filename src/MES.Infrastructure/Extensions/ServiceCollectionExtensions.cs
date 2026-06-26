using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MES.Domain.Repositories;
using MES.Infrastructure.Data;
using MES.Infrastructure.Repositories;

namespace MES.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MesDbContext>(options =>
            options.UseSnakeCaseNamingConvention()
                   .UseNpgsql(configuration.GetConnectionString("MesDb")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoutingRepository, RoutingRepository>();
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IMaterialRepository, MaterialRepository>();
        services.AddScoped<IQcInspectionRepository, QcInspectionRepository>();
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();

        return services;
    }
}

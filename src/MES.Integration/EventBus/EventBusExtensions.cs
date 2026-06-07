using Microsoft.Extensions.DependencyInjection;
using MES.Integration.Adapters;
using MES.Integration.EventBus;
using MES.Integration.Plc;

namespace MES.Integration;

public static class EventBusExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, RabbitMQEventBus>();
        services.AddSingleton<SubscriptionManager>();
        return services;
    }
}

public static class AdapterExtensions
{
    public static IServiceCollection AddIntegrationAdapters(this IServiceCollection services)
    {
        services.AddSingleton<IERPAdapter, MockERPAdapter>();
        services.AddSingleton<IWMSAdapter, MockWMSAdapter>();
        services.AddSingleton<IPlcCollector, MockPlcCollector>();
        return services;
    }
}
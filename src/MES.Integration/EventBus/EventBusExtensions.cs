using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    public static IServiceCollection AddIntegrationAdapters(this IServiceCollection services, IConfiguration? configuration = null)
    {
        var plcType = configuration?["Integration:Plc:Type"] ?? "Mock";
        var erpType = configuration?["Integration:ERP:Type"] ?? "Mock";

        switch (erpType)
        {
            case "SapB1":
                services.AddSingleton<IERPAdapter>(sp =>
                {
                    var config = configuration?.GetSection("Integration:ERP:SapB1");
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("SapB1");
                    var logger = sp.GetRequiredService<ILogger<SapB1Adapter>>();
                    return new SapB1Adapter(
                        httpClient,
                        config?["ServerUrl"] ?? "https://localhost:50000",
                        config?["CompanyDb"] ?? "SBO_DEMO",
                        config?["UserName"] ?? "manager",
                        config?["Password"] ?? "",
                        logger);
                });
                break;
            case "Kingdee":
                services.AddSingleton<IERPAdapter>(sp =>
                {
                    var config = configuration?.GetSection("Integration:ERP:Kingdee");
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("Kingdee");
                    var logger = sp.GetRequiredService<ILogger<KingdeeAdapter>>();
                    return new KingdeeAdapter(
                        httpClient,
                        config?["ServerUrl"] ?? "https://localhost:8080",
                        config?["DbName"] ?? "demo",
                        config?["AppId"] ?? "",
                        config?["AppSecret"] ?? "",
                        logger);
                });
                break;
            case "Mock":
            default:
                services.AddSingleton<IERPAdapter, MockERPAdapter>();
                break;
        }

        services.AddSingleton<IWMSAdapter, MockWMSAdapter>();

        switch (plcType)
        {
            case "Modbus":
                services.AddSingleton<IPlcCollector>(sp =>
                {
                    var config = configuration?.GetSection("Integration:Plc:Modbus") ?? configuration?.GetSection("Integration:Plc");
                    var deviceName = config?["DeviceName"] ?? "ModbusDevice";
                    var ipAddress = config?["IpAddress"] ?? "127.0.0.1";
                    var portStr = config?["Port"] ?? "502";
                    var port = int.TryParse(portStr, out var parsedPort) ? parsedPort : 502;
                    var logger = sp.GetRequiredService<ILogger<ModbusTcpCollector>>();
                    return new ModbusTcpCollector(deviceName, ipAddress, port, logger);
                });
                break;
            case "Mock":
            default:
                services.AddSingleton<IPlcCollector, MockPlcCollector>();
                break;
        }

        return services;
    }
}
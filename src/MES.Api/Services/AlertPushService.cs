using Microsoft.AspNetCore.SignalR;
using MES.Api.Hubs;
using Microsoft.Extensions.Logging;

namespace MES.Api.Services;

public interface IAlertPushService
{
    Task PushNewAlertAsync(AlertNotificationDto alert);
}

public class AlertPushService : IAlertPushService
{
    private readonly IHubContext<MesHub> _hubContext;
    private readonly ILogger<AlertPushService> _logger;

    public AlertPushService(
        IHubContext<MesHub> hubContext,
        ILogger<AlertPushService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PushNewAlertAsync(AlertNotificationDto alert)
    {
        try
        {
            await _hubContext.Clients.Group("ai-alerts").SendAsync("OnNewAlert", alert);
            _logger.LogInformation("AI预警已推送到客户端: {Title}", alert.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "推送AI预警失败: {Title}", alert.Title);
        }
    }
}
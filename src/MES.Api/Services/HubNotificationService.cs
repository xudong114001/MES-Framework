using Microsoft.AspNetCore.SignalR;
using MES.Api.Hubs;

namespace MES.Api.Services;

/// <summary>
/// SignalR 消息推送服务
/// 通过 IHubContext to MesHub 向客户端推送实时消息
/// </summary>
public class HubNotificationService
{
    private readonly IHubContext<MesHub> _hubContext;

    public HubNotificationService(IHubContext<MesHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// 推送工单状态更新到相关组
    /// </summary>
    public async Task NotifyOrderUpdate(object orderData)
    {
        // 推送到 dashboard 组
        await _hubContext.Clients.Group("dashboard").SendAsync("onOrderUpdate", orderData);
    }

    /// <summary>
    /// 推送产量更新到 "dashboard" 组
    /// </summary>
    public async Task NotifyOutputUpdate(object outputData)
    {
        await _hubContext.Clients.Group("dashboard").SendAsync("onOutputUpdate", outputData);
    }

    /// <summary>
    /// 推送 Andon 异常事件到相关组
    /// </summary>
    public async Task NotifyAndonEvent(object andonEvent)
    {
        // 推送到 dashboard 组和全体组
        await _hubContext.Clients.Group("dashboard").SendAsync("onAndonEvent", andonEvent);
        await _hubContext.Clients.All.SendAsync("onAndonEvent", andonEvent);
    }

    /// <summary>
    /// 推送 OEE 更新到 dashboard 组
    /// </summary>
    public async Task NotifyOeeUpdate(object oeeData)
    {
        await _hubContext.Clients.Group("dashboard").SendAsync("onOeeUpdate", oeeData);
    }
}

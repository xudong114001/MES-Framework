using Microsoft.AspNetCore.SignalR;

namespace MES.Api.Hubs;

public class MesHub : Hub
{
    /// <summary>
    /// 客户端加入组（如 "line-1", "dashboard"）
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("JoinedGroup", new { group = groupName });
    }

    /// <summary>
    /// 离开组
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("LeftGroup", new { group = groupName });
    }
}

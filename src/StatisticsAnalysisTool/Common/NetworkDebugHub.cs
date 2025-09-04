using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

public class NetworkDebugHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        System.Diagnostics.Debug.WriteLine($"Cliente {Context.ConnectionId} entrou no grupo {groupName}");
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        System.Diagnostics.Debug.WriteLine($"Cliente {Context.ConnectionId} saiu do grupo {groupName}");
    }

    public override async Task OnConnectedAsync()
    {
        System.Diagnostics.Debug.WriteLine($"Cliente conectado: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        System.Diagnostics.Debug.WriteLine($"Cliente desconectado: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}

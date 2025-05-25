using Microsoft.AspNetCore.SignalR;
using System.Net.Sockets;

namespace TrickFarmWebApp.Hubs;

public class ChatHub : Hub
{
    public async Task ConnectToTrickFarm()
    {
        var connectionId = Context.ConnectionId;
        if (false == GlobalClientManager.ClientConnections.ContainsKey(connectionId))
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("trickfarm-orleans.koreacentral.cloudapp.azure.com", 5000);

                var cts = new CancellationTokenSource();
                var state = new ClientConnection
                {
                    network_socket = tcpClient,
                    CancellationTokenSource = cts,
                    connectionId = connectionId,
                };

                GlobalClientManager.ClientConnections[connectionId] = state;
                Console.WriteLine($"[ChatHub] connectionId: {connectionId}, ClientConnections에 추가 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR][ChatHub] connectionId: {connectionId} failed: {ex.Message}");
            }
        }
    }

    public async Task LoginToServer(string user_name)
    {
        var connectionId = Context.ConnectionId;
        if (GlobalClientManager.ClientConnections.ContainsKey(connectionId))
        {
            Console.WriteLine($"[{connectionId}][Log] _{user_name}_으로 로그인을 시도합니다.");
            await GlobalClientManager.LoginToServer(connectionId, user_name);
        }
        else
        {
            Console.WriteLine($"[Log] _{user_name}_으로 로그인을 시도했지만 등록되지 않은 유저.");
        }
    }

    public async Task LogoutToServer(string user_name)
    {
        var connectionId = Context.ConnectionId;
        if (GlobalClientManager.ClientConnections.ContainsKey(connectionId))
        {
            await GlobalClientManager.LogoutToServer(connectionId, user_name);
        }
    }

    public async Task DisconnectToServer()
    {
        var connectionId = Context.ConnectionId;
        if (GlobalClientManager.ClientConnections.TryGetValue(connectionId, out var client_connection))
        {
            client_connection.network_socket.Dispose();
        }
    }

    public async Task JoinToChatrrom(string chatroom_name)
    {
        var connectionId = Context.ConnectionId;
        if(GlobalClientManager.ClientConnections.ContainsKey(connectionId))
        {
            await GlobalClientManager.JoinToChatrrom(connectionId, chatroom_name);
        }
    }

    public async Task LeaveToChatroom(string chatroom_name)
    {
        var connectionId = Context.ConnectionId;
        if (GlobalClientManager.ClientConnections.ContainsKey(connectionId))
        {
            await GlobalClientManager.LeaveToChatroom(connectionId, chatroom_name);
        }
    }

    public async Task SendToServer(string message)
    {
        var connectionId = Context.ConnectionId;
        if (GlobalClientManager.ClientConnections.ContainsKey(connectionId))
        {
            await GlobalClientManager.SendToServer(connectionId, message);
        }
    }
}
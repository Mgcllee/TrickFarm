using Microsoft.AspNetCore.SignalR;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace TrickFarmWebApp.Hubs;

public class ChatHub : Hub
{
    private static readonly Dictionary<string, TcpClient> TcpConnections = new();
    private static readonly Dictionary<string, ConnectionState> Connections = new();

    public async Task ConnectToTrickFarm()
    {
        var connectionId = Context.ConnectionId;

        if (TcpConnections.ContainsKey(connectionId))
        {
            return;
        }
        else
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("trickfarm-orleans.koreacentral.cloudapp.azure.com", 5000);
                TcpConnections[connectionId] = tcpClient;

                var cts = new CancellationTokenSource();
                var state = new ConnectionState
                {
                    TcpClient = tcpClient,
                    CancellationTokenSource = cts
                };

                state.ReceiveTask = Task.Run(() => ReceiveLoop(connectionId, tcpClient, cts.Token));
                Connections[connectionId] = state;
            }
            catch(Exception ex)
            {
                Console.WriteLine("TrickFarmWebApp에서 ConnectToTrickFarm 메서드 속 TcpClient 접속 실패");
                Console.WriteLine($"오류 발생: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("TrickFarmWebApp에서 ConnectToTrickFarm 메서드 종료");
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        if (Connections.TryGetValue(connectionId, out var state))
        {
            state.CancellationTokenSource.Cancel();

            try
            {
                await state.ReceiveTask!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{connectionId}] 수신 루프 종료 중 예외: {ex.Message}");
            }
            finally
            {
                state.TcpClient.Close();
                Connections.Remove(connectionId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task LoginToServer(string message)
    {
        var connectionId = Context.ConnectionId;

        if (TcpConnections.TryGetValue(connectionId, out var client))
        {
            var stream = client.GetStream();
            var buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            await Clients.Client(connectionId)
                .SendAsync("ReceiveFromServer", $"{message}님 접속 성공!");
        }
    }

    public async Task JoinToChatrrom(string request)
    {
        var connectionId = Context.ConnectionId;

        if (TcpConnections.TryGetValue(connectionId, out var client))
        {
            var stream = client.GetStream();
            var buffer = Encoding.UTF8.GetBytes(request);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            await Clients.Client(connectionId)
                .SendAsync("ReceiveFromServer", $"{request.Replace("join ", "")}방에 어서오세요!");
        }
    }

    public async Task SendToServer(string message)
    {
        var connectionId = Context.ConnectionId;

        if (TcpConnections.TryGetValue(connectionId, out var client))
        {
            var stream = client.GetStream();
            var buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }

    public async Task LogoutToServer()
    {
        var connectionId = Context.ConnectionId;

        if(TcpConnections.TryGetValue(connectionId, out var client))
        {
            var stream = client.GetStream();
            var buffer = Encoding.UTF8.GetBytes("leave");
            await stream.WriteAsync(buffer, 0, buffer.Length);
            client.Dispose();
            Console.WriteLine($"{connectionId}님이 나가셨습니다.");
        }
    }

    private async Task ReceiveLoop(string ConnectionId, TcpClient client, CancellationToken token)
    {
        if (client is null || client.GetStream() is null) return;

        var stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (!token.IsCancellationRequested)
            {
                int recv_len = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                if (recv_len == 0) break;

                string response = Encoding.UTF8.GetString(buffer, 0, recv_len);

                if (GlobalHubContext.HubContext != null)
                {
                    await GlobalHubContext.HubContext.Clients.Client(ConnectionId).SendAsync("ReceiveFromServer", response);
                }
            }
        }
        catch (Exception ex)
        {
            client.Dispose();
            Console.WriteLine($"[Log][ReceiveLoop] {ex.Message}");
        }
    }
}
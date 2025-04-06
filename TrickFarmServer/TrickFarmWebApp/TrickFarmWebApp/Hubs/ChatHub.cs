using Microsoft.AspNetCore.SignalR;
using System.Net.Sockets;
using System.Text;

namespace TrickFarmWebApp.Hubs;

public class ChatHub : Hub
{
    private static readonly Dictionary<string, TcpClient> TcpConnections = new();

    public async Task ConnectToTrickFarm()
    {
        var connectionId = Context.ConnectionId;

        // 이미 연결된 경우 무시
        if (TcpConnections.ContainsKey(connectionId))
            return;

        TcpClient tcpClient = new TcpClient();
        await tcpClient.ConnectAsync("127.0.0.1", 5000); // TrickFarmServer 주소와 포트

        TcpConnections[connectionId] = tcpClient;

        _ = Task.Run(() => ListenToServer(tcpClient, connectionId)); // 서버로부터 수신 대기
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        if (TcpConnections.TryGetValue(connectionId, out var client))
        {
            client.Close();
            TcpConnections.Remove(connectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task ListenToServer(TcpClient client, string connectionId)
    {
        var stream = client.GetStream();
        var buffer = new byte[1024];

        while (client.Connected)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
                break;

            var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            await Clients.Client(connectionId).SendAsync("ReceiveFromServer", message);
        }

        client.Close();
        TcpConnections.Remove(connectionId);
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
}
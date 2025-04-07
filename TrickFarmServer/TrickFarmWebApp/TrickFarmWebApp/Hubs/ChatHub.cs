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

    public async Task LoginToServer(string message)
    {
        var connectionId = Context.ConnectionId;
        if (TcpConnections.TryGetValue(connectionId, out var client))
        {
            var stream = client.GetStream();
            var buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }

    public async Task JoinToChatrrom(string request)
    {
        Console.WriteLine($"JoinToChatrrom: {request} ->> ");
        var connectionId = Context.ConnectionId;
        if (TcpConnections.TryGetValue(connectionId, out var client))
        {
            var stream = client.GetStream();
            var buffer = Encoding.UTF8.GetBytes(request);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }

    public async Task SendToServer(string message)
    {
        var connectionId = Context.ConnectionId;

        Console.WriteLine($"서버로 보낼 내용: {message} ->> ");
        if (TcpConnections.TryGetValue(connectionId, out var client))
        {
            var stream = client.GetStream();
            var buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);

            Console.WriteLine($"서버로 보낸 내용: {message}");

            // 서버로부터 응답을 받는 부분
            byte[] responseBuffer = new byte[1024];
            int recv_len = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.UTF8.GetString(responseBuffer, 0, recv_len);

            Console.WriteLine($"서버에서 받은 내용: {response}");

            await Clients.All.SendAsync("ReceiveFromServer", response);
        }
    }
}
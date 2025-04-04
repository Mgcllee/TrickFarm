using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    public string SendMessageToTcpServer(string message)
    {
        return $"서버로부터 응답: {message.ToUpper()}";
    }

    public async Task BroadcastMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}

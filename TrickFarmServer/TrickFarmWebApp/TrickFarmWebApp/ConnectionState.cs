using Microsoft.AspNetCore.SignalR;
using System.Net.Sockets;

public class ConnectionState
{
    public TcpClient TcpClient { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; set; }
    public Task? ReceiveTask { get; set; }
}

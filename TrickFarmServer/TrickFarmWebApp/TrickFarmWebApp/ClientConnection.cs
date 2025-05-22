using Microsoft.AspNetCore.SignalR;
using System.Net.Sockets;

public class ClientConnection
{
    public TcpClient TcpClient { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; set; }
}

using Microsoft.AspNetCore.SignalR;
using System.Net.Sockets;

public class ClientConnection
{
    public TcpClient network_socket { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; set; }
    public string connectionId { get; set; }
}

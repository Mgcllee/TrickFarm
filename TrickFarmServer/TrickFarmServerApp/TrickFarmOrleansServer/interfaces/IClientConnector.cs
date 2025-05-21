using System.Net.Sockets;

public interface IClientConnector
{
    Task add_gclient(TcpClient client_socket);
    
    Task disconnect_all_clients();
}
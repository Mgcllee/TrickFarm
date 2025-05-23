using System.Net.Sockets;

public interface IClientConnector
{
    
    Task disconnect_all_clients();
}
using System.Net.Sockets;

public interface IClientConnector
{
    Guid add_client_connector(TcpClient client_socket);
    TcpClient? get_client_connector(Guid user_guid);
    void disconnect_all_clients();
}
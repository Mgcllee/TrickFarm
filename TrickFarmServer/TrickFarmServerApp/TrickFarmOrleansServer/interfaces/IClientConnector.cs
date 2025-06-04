using System.Net.Sockets;

public interface IClientConnector
{
    Task start_client_accepter();
    Task check_exist_client();
    Task discoonect_client(Guid user_guid);
    Task disconnect_all_clients();

    void stop_server_accepter();
}
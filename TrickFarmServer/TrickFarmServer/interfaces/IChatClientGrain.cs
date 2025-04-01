using System.Net.Sockets;

public interface IChatClientGrain : IGrainWithGuidKey
{
    Task set_tcp_client(TcpClient tcpClient);
    Task recv_client_message(string message);
    Task packet_worker();
    Task leave_client();
    Task join_chat_room(string chatroom_name);
    Task send_to_client(string message);
}
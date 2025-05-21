using System.Net.Sockets;

public interface IChatClientGrain : IGrainWithGuidKey
{
    Task process_packet(string message);
    Task leave_client();
    Task join_chat_room(string chatroom_name);
}
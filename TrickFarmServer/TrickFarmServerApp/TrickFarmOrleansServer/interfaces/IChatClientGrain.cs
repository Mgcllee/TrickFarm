using System.Net.Sockets;

public interface IChatClientGrain : IGrainWithGuidKey
{
    Task enter_chat_message(string message);
    Task logout_client();
    Task join_chatroom(string chatroom_name);
    Task leave_chatroom();
}
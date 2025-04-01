public interface IChatClient
{
    Task<string> recv_chat_message();
    Task send_chat_message(string message);
}
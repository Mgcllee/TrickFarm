public interface IChatClient
{
    Task<string> recv_client_chat_message();
    Task send_to_client_chat_message(string message);
}

public interface IChatRoomGrain : IGrainWithStringKey
{
    Task<bool> join_user(long user_guid, string user_name);
    Task<bool> leave_user(long user_name);
    Task<List<string>> get_chat_log();
    Task broadcast_message(string formatted_message);
}

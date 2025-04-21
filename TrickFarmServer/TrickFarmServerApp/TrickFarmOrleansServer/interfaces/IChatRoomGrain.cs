
public interface IChatRoomGrain : IGrainWithStringKey
{
    Task<bool> join_user(Guid user_guid, string user_name);
    Task<bool> leave_user(Guid user_name);
    Task<List<string>> get_chat_log();
    Task broadcast_message(string formatted_message);
}

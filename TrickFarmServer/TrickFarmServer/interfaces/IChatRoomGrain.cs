
public interface IChatRoomGrain : IGrainWithStringKey
{
    Task<bool> join_user(string user_name);
    Task<bool> leave_user(string user_name);
    Task<List<string>> get_chat_log();
}

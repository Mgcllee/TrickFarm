
public interface IChatRoomGrain
{
    Task<bool> join_user(string user_guid, IChatClient new_member);
    Task<bool> leave_user(string user_guid);
    Task<List<string>> get_chat_log();
}


public interface IChatRoomGrain : IGrainWithStringKey
{
    Task join_user(Guid user_guid, string user_name);
    Task leave_user(Guid user_guid);
    Task<List<string>> get_chat_log();
    Task broadcast_message(string formatted_message);
}

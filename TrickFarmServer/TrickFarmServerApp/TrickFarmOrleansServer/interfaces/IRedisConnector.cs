public interface IRedisConnector
{
    bool write_user_info(Guid user_guid, string user_name);
    string? get_user_name(Guid user_guid);
    bool delete_user_info(Guid user_guid);
    void disconnect_redis();
}
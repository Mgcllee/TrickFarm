using StackExchange.Redis;

public class RedisConnector : IRedisConnector
{
    private static ConnectionMultiplexer redis = null!;
    public static IDatabase user_db = null!;
    public RedisConnector()
    {
        redis = ConnectionMultiplexer
    .Connect(new ConfigurationOptions { EndPoints = { $"localhost:6379" }, AllowAdmin = true });
        user_db = redis.GetDatabase();
        var pong = user_db.Ping();
        Console.WriteLine($"Redis Connection and PongTime: {pong}");
    }

    public bool write_user_info(Guid user_guid, string user_name)
    {
        return user_db.StringSet(user_guid.ToString(), user_name);
    }
    public string? get_user_name(Guid user_guid)
    {
        return user_db.StringGet(user_guid.ToString());
    }

    public bool delete_user_info(long user_guid)
    {
        if (user_db.KeyExists(user_guid.ToString()))
        {
            user_db.KeyDelete(user_guid.ToString());
            return true;
        }
        else
        {
            return false;
        }
    }

    public void disconnect_redis()
    {
        var server = redis.GetServer("127.0.0.1", 6379);
        server.FlushDatabase();
        redis.Close();
        Console.WriteLine("Redis 종료");
    }
}
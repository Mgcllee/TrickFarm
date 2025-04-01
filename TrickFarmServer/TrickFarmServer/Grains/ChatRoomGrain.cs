using System.Collections.Concurrent;

public class ChatRoomGrain : IChatRoomGrain
{
    private readonly List<string> chat_log = new();
    private readonly ConcurrentDictionary<string, IChatClient> clients
        = new ConcurrentDictionary<string, IChatClient>();

    public Task<bool> join_user(string user_guid, IChatClient new_member)
    {
        if (clients.TryAdd(user_guid, new_member))
        {
            Console.WriteLine($"{user_guid}님이 방에 입장하셨습니다.");
            return Task.FromResult(true);
        }
        else
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> leave_user(string user_guid) 
    { 
        if(clients.TryRemove(user_guid, out var value))
        {
            Console.WriteLine($"{user_guid}님이 방을 떠났습니다.");
            return Task.FromResult(true);
        }
        else
        {
            return Task.FromResult(false);
        }
    }
    public Task<List<string>> get_chat_log()
    {
        return Task.FromResult(chat_log);
    }
}
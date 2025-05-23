using System.Collections.Concurrent;

public class ChatRoomGrain : Grain, IChatRoomGrain
{
    private IGrainFactory grain_factory;
    private ClientConnector client_connector = null!;
    private RedisConnector redis_connector = null!;

    private readonly List<string> chat_log = new();
    private readonly ConcurrentDictionary<Guid, string> room_member
        = new ConcurrentDictionary<Guid, string>();

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"[ChatRoomGrain::OnActivateAsync] {this.GetPrimaryKeyString()} 이름의 채팅방이 생성되었습니다.");
        return Task.CompletedTask;
    }

    public ChatRoomGrain(IGrainFactory grain_factory, ClientConnector client_connector, RedisConnector redis_connector)
    {
        this.client_connector = client_connector;
        this.redis_connector = redis_connector;
        this.grain_factory = grain_factory;
    }

    public async Task join_user(Guid user_guid, string user_name)
    {
        string formatted_message;
        if (room_member.TryAdd(user_guid, user_name))
        {
            formatted_message = $"{user_name}님이 {this.GetPrimaryKeyString()} 방에 입장하셨습니다.";
            if (ClientConnector.clients.TryGetValue(user_guid, out var g_client))
            {
                await g_client.send_to_client_chat_message(formatted_message);
                Console.WriteLine(formatted_message);
            }
            else
            {
                Console.WriteLine($"ClientConnector.clients 컨테이너에 {user_guid}가 없음"
                    + $" -> {ClientConnector.clients.ContainsKey(user_guid)}");
                foreach(var v in ClientConnector.clients)
                {
                    Console.WriteLine(v.Key);
                }
            }
        }
        else
        {
            formatted_message = $"{user_name}님이 {this.GetPrimaryKeyString()} 방에 입장 실패!";
        }
    }

    public Task<List<string>> get_chat_log()
    {
        return Task.FromResult(chat_log);
    }

    public async Task broadcast_message(string formatted_message)
    {
        chat_log.Add(formatted_message);
        foreach (var client in room_member)
        {
            if (ClientConnector.clients.TryGetValue(client.Key, out var g_client))
            {
                await g_client.send_to_client_chat_message(formatted_message);
                Console.WriteLine(formatted_message);
            }
        }
    }

    public async Task leave_user(Guid user_guid) 
    { 
        if(room_member.TryRemove(user_guid, out var value))
        {
            string formatted_message = $"{value}님이 {this.GetPrimaryKeyString()} 방을 떠났습니다.";
            foreach (var client in room_member)
            {
                if (client.Key != user_guid)
                {
                    await ClientConnector.clients[client.Key].send_to_client_chat_message(formatted_message);
                }
            }
            Console.WriteLine(formatted_message);
        }

        if(room_member.Count() == 0)
        {
            DeactivateOnIdle();
        }
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        Console.WriteLine($"채팅룸 {this.GetPrimaryKeyString()} 이 {room_member.Count()}명 이므로 제거되었습니다.");
        return base.OnDeactivateAsync(reason, cancellationToken);
    }
}
using System.Collections.Concurrent;

public class ChatRoomGrain : Grain, IChatRoomGrain
{
    private readonly List<string> chat_log = new();
    private readonly ConcurrentDictionary<Guid, string> clients
        = new ConcurrentDictionary<Guid, string>();

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"[{this.GetPrimaryKey()}] 채팅룸이 생성되었습니다.");
        return Task.CompletedTask;
    }

    public Task<bool> join_user(Guid user_guid, string user_name)
    {
        if (clients.TryAdd(user_guid, user_name))
        {
            Console.WriteLine($"{user_name}님이 방에 입장하셨습니다.");
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

    public Task broadcast_message(string formatted_message)
    {
        chat_log.Add(formatted_message);
        foreach (var client in clients)
        {
            var client_grain = GrainFactory.GetGrain<IChatClientGrain>(client.Key);
            client_grain.send_to_client(formatted_message);
        }
        return Task.CompletedTask;
    }

    public Task<bool> leave_user(Guid user_guid) 
    { 
        if(clients.TryRemove(user_guid, out var value))
        {
            Console.WriteLine($"{value}님이 방을 떠났습니다.");
        }

        if(clients.Count() == 0)
        {
            DeactivateOnIdle();
        }

        return Task.FromResult(false);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        Console.WriteLine($"방 {this.GetPrimaryKey()} 이 제거되었습니다.");
        return base.OnDeactivateAsync(reason, cancellationToken);
    }
}
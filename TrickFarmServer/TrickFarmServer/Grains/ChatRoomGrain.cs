using System.Collections.Concurrent;

public class ChatRoomGrain : Grain, IChatRoomGrain
{
    private readonly List<string> chat_log = new();
    private readonly ConcurrentDictionary<string, bool> clients
        = new ConcurrentDictionary<string, bool>();

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"[{this.GetPrimaryKey()}] 채팅룸이 생성되었습니다.");
        return Task.CompletedTask;
    }

    public Task<bool> join_user(string user_name)
    {
        if (clients.TryAdd(user_name, false))
        {
            if (clients.Count() == 1)
            {
                clients[user_name] = true; // (임시) 방장으로 설정
            }
            Console.WriteLine($"{user_name}님이 방에 입장하셨습니다.");
            return Task.FromResult(true);
        }
        else
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> leave_user(string user_name) 
    { 
        if(clients.TryRemove(user_name, out var value))
        {
            Console.WriteLine($"{user_name}님이 방을 떠났습니다.");
        }

        if(clients.Count() == 0)
        {
            DeactivateOnIdle();
        }

        return Task.FromResult(false);
    }
    public Task<List<string>> get_chat_log()
    {
        return Task.FromResult(chat_log);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        Console.WriteLine($"방 {this.GetPrimaryKey()} 이 제거되었습니다.");
        return base.OnDeactivateAsync(reason, cancellationToken);
    }
}
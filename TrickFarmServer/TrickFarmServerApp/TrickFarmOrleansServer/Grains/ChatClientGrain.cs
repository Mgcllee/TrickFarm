using System.Text;

public class ChatClientGrain : Grain, IChatClientGrain
{
    private IGrainFactory grain_factory;
    private ClientConnector client_connector = null!;
    private RedisConnector redis_connector = null!;

    private long grain_ticket = -1;
    private string user_name = null!;
    private string user_chatroom_name = null!;
    private bool enter_chatroom = false;

    public ChatClientGrain(IGrainFactory grain_factory, ClientConnector client_connector, RedisConnector redis_connector)
    {
        this.client_connector = client_connector;
        this.redis_connector = redis_connector;
        this.grain_factory = grain_factory;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        grain_ticket = this.GetPrimaryKeyLong();
        user_name = redis_connector.get_user_name(grain_ticket)!;
        Console.WriteLine($"ChatClientGrain::OnActivateAsync: {grain_ticket}, {user_name}");
        return Task.CompletedTask;
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        if (redis_connector.delete_user_info(grain_ticket))
        {
            Console.WriteLine($"Grain::OnDeactivateAsync: {grain_ticket}");
        }
        else
        {
            Console.WriteLine($"[Error][OnDeactivateAsync]: {grain_ticket}를 찾을 수 없음");
        }
        return base.OnDeactivateAsync(reason, cancellationToken);
    }

    public async Task process_packet(string message)
    {
        if (message.Length <= 0 || message is null)
        {
            return;
        }

        if (message == "leave")
        {
            await leave_client();
        }
        else if (message.Contains("join ") && false == enter_chatroom)
        {
            user_chatroom_name = message.Split(new[] { ' ' }, 2)[1];
            var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(user_chatroom_name);
            await chatroom_grain.join_user(grain_ticket, user_name);
            await join_chat_room(user_chatroom_name);
            enter_chatroom = true;
        }
        else if (enter_chatroom)
        {
            var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(user_chatroom_name);
            await chatroom_grain.broadcast_message($"[{user_chatroom_name}][{user_name}]: {message}");
        }
    }

    public Task join_chat_room(string chatroom_name)
    {
        user_chatroom_name = chatroom_name;
        return Task.CompletedTask;
    }

    public Task send_to_client(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);

        client_connector.get_client_connector(grain_ticket);
        
        return Task.CompletedTask;
    }

    public Task leave_client()
    {
        var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(user_chatroom_name);
        if (chatroom_grain is not null)
        {
            chatroom_grain.leave_user(grain_ticket);
        }
        
        DeactivateOnIdle();
        return Task.CompletedTask;
    }
}


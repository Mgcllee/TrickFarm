using System.Text;

public class ChatClientGrain : Grain, IChatClientGrain
{
    private IGrainFactory grain_factory;
    private ClientConnector client_connector = null!;
    private RedisConnector redis_connector = null!;

    private Guid grain_id;
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
        grain_id = this.GetPrimaryKey();
        user_name = redis_connector.get_user_name(grain_id)!;
        Console.WriteLine($"ChatClientGrain::OnActivateAsync: {grain_id}, {user_name}");
        return Task.CompletedTask;
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        if (redis_connector.delete_user_info(grain_id))
        {
            Console.WriteLine($"Grain::OnDeactivateAsync: {grain_id}");
        }
        else
        {
            Console.WriteLine($"[Error][OnDeactivateAsync]: {grain_id}를 찾을 수 없음");
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
            await chatroom_grain.join_user(grain_id, user_name);
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

    public async Task leave_client()
    {
        var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(user_chatroom_name);
        if (chatroom_grain is not null)
        {
            await chatroom_grain.leave_user(grain_id);
        }
        
        await ClientConnector.check_exist_client();

        DeactivateOnIdle();
    }
}


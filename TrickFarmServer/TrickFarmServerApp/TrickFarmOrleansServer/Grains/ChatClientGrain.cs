using Orleans;
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

    public async Task logout_client()
    {
        var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(user_chatroom_name);
        if (chatroom_grain is not null)
        {
            await chatroom_grain.leave_user(grain_id);
        }

        await ClientConnector.check_exist_client();

        DeactivateOnIdle();
    }

    public async Task join_chatroom(string chatroom_name)
    {
        enter_chatroom = true;
        user_chatroom_name = chatroom_name;
        Console.WriteLine($"{user_name}님이 {user_chatroom_name}방에 입장 시도");
        var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(user_chatroom_name);
        await chatroom_grain.join_user(grain_id, user_name);
    }

    public async Task leave_chatroom()
    {
        enter_chatroom = false;
        Console.WriteLine($"{user_name}님이 {user_chatroom_name}방에서 떠납니다.");
        var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(user_chatroom_name);
        await chatroom_grain.leave_user(grain_id);
    }

    public async Task enter_chat_message(string message)
    {
        Console.WriteLine($"{user_chatroom_name}방으로 '{message}' 채팅을 보내려고 하려고 함.");
        if (enter_chatroom)
        {
            var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(user_chatroom_name);
            await chatroom_grain.broadcast_message($"[{user_chatroom_name}][{user_name}]: {message}");
        }
    }
}


using Orleans;
using System.Text;

public class ChatClientGrain : Grain<ChatClientGrainState>, IChatClientGrain
{
    private IGrainFactory grain_factory;
    private ClientConnector client_connector = null!;
    private RedisConnector redis_connector = null!;

    public ChatClientGrain(IGrainFactory grain_factory, ClientConnector client_connector, RedisConnector redis_connector)
    {
        this.grain_factory = grain_factory;
        this.client_connector = client_connector;
        this.redis_connector = redis_connector;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        State.grain_id = this.GetPrimaryKey();
        State.user_name = redis_connector.get_user_name(State.grain_id)!;
        Console.WriteLine($"ChatClientGrain::OnActivateAsync: {State.grain_id}, {State.user_name}");
        return Task.CompletedTask;
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        if (redis_connector.delete_user_info(State.grain_id))
        {
            Console.WriteLine($"Grain::OnDeactivateAsync: {State.grain_id}");
        }
        else
        {
            Console.WriteLine($"[Error][OnDeactivateAsync]: {State.grain_id}를 찾을 수 없음");
        }
        return base.OnDeactivateAsync(reason, cancellationToken);
    }

    public async Task logout_client()
    {
        var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(State.user_chatroom_name);
        if (chatroom_grain is not null)
        {
            await chatroom_grain.leave_user(State.grain_id);
        }

        await ClientConnector.check_exist_client();

        DeactivateOnIdle();
    }

    public async Task join_chatroom(string chatroom_name)
    {
        State.enter_chatroom = true;
        State.user_chatroom_name = chatroom_name;
        Console.WriteLine($"{State.user_name}님이 {State.user_chatroom_name}방에 입장 시도");
        var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(State.user_chatroom_name);
        await chatroom_grain.join_user(State.grain_id, State.user_name);
    }

    public async Task leave_chatroom()
    {
        State.enter_chatroom = false;
        Console.WriteLine($"{State.user_name}님이 {State.user_chatroom_name}방에서 떠납니다.");
        var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(State.user_chatroom_name);
        await chatroom_grain.leave_user(State.grain_id);
    }

    public async Task enter_chat_message(string message)
    {
        Console.WriteLine($"{State.user_chatroom_name}방으로 '{message}' 채팅을 보내려고 하려고 함.");
        if (State.enter_chatroom)
        {
            var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(State.user_chatroom_name);
            await chatroom_grain.broadcast_message($"[{State.user_chatroom_name}][{State.user_name}]: {message}");
        }
    }
}


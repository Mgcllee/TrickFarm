using System.Text;

public class ChatClientGrain : Grain, IChatClientGrain
{
    private IGrainFactory grain_factory;
    private ClientConnector client_connector = null!;
    private RedisConnector redis_connector = null!;

    private Guid user_guid;
    private string user_name = null!;
    private string user_chatroom_name = null!;
    private bool enter_chatroom = false;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        user_guid = this.GetPrimaryKey();
        user_name = redis_connector.get_user_name(user_guid)!;
        Console.WriteLine($"ChatClientGrain::OnActivateAsync: {user_guid}, {user_name}");
        return Task.CompletedTask;
    }

    public ChatClientGrain(IGrainFactory grain_factory, ClientConnector client_connector, RedisConnector redis_connector) 
    {
        this.client_connector = client_connector;
        this.redis_connector = redis_connector;
        this.grain_factory = grain_factory;
    }

    public Task packet_worker()
    {
        Task.Run(async () =>
     {
         var client_socket = client_connector.get_client_connector(user_guid);
         if (client_socket == null)
         {
             return;
         }

         var client_stream = client_socket.GetStream();
         while (client_stream != null)
         {
             byte[] bytes = new byte[1024];
             int recv_len = await client_stream.ReadAsync(bytes, 0, bytes.Length);
             string message = Encoding.UTF8.GetString(bytes, 0, recv_len);
             if (recv_len <= 0 || message.Length <= 0)
             {
                 continue;
             }

             if (message == "leave" && enter_chatroom)
             {
                 var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(user_chatroom_name);
                 await chatroom_grain.leave_user(user_guid);
                 await leave_client();
                 break;
             }
             else if (message.Contains("join ") && false == enter_chatroom)
             {
                 user_chatroom_name = message.Split(new[] { ' ' }, 2)[1];
                 var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(user_chatroom_name);
                 await chatroom_grain.join_user(user_guid, user_name);
                 await join_chat_room(user_chatroom_name);
                 enter_chatroom = true;
             }
             else if (enter_chatroom)
             {
                 var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(user_chatroom_name);
                 await chatroom_grain.broadcast_message($"[{user_chatroom_name}][{user_name}]: {message}");
             }
         }
     });
        return Task.CompletedTask;
    }

    public Task join_chat_room(string chatroom_name)
    {
        user_chatroom_name = chatroom_name;
        return Task.CompletedTask;
    }

    public Task send_to_client(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        var client_socket = client_connector.get_client_connector(user_guid);
        if (client_socket is not null && client_socket.Connected)
        {
            var client_stream = client_socket.GetStream();
            client_stream.WriteAsync(bytes, 0, bytes.Length);

            Console.WriteLine("ClientGrain::send_to_client - " + message);
        }
        else
        {
            client_socket.Dispose();
            leave_client();
        }
        return Task.CompletedTask;
    }

    public Task leave_client()
    {
        var chatroom_grain = grain_factory.GetGrain<IChatRoomGrain>(user_chatroom_name);
        chatroom_grain.leave_user(user_guid);
        DeactivateOnIdle();
        return Task.CompletedTask;
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        if (redis_connector.delete_user_info(user_guid))
        {
            Console.WriteLine($"Grain::OnDeactivateAsync: {user_guid}");
        }
        else
        {
            Console.WriteLine($"[Error][OnDeactivateAsync]: {user_guid}를 찾을 수 없음");
        }
        return base.OnDeactivateAsync(reason, cancellationToken);
    }
}


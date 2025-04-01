
using System.Net.Sockets;

public class ChatClientGrain : Grain, IChatClientGrain
{
    private IGrainFactory _grainFactory = null!;
    private Guid user_guid;
    private string user_name = null!;
    private string user_chatroom_name = null!;
    private int user_level = 1;
    private int user_exp = 0;
    private ChatClient _tcpClient = null!;
    private bool enter_chatroom = false;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        user_guid = this.GetPrimaryKey();
        if (Program.user_db.KeyExists(user_guid.ToString()))
        {
            user_name = Program.user_db.StringGet(user_guid.ToString())!;
            Console.WriteLine($"Grain::OnActivateAsync: {user_guid}, {user_name}");
        }
        else
        {
            Console.WriteLine($"[Error][OnActivateAsync]: {user_guid}를 찾을 수 없음");
            DeactivateOnIdle();
        }
        return Task.CompletedTask;
    }

    public Task packet_worker()
    {
        Task.Run(async () =>
     {
         while (true)
         {
             string message = " "; // = await _tcpClient.recv_client_chat_message();
             if (message.Length <= 0)
             {
                 continue;
             }

             if (message == "leave")
             {
                 var chatroom_grain = _grainFactory.GetGrain<IChatRoomGrain>(user_chatroom_name);
                 await chatroom_grain.leave_user(user_guid);
                 await leave_client();
                 break;
             }
             else if (message.Contains("join ") && false == enter_chatroom)
             {
                 user_chatroom_name = message.Split(new[] { ' ' }, 2)[1];
                 var chatroom_grain = _grainFactory.GetGrain<IChatRoomGrain>(user_chatroom_name);
                 await chatroom_grain.join_user(user_guid, user_name);
                 await join_chat_room(user_chatroom_name);
                 enter_chatroom = true;
             }
             else if (enter_chatroom)
             {
                 var chatroom_grain = _grainFactory.GetGrain<IChatRoomGrain>(user_chatroom_name);
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

    public Task recv_client_message(string message)
    {
        string formatted_message = $"[{user_chatroom_name}][{user_name}]: {message}";
        var chatroom_grain = _grainFactory.GetGrain<IChatRoomGrain>(user_chatroom_name);
        chatroom_grain.broadcast_message(formatted_message);
        return Task.CompletedTask;
    }

    public Task send_to_client(string message)
    {
        // _ = _tcpClient.send_to_client_chat_message(message);
        return Task.CompletedTask;
    }

    public Task leave_client()
    {
        var chatroom_grain = _grainFactory.GetGrain<IChatRoomGrain>(user_chatroom_name);
        chatroom_grain.leave_user(user_guid);
        DeactivateOnIdle();
        return Task.CompletedTask;
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Grain::OnDeactivateAsync: {user_guid}");
        if (Program.user_db.KeyExists(user_guid.ToString()))
        {
            Program.user_db.KeyDelete(user_guid.ToString());
        }
        else
        {
            Console.WriteLine($"[Error][OnDeactivateAsync]: {user_guid}를 찾을 수 없음");
        }
        return base.OnDeactivateAsync(reason, cancellationToken);
    }
}


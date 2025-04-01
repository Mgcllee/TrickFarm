
public class ChatClientGrain : Grain, IChatClientGrain
{
    private IGrainFactory _grainFactory = null!;
    private Guid user_guid;
    private string user_name = null!;
    private int user_level = 1;
    private int user_exp = 0;

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

    public Task print_recv_message(string message)
    {
        Console.WriteLine($"{user_name}: {message}");
        return Task.CompletedTask;
    }

    public Task leave_client()
    {
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


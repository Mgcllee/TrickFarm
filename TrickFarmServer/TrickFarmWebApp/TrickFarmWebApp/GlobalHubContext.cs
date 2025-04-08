using Microsoft.AspNetCore.SignalR;
using TrickFarmWebApp.Hubs;

public static class GlobalHubContext
{
    public static IHubContext<ChatHub>? HubContext;

    public static void Initialize(IHubContext<ChatHub> context)
    {
        HubContext = context;
    }
}

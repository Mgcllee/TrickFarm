using System.Security.AccessControl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

public static class Program
{
    private static IHost host = null!;

    public static async Task Main()
    {   
        Console.CancelKeyPress += new ConsoleCancelEventHandler(OnProcessExit!);
        host = new HostBuilder()
            .UseOrleans(siloBuilder =>
            {
                siloBuilder.UseLocalhostClustering();
                
                siloBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton<RedisConnector>();
                    services.AddSingleton<ClientConnector>();
                });

                siloBuilder.UseDashboard(options =>
                {
                    options.Host = "*";
                    options.Port = 8080; 
                    options.BasePath = "/dashboard";
                    options.HostSelf = true; 
                });
            })
            .Build();
        await host.StartAsync();

        var grainFactory = host.Services.GetRequiredService<IGrainFactory>();
        var redisConnector = host.Services.GetRequiredService<RedisConnector>();
        var client_accepter = host.Services.GetRequiredService<ClientConnector>();
        await client_accepter.start_client_accepter();
        
        await host.WaitForShutdownAsync();
        Environment.Exit(0);
    }

    public static async void OnProcessExit(object sender, EventArgs e)
    {
        Console.WriteLine("프로그램이 종료됩니다. 정리 작업을 수행합니다.");
        
        await host.WaitForShutdownAsync();
        Console.WriteLine("Orleans 종료");

        host.Services.GetRequiredService<ClientConnector>().stop_server_accepter();
        Environment.Exit(0);
    }
}
using System.Security.AccessControl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

public static class Program
{
    private static IHost host = null!;
    private static TcpChatServer server = null!;

    public static async Task Main()
    {   
        Console.CancelKeyPress += new ConsoleCancelEventHandler(OnProcessExit);
        host = new HostBuilder()
            .UseOrleans(siloBuilder =>
            {
                siloBuilder.UseLocalhostClustering();
                
                siloBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton<RedisConnector>();
                });

                // Orleans 대시보드 설정 ( http://localhost:8080/dashboard )
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
        server = new TcpChatServer(grainFactory);
        await server.StartAsync(host.Services.GetRequiredService<RedisConnector>());
        await host.WaitForShutdownAsync();
        Environment.Exit(0);
    }

    public static async void OnProcessExit(object sender, EventArgs e)
    {
        Console.WriteLine("프로그램이 종료됩니다. 정리 작업을 수행합니다.");
        
        await host.WaitForShutdownAsync();
        Console.WriteLine("Orleans 종료");

        server.StopServer();

        Environment.Exit(0);
    }
}
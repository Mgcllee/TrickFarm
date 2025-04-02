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
                // 로컬에서 동작하기 위한 설정
                siloBuilder.UseLocalhostClustering();
                
                // Orleans 설정들 입력
                siloBuilder.ConfigureServices(services =>
                {
                    // Singleton (Orleans에서 관리)
                    services.AddSingleton<ClientConnector>();
                    services.AddSingleton<RedisConnector>();
                });
            })
            .Build();
        await host.StartAsync();

        var grainFactory = host.Services.GetRequiredService<IGrainFactory>();
        server = new TcpChatServer(grainFactory, 5000);
        await server.StartAsync(
            host.Services.GetRequiredService<ClientConnector>(), 
            host.Services.GetRequiredService<RedisConnector>()
            );

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
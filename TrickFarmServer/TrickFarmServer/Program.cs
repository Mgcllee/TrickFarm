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

                // Orleans 대시보드 설정 ( http://localhost:8080/dashboard )
                siloBuilder.UseDashboard(options =>
                {
                    options.Host = "*";  // 모든 IP에서 접근 가능
                    options.Port = 8080; // 대시보드 포트
                    options.BasePath = "/dashboard"; // URL 경로
                    options.HostSelf = true; // 별도 웹 서버 없이 실행
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
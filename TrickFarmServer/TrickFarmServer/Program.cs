using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

public static class Program
{
    private static ConnectionMultiplexer redis = null!;
    private static IHost host = null!;
    public static IDatabase user_db = null!;

    public static async Task Main()
    {
        Console.CancelKeyPress += new ConsoleCancelEventHandler(OnProcessExit);

        redis = ConnectionMultiplexer.Connect(new ConfigurationOptions { EndPoints = { "127.0.0.1:6379" }, AllowAdmin=true });
        user_db = redis.GetDatabase();
        var pong = user_db.Ping();
        Console.WriteLine(pong);

        host = new HostBuilder()
            .UseOrleans(siloBuilder =>
            {
                // Orleans 설정들 입력
                siloBuilder.UseLocalhostClustering(); // 로컬에서 동작하기 위한 설정
            })
            .Build();
        await host.StartAsync();

        var grainFactory = host.Services.GetRequiredService<IGrainFactory>();
        var server = new TcpChatServer(grainFactory, 5000);
        await server.StartAsync();

        await host.WaitForShutdownAsync();
        redis.GetServer("127.0.0.1", 6379).FlushAllDatabases();
        redis.Close();
        Environment.Exit(0);
    }

    public static async void OnProcessExit(object sender, EventArgs e)
    {
        Console.WriteLine("프로그램이 종료됩니다. 정리 작업을 수행합니다.");
        
        await host.WaitForShutdownAsync();
        Console.WriteLine("Orleans 종료");

        var server = redis.GetServer("127.0.0.1", 6379);
        server.FlushDatabase();
        redis.Close();
        Console.WriteLine("Redis 종료");
        
        Environment.Exit(0);
    }
}
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;

// 1. 채팅 Grain 인터페이스 정의
public interface IChatClientGrain : IGrainWithStringKey
{
    Task SendMessageAsync(string message);
    Task ReceiveMessageAsync(string message);
}

// 2. 채팅 Grain 구현
public class ChatClientGrain : Grain, IChatClientGrain
{
    private static ConcurrentBag<IChatClientGrain> Clients = new();
    private string? _clientId;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _clientId = this.GetPrimaryKeyString();
        Clients.Add(this);
        Console.WriteLine($"클라이언트 연결됨: {_clientId}");
        return Task.CompletedTask;
    }

    public Task SendMessageAsync(string message)
    {
        Console.WriteLine($"[{_clientId}] {message}");
        Parallel.ForEach(Clients, async client => await client.ReceiveMessageAsync(message));
        return Task.CompletedTask;
    }

    public Task ReceiveMessageAsync(string message)
    {
        Console.WriteLine($"클라이언트에게 전송됨: {message}");
        return Task.CompletedTask;
    }
}

// 3. TCP 서버
class TcpChatServer
{
    private IGrainFactory _grainFactory;
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<TcpClient, IChatClientGrain> _clients = new();

    public TcpChatServer(IGrainFactory grainFactory, int port)
    {
        _grainFactory = grainFactory;
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine("채팅 서버 시작됨");

        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            Console.WriteLine("클라이언트 연결됨");
            _ = HandleClientAsync(client);
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        string guid = Guid.NewGuid().ToString();
        var grain = _grainFactory.GetGrain<IChatClientGrain>("Mgcllee");
        _clients[client] = grain;

        using var stream = client.GetStream();
        var buffer = new byte[1024];

        try
        {
            while (client.Connected)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                Console.WriteLine($"[{guid}] {bytesRead}바이트 수신: {buffer}");

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                await grain.SendMessageAsync(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"오류 발생: {ex.Message}");
        }
        finally
        {
            _clients.TryRemove(client, out var clientGrain);
            client.Close();
            Console.WriteLine("클라이언트 연결 해제됨");
        }
    }
}

// 4. 서버 실행
class Program
{
    public static async Task Main()
    {
        var host = new HostBuilder()
            .UseOrleans(siloBuilder =>
            {
                siloBuilder.UseLocalhostClustering();
            })
            .Build();

        await host.StartAsync();
        var grainFactory = host.Services.GetRequiredService<IGrainFactory>();
        var server = new TcpChatServer(grainFactory, 5000);
        await server.StartAsync();
    }
}
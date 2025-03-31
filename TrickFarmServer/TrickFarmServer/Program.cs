using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

public interface IChatClient
{
    
}

public class ChatClient : IChatClient
{
    private TcpClient _tcpClient;

    public ChatClient(TcpClient tcpClient) => _tcpClient = tcpClient;
    public string get_user_name()
    {
        byte[] buffer = new byte[1024];
        int len = _tcpClient.GetStream().Read(buffer, 0, buffer.Length); 
        return Encoding.UTF8.GetString(buffer, 0, len);
    }
    
    public async Task<string> recv_chat_message()
    {
        byte[] buffer = new byte[1024];
        int len = await _tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);
        Task<string> chat;
        return chat;
    }
}

// 1. 채팅 Grain 인터페이스 정의
public interface IChatClientGrain : IGrainWithStringKey
{
    Task Init(IGrainFactory grainFactory, IChatClient chatClient);
    Task SendMessageAsync(string message);
    Task ReceiveMessageAsync(string message);
}

// 2. 채팅 Grain 구현
public class ChatClientGrain : Grain, IChatClientGrain
{
    private IGrainFactory _grainFactory;
    private string _clientId = string.Empty;
    private IChatClient client_socket;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _clientId = this.GetPrimaryKeyString();
        Console.WriteLine($"Grain::OnActivateAsync: {_clientId}");
        return Task.CompletedTask;
    }

    public Task Init(IGrainFactory grainFactory, IChatClient chatClient)
    {
        _grainFactory = grainFactory;
        client_socket = chatClient;
        return Task.CompletedTask;
    }
    public Task SendMessageAsync(string message)
    {
        Console.WriteLine($"[{_clientId}] {message}");
        return Task.CompletedTask;
    }

    public Task ReceiveMessageAsync(string message)
    {
        Console.WriteLine($"[{_clientId}] {message}");
        return Task.CompletedTask;
    }
}

// 3. TCP 서버
class TcpChatServer
{
    private readonly IGrainFactory _grainFactory;
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
        ChatClient chatClient = new ChatClient(client);
        string user_name = chatClient.get_user_name();

        var grain = _grainFactory.GetGrain<IChatClientGrain>(user_name);
        await grain.Init(_grainFactory, chatClient);
        _clients[client] = grain;
        
        try
        {
            while (client.Connected)
            {
                // chatClient.recv_chat_message(message);
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
                // Orleans 설정들 입력
                siloBuilder.UseLocalhostClustering(); // 로컬에서 동작하기 위한 설정

            })
            .Build();
        await host.StartAsync();

        var grainFactory = host.Services.GetRequiredService<IGrainFactory>();
        var server = new TcpChatServer(grainFactory, 5000);
        await server.StartAsync();
        await host.WaitForShutdownAsync();
    }
}
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;

class TcpChatServer
{
    private readonly IGrainFactory _grainFactory;
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<string, IChatClient> _clients = new();

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
        _clients[user_name] = chatClient;

        Guid new_user_guid = Guid.NewGuid();
        var grain = _grainFactory.GetGrain<IChatClientGrain>(new_user_guid);

        if (Program.user_db.StringSet(new_user_guid.ToString(), user_name))
        {
            var db_guid = Program.user_db.StringGet(new_user_guid.ToString());
            Console.WriteLine($"Redis에 기록 성공! 어서오세요 {db_guid}님");
        }
        else
        {
            Console.WriteLine("Redis에 기록 실패...");
            _clients.TryRemove(user_name, out var clientGrain);
            client.Close();
            Console.WriteLine("클라이언트 연결 해제됨");
            return;
        }

        try
        {
            while (client.Connected)
            {
                string message = await _clients[user_name].recv_chat_message();
                if (message == "leave")
                {
                    await grain.leave_client();
                    break;
                }
                else
                {
                    await grain.print_recv_message(message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"오류 발생: {ex.Message}");
        }
        finally
        {
            _clients.TryRemove(user_name, out var clientGrain);
            client.Close();
            Console.WriteLine("클라이언트 연결 해제됨");
        }
    }
}
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Text;

class TcpChatServer
{
    private readonly IGrainFactory _grainFactory;
    private readonly TcpListener _listener;

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

    private async Task HandleClientAsync(TcpClient client_socket)
    {
        byte[] bytes = new byte[1024];
        int name_len = await client_socket.GetStream().ReadAsync(bytes, 0, bytes.Length);
        string user_name = Encoding.UTF8.GetString(bytes, 0, name_len);

        Guid new_user_guid = Guid.NewGuid();
        var client_grain = _grainFactory.GetGrain<IChatClientGrain>(new_user_guid);

        if (Program.user_db.StringSet(new_user_guid.ToString(), user_name))
        {
            var db_guid = Program.user_db.StringGet(new_user_guid.ToString());
            Console.WriteLine($"Redis에 기록 성공! 어서오세요 {db_guid}님");

            // [TODO]: Grain에 직접 TcpClient를 넘겨주면 안됨, 다른 방법을 찾아야 함
            await client_grain.packet_worker();
        }
        else
        {
            Console.WriteLine("Redis에 기록 실패...");
            client_socket.Close();
            Console.WriteLine("클라이언트 연결 해제됨");
        }
    }
}
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Text;

class TcpChatServer
{
    private readonly IGrainFactory _grainFactory;
    private readonly TcpListener _listener;
    private ClientConnector client_connector = null!;
    private RedisConnector redis_connector = null!;

    public TcpChatServer(IGrainFactory grainFactory, int port)
    {
        _grainFactory = grainFactory;
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync(ClientConnector client_connector, RedisConnector redis_connector)
    {
        this.client_connector = client_connector;
        this.redis_connector = redis_connector;
        _listener.Start();

        Console.WriteLine("[Start] server listening socket");

        while (true)
        {
            var client_socket = await _listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client_socket);
        }
    }

    private async Task HandleClientAsync(TcpClient client_socket)
    {
        var user_guid = client_connector.add_client_connector(client_socket);
        var client = client_connector.get_client_connector(user_guid);
        
        if(client == null)
        {
            client_connector.discoonect_client(user_guid);
            Console.WriteLine("HandleClientAsync가 전달 받은 client_socket 인스턴스가 유효하지 않음");
            return;
        }

        byte[] bytes = new byte[1024];
        int name_len = await client_socket.GetStream().ReadAsync(bytes, 0, bytes.Length);
        string user_name = Encoding.UTF8.GetString(bytes, 0, name_len);

        if (name_len > 0 && user_name is not "leave" && user_name.Length <= 10 && user_name.Contains(" ")
            && redis_connector.write_user_info(user_guid, user_name))
        {
            var client_grain = _grainFactory.GetGrain<IChatClientGrain>(user_guid);
            Console.WriteLine($"Redis에 기록 성공! 어서오세요 {user_name}님");
            await client_grain.packet_worker();
        }
        else
        {
            redis_connector.delete_user_info(user_guid);
            Console.WriteLine("Redis에 기록 실패...");

            client_connector.discoonect_client(user_guid);
            Console.WriteLine("클라이언트 연결 해제됨");
        }
    }

    public void StopServer()
    {
        client_connector.disconnect_all_clients();
        Console.WriteLine("모든 클라이언트 연결 종료");

        redis_connector.disconnect_redis();
        Console.WriteLine("Redis 메모리 FlushALL 처리 완료");
    }
}
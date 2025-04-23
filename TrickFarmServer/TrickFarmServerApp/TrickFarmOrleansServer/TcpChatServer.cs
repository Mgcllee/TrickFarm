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

        Console.WriteLine("[Server][Start] listening client socket");

        while (true)
        {
            var client_socket = await _listener.AcceptTcpClientAsync();
            if(client_socket is null || false == client_socket.Connected) {
                continue;
            }

            byte[] bytes = new byte[1024];
            int name_len = await client_socket.GetStream().ReadAsync(bytes, 0, bytes.Length);
            string user_name = Encoding.UTF8.GetString(bytes, 0, name_len);

            var user_guid = client_connector.add_client_connector(client_socket);
            if (name_len > 0 && user_name is not "leave" 
                && user_name.Length <= 10 
                && user_name.Contains(" ")
                && redis_connector.write_user_info(user_guid, user_name))
            {
                var client_grain = _grainFactory.GetGrain<IChatClientGrain>(user_guid);
                Console.WriteLine($"Redis에 기록 성공! 어서오세요 {user_name}님");
                ClientRequestWorkerAsync(client_socket);
            }
            else
            {
                redis_connector.delete_user_info(user_guid);
                Console.WriteLine("Redis에 기록 실패...");

                client_connector.discoonect_client(user_guid);
                Console.WriteLine("클라이언트 연결 해제됨");
            }
        }
    }

    private async Task ClientRequestWorkerAsync(TcpClient client_socket, IChatClientGrain client_grain)
    {
        while(true) 
        {
            
            switch(buffer[0]) 
            {
                
            }
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
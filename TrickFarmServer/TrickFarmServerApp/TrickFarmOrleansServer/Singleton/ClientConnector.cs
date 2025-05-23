using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;

public class ClientConnector : IClientConnector
{
    static public readonly ConcurrentDictionary<Guid, GClient> clients = new();    

    private readonly IGrainFactory _grainFactory;

    private readonly RedisConnector redis_connector;

    public ClientConnector(IGrainFactory grainFactory, RedisConnector redisConnector)
    {
        _grainFactory = grainFactory;
        this.redis_connector = redisConnector;
    }

    public async Task start_client_accepter()
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Any, port: 5000);
        tcpListener.Start();

        Console.WriteLine("[Log] Accept web client socket");

        while (true)
        {
            // await check_exist_client();
            Console.WriteLine("[Log] 클라이언트 접속 대기중...");

            TcpClient new_client = await tcpListener.AcceptTcpClientAsync();

            if (new_client is null || false == new_client.Connected)
                continue;
                
            Console.WriteLine("[Success] accept new client");

            Guid new_client_guid = Guid.NewGuid();
            var exist_client = new GClient(new_client, _grainFactory, redis_connector, new_client_guid);
            clients[new_client_guid] = exist_client;
            await clients[new_client_guid].process_request();
        }
    }

    static public async Task check_exist_client()
    {
        foreach (var elem in clients)
        {
            GClient clinet = elem.Value;
            if (clinet.is_exist_client is false)
            {
                clients.Remove(elem.Key, out var g_client);
                if(g_client is not null)
                {
                    await g_client.disconnect();
                }
            }
        }
    }

    static public async Task discoonect_client(Guid user_guid)
    {
        clients.TryRemove(user_guid, out var gClient);

        if (gClient is not null)
        {
            Console.WriteLine($"{gClient.user_name}님 연결 종료");
            await gClient.disconnect();
        }
        else 
        {
            Console.WriteLine($"유효하지 않는 클라이언트를 연결 종료 시도함");
        }
    }

    public async Task disconnect_all_clients()
    {
        foreach (var client_socket in clients.Values)
        {
            await client_socket.disconnect();
        }
    }

    public void stop_server_accepter()
    {
        redis_connector.disconnect_redis();
        Console.WriteLine("Redis 메모리 FlushALL 처리 완료");
        clients.Clear();
        Console.WriteLine("clients 모든 원소 제거 완료");
    }
}
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Security.AccessControl;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

class ClientAccepter
{
    private readonly IGrainFactory _grainFactory;

    private readonly RedisConnector redis_connector;

    static private readonly Dictionary<Guid, GClient> clients = new Dictionary<Guid, GClient>();

    public ClientAccepter(IGrainFactory grainFactory, RedisConnector redisConnector)
    {
        _grainFactory = grainFactory;
        this.redis_connector = redisConnector;
    }

    public async Task start_client_accepter()
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Any, port: 7070);
        tcpListener.Start();

        while (true)
        {
            TcpClient new_client = await tcpListener.AcceptTcpClientAsync();

            if (new_client is null || false == new_client.Connected)
                continue;

            Guid new_guid = Guid.NewGuid();
            var exist_client = new GClient(new_client, _grainFactory, new_guid);
            await exist_client.process_request();
            clients.Add(new_guid, exist_client);
        }
    }

    static public async void check_exist_client()
    {
        foreach (var elem in clients)
        {
            GClient clinet = elem.Value;
            if (clinet.is_exist_client is false)
            {
                clients.Remove(elem.Key);
            }
        }
    }

    public void StopServer()
    {
        redis_connector.disconnect_redis();
        Console.WriteLine("Redis 메모리 FlushALL 처리 완료");
        clients.Clear();
        Console.WriteLine("clients 모든 원소 제거 완료");
    }
}
using Microsoft.AspNetCore.SignalR;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using TrickFarmWebApp.Hubs;

public class GlobalClientManager
{
    private TcpClient? _client;
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private ChatHub? _hubContext;
    private readonly IHubContext<ChatHub> _hub;

    public static Dictionary<string, ClientConnection> ClientConnections = new Dictionary<string, ClientConnection>();

    public GlobalClientManager(IHubContext<ChatHub> hub)
    {
        _hub = hub;
        ConnectToTcpServer();
    }

    private void ConnectToTcpServer()
    {
        try
        {
            _client = new TcpClient("trickfarm-orleans.koreacentral.cloudapp.azure.com", 5000);
            NetworkStream stream = _client.GetStream();
            _writer = new StreamWriter(stream) { AutoFlush = true };
            _reader = new StreamReader(stream);

            _ = Task.Run(ReceiveLoop);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TCP 연결 실패] {ex.Message}");
        }
    }

    private async Task ReceiveLoop()
    {
        try
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                if (_client is null) break;
                int len = await _client.GetStream().ReadAsync(buffer, 0, buffer.Length);

                if (len <= 0)
                    continue;

                PACKET_TYPE packet_type = (PACKET_TYPE)buffer[0];
                switch(packet_type)
                {
                    case PACKET_TYPE.S2C_CHAT_MESSAGE:
                        var message_packet = ByteArrayToStructure<C2S_MESSAGE_PACKET>(buffer);
                        if (message_packet.size > 0)
                        {
                            string msg = Encoding.UTF8.GetString(message_packet.message, 0, message_packet.message.Length);
                            await _hub.Clients.All.SendAsync("ReceiveChatFromServer", msg);
                        }
                        break;
                    default:
                        Console.WriteLine($"[알 수 없는 패킷 타입] {packet_type}");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TCP 수신 오류] {ex.Message}");
        }
    }

    private byte[] StructureToByteArray<T>(T obj)
    {
        int size = Marshal.SizeOf(typeof(T));
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(obj, buffer, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(buffer, bytes, 0, size);
            return bytes;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private T? ByteArrayToStructure<T>(byte[] bytes)
    {
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            return Marshal.PtrToStructure<T>(ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}

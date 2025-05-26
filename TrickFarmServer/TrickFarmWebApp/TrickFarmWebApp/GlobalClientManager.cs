using Microsoft.AspNetCore.SignalR;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using TrickFarmWebApp.Hubs;

public class GlobalClientManager : BackgroundService
{
    private TcpClient _client;
    private NetworkStream _stream;
    private ChatHub? _hubContext;
    private readonly IHubContext<ChatHub> _hub;

    public static Dictionary<string, ClientConnection> ClientConnections = new Dictionary<string, ClientConnection>();

    public GlobalClientManager(IHubContext<ChatHub> hub)
    {
        _hub = hub;
        Console.WriteLine("[Log] GlobalClientManager 생성자 호출됨");
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            Console.WriteLine("[Log] GlobalClientManager.ExecuteAsync 시작됨");

            _client = new TcpClient();
            Console.WriteLine("[Log] TcpClient 객체 생성 완료");

            await _client.ConnectAsync("trickfarm-orleans.koreacentral.cloudapp.azure.com", 5000);
            Console.WriteLine("[Log] TCP 연결 성공");

            _stream = _client.GetStream();
            Console.WriteLine("[Log] Stream 객체 초기화 완료");

            await ReceiveLoop();
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"[TCP 연결 실패 - 소켓 예외] {ex.SocketErrorCode} - {ex.Message}");
        }
        catch (IOException ioEx)
        {
            Console.WriteLine($"[TCP 수신 오류 - IO 예외] {ioEx.Message}");
            if (ioEx.InnerException is SocketException sockEx)
            {
                Console.WriteLine($"[내부 소켓 예외] {sockEx.SocketErrorCode} - {sockEx.Message}");
            }
        }
        catch (NullReferenceException nullEx)
        {
            Console.WriteLine($"[NullReferenceException] {nullEx.Message}");
            Console.WriteLine($"[스택트레이스] {nullEx.StackTrace}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[알 수 없는 예외] {ex.Message}");
            Console.WriteLine($"[스택트레이스] {ex.StackTrace}");
        }
    }

    private async Task ReceiveLoop()
    {
        try
        {
            Console.WriteLine("ReceiveLoop에서 수신 대기중...");
            byte[] buffer = new byte[1024];
            int len = await _stream.ReadAsync(buffer, 0, buffer.Length);
            if (len <= 0)
                return;

            PACKET_TYPE packet_type = (PACKET_TYPE)buffer[0];
            Console.WriteLine($"[Log] 수신 받은 패킷 타입: {packet_type}");
            switch (packet_type)
            {
                case PACKET_TYPE.S2C_CHAT_MESSAGE:
                    var message_packet = ByteArrayToStructure<S2C_MESSAGE_PACKET>(buffer);
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
        catch (Exception ex)
        {
            Console.WriteLine($"[TCP 수신 오류] {ex.Message}");
        }
    }

    static public async Task LoginToServer(string connectionId, string user_name)
    {
        if (ClientConnections.TryGetValue(connectionId, out ClientConnection? clientConnection)
            && clientConnection.network_socket.Connected)
        {
            byte[] user_name_bytes = Encoding.UTF8.GetBytes(user_name);

            if (user_name_bytes.Length < 100)
            {
                Array.Resize(ref user_name_bytes, 100);
            }

            await clientConnection.network_socket.GetStream().WriteAsync(StructureToByteArray(
                new C2S_LOGIN_PACKET
                {
                    type = (byte)PACKET_TYPE.C2S_LOGIN_USER,
                    size = (byte)Marshal.SizeOf(typeof(C2S_LOGIN_PACKET)),
                    user_name = user_name_bytes
                }));

            Console.WriteLine($"[{connectionId}][Log][GlobalClientManager.LoginToServer] _{user_name_bytes}_ 이름으로 패킷 전송 완료, 크기: {(byte)Marshal.SizeOf(typeof(C2S_LOGIN_PACKET))}");
        }
        else
        {
            Console.WriteLine($"[Error][GlobalClientManager.LoginToServer] _{user_name}_ 이름의 유저가 없음.");
        }
    }

    static public async Task LogoutToServer(string connectionId, string user_name)
    {
        if (ClientConnections.TryGetValue(connectionId, out ClientConnection? clientConnection))
        {
            byte[] user_name_bytes = Encoding.UTF8.GetBytes(user_name);

            if (user_name_bytes.Length < 100)
            {
                Array.Resize(ref user_name_bytes, 100);
            }

            await clientConnection.network_socket.GetStream().WriteAsync(StructureToByteArray(
                new C2S_LOGOUT_PACKET
                {
                    type = (byte)PACKET_TYPE.C2S_LOGOUT_USER,
                    size = (byte)Marshal.SizeOf(typeof(C2S_LOGOUT_PACKET)),
                    user_name = user_name_bytes
                }));
        }
    }

    static public async Task JoinToChatrrom(string connectionId, string chatroom_name)
    {
        if (ClientConnections.TryGetValue(connectionId, out ClientConnection? clientConnection))
        {
            byte[] chatroom_name_bytes = Encoding.UTF8.GetBytes(chatroom_name);

            if (chatroom_name_bytes.Length < 100)
            {
                Array.Resize(ref chatroom_name_bytes, 100);
            }

            await clientConnection.network_socket.GetStream().WriteAsync(StructureToByteArray(
                new C2S_ENTER_CHATROOM_PACKET
                {
                    type = (byte)PACKET_TYPE.C2S_ENTER_CHATROOM,
                    size = (byte)Marshal.SizeOf(typeof(C2S_ENTER_CHATROOM_PACKET)),
                    chatroom_name = chatroom_name_bytes
                }));
        }
    }

    static public async Task LeaveToChatroom(string connectionId, string chatroom_name)
    {
        if (ClientConnections.TryGetValue(connectionId, out ClientConnection? clientConnection))
        {
            byte[] chatroom_name_bytes = Encoding.UTF8.GetBytes(chatroom_name);

            if (chatroom_name_bytes.Length < 100)
            {
                Array.Resize(ref chatroom_name_bytes, 100);
            }

            await clientConnection.network_socket.GetStream().WriteAsync(StructureToByteArray(
                new C2S_LEAVE_CHATROOM_PACKET
                {
                    type = (byte)PACKET_TYPE.C2S_LEAVE_CHATROOM,
                    size = (byte)Marshal.SizeOf(typeof(C2S_LEAVE_CHATROOM_PACKET)),
                    chatroom_name = chatroom_name_bytes
                }));
        }
    }

    static public async Task SendToServer(string connectionId, string message)
    {
        if (ClientConnections.TryGetValue(connectionId, out ClientConnection? clientConnection))
        {
            byte[] message_bytes = Encoding.UTF8.GetBytes(message);

            if (message_bytes.Length < 100)
            {
                Array.Resize(ref message_bytes, 100);
            }

            await clientConnection.network_socket.GetStream().WriteAsync(StructureToByteArray(
                new C2S_MESSAGE_PACKET
                {
                    type = (byte)PACKET_TYPE.C2S_CHAT_MESSAGE,
                    size = (byte)Marshal.SizeOf(typeof(C2S_MESSAGE_PACKET)),
                    message = message_bytes
                }));
        }
    }

    static private byte[] StructureToByteArray<T>(T obj)
    {
        int size = Marshal.SizeOf(typeof(T));
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(obj, buffer, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(buffer, bytes, 0, size);
            Array.Resize(ref bytes, 1024);
            return bytes;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    static private T? ByteArrayToStructure<T>(byte[] bytes)
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

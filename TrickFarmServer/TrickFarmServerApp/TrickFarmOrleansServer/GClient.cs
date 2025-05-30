using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;

public class GClient : IChatClient
{
    public bool is_exist_client = false;
    private TcpClient tcp_socket;
    private IGrainFactory grainFactory;
    private RedisConnector redisConnector;
    public string user_name = "";
    private Guid user_guid;
    private string chatroom_name = "";

    public GClient(TcpClient tcpClient, IGrainFactory grainFactory, RedisConnector redisConnector, Guid user_guid)
    {
        this.tcp_socket = tcpClient;
        this.grainFactory = grainFactory;
        this.redisConnector = redisConnector;
        this.is_exist_client = true;
        this.user_guid = user_guid;
    }

    public async Task<string> recv_client_chat_message()
    {
        byte[] buffer = new byte[1024];
        int recv_bytes = await tcp_socket.GetStream().ReadAsync(buffer, 0, buffer.Length);
        if (recv_bytes > 0)
        {
            string recv_str = Encoding.UTF8.GetString(buffer, 0, recv_bytes);
            return recv_str;
        }
        return "";
    }

    public async Task send_to_client_chat_message(string message)
    {
        byte[] message_bytes = Encoding.UTF8.GetBytes(message);
        if (message_bytes.Length < 100)
        {
            Array.Resize(ref message_bytes, 100);
        }

        await tcp_socket.GetStream().WriteAsync(StructureToByteArray(
            new S2C_MESSAGE_PACKET
            {
                size = (byte)Marshal.SizeOf(typeof(S2C_MESSAGE_PACKET)),
                type = (byte)PACKET_TYPE.S2C_CHAT_MESSAGE,
                message = message_bytes
            }));
    }

    public async Task process_request()
    {
        Console.WriteLine("[Log]: start process_request");
        while (tcp_socket.Connected)
        {
            Console.WriteLine($"[Log] 클라이언트 요청 대기중...");
            byte[] buffer = new byte[1024];
            int read = tcp_socket.GetStream().Read(buffer, 0, buffer.Length);
            PACKET_TYPE packet_type = (PACKET_TYPE)ByteArrayToStructure<BASIC_PACKET>(buffer).type;

            Console.WriteLine($"[Log] 수신 받은 패킷 타입: {packet_type}");

            switch (packet_type)
            {
                case PACKET_TYPE.C2S_LOGIN_USER:
                    {
                        C2S_LOGIN_PACKET login_info = ByteArrayToStructure<C2S_LOGIN_PACKET>(buffer)!;
                        if (login_info.size <= 0)
                            break;

                        await make_grain(Encoding.UTF8.GetString(login_info.user_name).TrimEnd('\0'));
                        break;
                    }
                case PACKET_TYPE.C2S_ENTER_CHATROOM:
                    {
                        C2S_ENTER_CHATROOM_PACKET chatroom_info = ByteArrayToStructure<C2S_ENTER_CHATROOM_PACKET>(buffer)!;
                        if (chatroom_info.size <= 0 || chatroom_info.chatroom_name is null)
                            break;

                        chatroom_name = Encoding.UTF8.GetString(chatroom_info.chatroom_name)!;
                        await grainFactory.GetGrain<IChatClientGrain>(user_guid).join_chatroom(chatroom_name.TrimEnd('\0'));
                        break;
                    }
                case PACKET_TYPE.C2S_LEAVE_CHATROOM:
                    {
                        C2S_LEAVE_CHATROOM_PACKET leave_request = ByteArrayToStructure<C2S_LEAVE_CHATROOM_PACKET>(buffer)!;
                        if (leave_request.size <= 0 || leave_request.chatroom_name is null)
                            break;

                        await grainFactory.GetGrain<IChatClientGrain>(user_guid).leave_chatroom();
                        break;
                    }
                case PACKET_TYPE.C2S_CHAT_MESSAGE:
                    {
                        C2S_MESSAGE_PACKET message_packet = ByteArrayToStructure<C2S_MESSAGE_PACKET>(buffer)!;
                        if (message_packet.size <= 0 || message_packet.message is null)
                            break;

                        string chat_message = Encoding.UTF8.GetString(message_packet.message).TrimEnd('\0');
                        await grainFactory.GetGrain<IChatClientGrain>(user_guid).enter_chat_message(chat_message);
                        break;
                    }
                case PACKET_TYPE.C2S_LOGOUT_USER:
                    {
                        await grainFactory.GetGrain<IChatClientGrain>(user_guid).logout_client();
                        await ClientConnector.discoonect_client(user_guid);
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"[Error] 잘못된 패킷 타입을 수신 {packet_type}");
                        break;
                    }
            }
        }

        Console.WriteLine("[Error] Not exist Socket");
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

    private byte[] StructureToByteArray<T>(T obj)
    {
        int size = Marshal.SizeOf(obj);
        byte[] bytes = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            if (obj is not null)
            {
                Marshal.StructureToPtr(obj, ptr, true);
                Marshal.Copy(ptr, bytes, 0, size);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return bytes;
    }

    private async Task make_grain(string input_name)
    {
        Console.WriteLine($"_{input_name}_라는 이름으로 시도");
        if (input_name != "leave"
            && 0 < input_name.Length && input_name.Length < 10
            && false == input_name.Contains(" "))
        {
            this.user_name = input_name;
            Console.WriteLine($"[LOG] 어서오세요! {this.user_name}님!");
        }
        else
        {
            Console.WriteLine($"{input_name}은(는) 잘못된 형식의 사용자 이름입니다.");
            if(false == (0 < input_name.Length && input_name.Length < 10))
            {
                Console.WriteLine("길이가 허용범위 아님");
            }
            else if(input_name.Contains(" "))
            {
                Console.WriteLine("이름 안에 공백이 존재함.");
            }
            else if(input_name == "leave")
            {
                Console.WriteLine("허용되지 않는 이름");
            }
            return;
        }

        if (redisConnector.write_user_info(user_guid, user_name))
        {
            var client_grain = grainFactory.GetGrain<IChatClientGrain>(user_guid);
            Console.WriteLine($"Redis에 기록 성공! 어서오세요 {user_name}님");
        }
        else
        {
            Console.WriteLine($"Redis에 기록 실패... user_name: {user_name}");
        }
    }

    public async Task disconnect()
    {
        var client_grain = grainFactory.GetGrain<IChatClientGrain>(user_guid);
        if (client_grain != null)
        {
            await client_grain.logout_client();
        }
        tcp_socket.Dispose();
    }
}

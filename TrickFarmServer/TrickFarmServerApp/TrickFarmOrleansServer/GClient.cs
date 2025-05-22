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

    public GClient(TcpClient tcpClient, IGrainFactory grainFactory, RedisConnector redisConnector,  Guid user_guid)
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
        S2C_MESSAGE_PACKET packet = new S2C_MESSAGE_PACKET();
        packet.size = (byte)Marshal.SizeOf(packet);
        packet.type = (byte)PACKET_TYPE.S2C_CHAT_MESSAGE;
        packet.message = new byte[100];
        Array.Copy(Encoding.UTF8.GetBytes(message), packet.message, Math.Min(message.Length, 100));

        byte[] buffer = StructureToByteArray(packet);
        await tcp_socket.GetStream().WriteAsync(buffer, 0, buffer.Length);
    }

    public async Task process_request()
    {
        byte[] buffer = new byte[1024];
        Console.WriteLine("[Client]: start process_request");
        while (true)
        {
            int len = await tcp_socket.GetStream().ReadAsync(buffer, 0, buffer.Length);
            if (len <= 0)
                continue;

            PACKET_TYPE packet_type = (PACKET_TYPE)buffer[0];
            switch (packet_type)
            {
                case PACKET_TYPE.C2S_LOGIN_USER:
                    {
                        C2S_LOGIN_PACKET login_info = ByteArrayToStructure<C2S_LOGIN_PACKET>(buffer)!;
                        if (login_info.size <= 0)
                            break;

                        string input_user_name = login_info.user_name.ToString()!;
                        Console.WriteLine($"[Log] 어서오세요! {input_user_name}님!");
                        await make_grain(input_user_name);
                        break;
                    }
                case PACKET_TYPE.C2S_ENTER_CHATROOM:
                    {
                        C2S_ENTER_CHATROOM_PACKET chatroom_info = ByteArrayToStructure<C2S_ENTER_CHATROOM_PACKET>(buffer)!;
                        if (chatroom_info.size <= 0)
                            break;

                        chatroom_name = chatroom_info.chatroom_name.ToString()!;
                        if (chatroom_name is null)
                            break;

                        var chatroom_grain = grainFactory.GetGrain<IChatRoomGrain>(chatroom_name);
                        await chatroom_grain.join_user(user_guid, user_name);
                        break;
                    }
                case PACKET_TYPE.C2S_CHAT_MESSAGE:
                    {
                        C2S_MESSAGE_PACKET message_packet = ByteArrayToStructure<C2S_MESSAGE_PACKET>(buffer)!;
                        if (message_packet.size <= 0)
                            break;
                        if (message_packet.message is null)
                            break;

                        var chatroom_grain = grainFactory.GetGrain<IChatRoomGrain>(chatroom_name);
                        string chat_message = message_packet.message.ToString()!;
                        if (chat_message is not null && chat_message.Length > 0)
                        {
                            await chatroom_grain.broadcast_message(chat_message);
                        }
                        break;
                    }
                case PACKET_TYPE.C2S_LEAVE_CHATROOM:
                    {
                        C2S_LEAVE_CHATROOM_PACKET leave_request = ByteArrayToStructure<C2S_LEAVE_CHATROOM_PACKET>(buffer)!;
                        if (leave_request.size <= 0)
                            break;
                        if (leave_request.chatroom_name is null)
                            break;

                        string chatroom_name = leave_request.chatroom_name.ToString()!;
                        var chatroom_grain = grainFactory.GetGrain<IChatRoomGrain>(chatroom_name);
                        await chatroom_grain.leave_user(user_guid);
                        break;
                    }
                case PACKET_TYPE.C2S_LOGOUT_USER:
                    {
                        await ClientConnector.discoonect_client(user_guid);
                        break;
                    }
            }
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
 
    private async Task make_grain(string user_name)
    {
        if (user_name is not null)
        {
            this.user_name = user_name;
            Console.WriteLine($"[LOG] 어서오세요! {this.user_name}님!");
        }
        else return;

        var is_exist_user_name = await exist_user_name(user_name);
        if (is_exist_user_name)
        {
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
        else
        {
            Console.WriteLine($"Redis에 기록 실패... user_name: {user_name}");
        }
    }

    private Task<bool> exist_user_name(string input_name)
    {
        // TODO: 금지어, 형식이 추가될 때 마다 하드 코딩은 비효율. > 개선 필요.
        if (user_name is not "leave"
            && 0 < user_name.Length && user_name.Length < 10
            && user_name.Contains(" "))
            return Task.FromResult(true);
        else 
            return Task.FromResult(false);
    }

    public async Task disconnect() {
        var client_grain = grainFactory.GetGrain<IChatClientGrain>(user_guid);
        await client_grain.leave_client();
        tcp_socket.Dispose();
    }
}

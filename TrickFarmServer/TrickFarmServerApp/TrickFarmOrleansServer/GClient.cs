using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;

public class GClient : IChatClient
{
    public bool is_exist_client = false;
    private TcpClient tcp_socket;
    private IGrainFactory grainFactory;
    private RedisConnector redisConnector;
    private string user_name;
    private Guid user_guid;
    private string chatroom_name;

    public GClient(TcpClient tcpClient, IGrainFactory grainFactory, Guid user_guid)
    {
        this.tcp_socket = tcpClient;
        this.grainFactory = grainFactory;
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
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        await tcp_socket.GetStream().WriteAsync(buffer, 0, buffer.Length);
    }

    public async Task process_request()
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            int len = await tcp_socket.GetStream().ReadAsync(buffer, 0, buffer.Length);

            PACKET_TYPE packet_type = (PACKET_TYPE)buffer[0];
            switch (packet_type)
            {
                case PACKET_TYPE.C2S_LOGIN_USER:
                    {
                        C2S_LOGIN_PACKET login_info = ByteArrayToStructure<C2S_LOGIN_PACKET>(buffer);
                        string input_user_name = login_info.user_name.ToString()!;
                        await make_grain(input_user_name);
                        break;
                    }
                case PACKET_TYPE.C2S_CHAT_MESSAGE:
                    {
                        C2S_MESSAGE_PACKET message_packet = ByteArrayToStructure<C2S_MESSAGE_PACKET>(buffer);
                        var chatroom_grain = grainFactory.GetGrain<IChatRoomGrain>(chatroom_name);
                        await chatroom_grain.broadcast_message(message_packet.message.ToString());
                        break;
                    }
            }
        }
    }

    private T ByteArrayToStructure<T>(byte[] bytes) where T : class
    {
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            return Marshal.PtrToStructure(ptr, typeof(T)) as T;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    private async Task make_grain(string user_name)
    {
        if (user_name is not null)
        {
            this.user_name = user_name;
            Console.WriteLine($"[LOG] 어서오세요! {this.user_name}님!");
        }
        else return;

        // TODO: 금지어, 형식이 추가될 때 마다 하드 코딩은 비효율. > 개선 필요.
        if (user_name is not "leave"
        && 0 < user_name.Length && user_name.Length < 10
        && user_name.Contains(" "))
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
}

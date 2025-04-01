using System.Net.Sockets;
using System.Text;

public class ChatClient : IChatClient
{
    private TcpClient _tcpClient;

    public ChatClient(TcpClient tcpClient) => _tcpClient = tcpClient;
    public string get_user_name()
    {
        byte[] buffer = new byte[1024];
        int len = _tcpClient.GetStream().Read(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer, 0, len);
    }

    public async Task<string> recv_client_chat_message()
    {
        byte[] buffer = new byte[1024];
        int recv_bytes = await _tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);
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
        await _tcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
    }
}

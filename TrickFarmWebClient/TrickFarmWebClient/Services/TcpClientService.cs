using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class TcpClientService
{
    private TcpClient _client;
    private NetworkStream _stream;
    private readonly IHubContext<ChatHub> _hubContext;

    public TcpClientService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
        _client = new TcpClient();
    }

    public async Task<bool> ConnectAsync(string ip, int port)
    {
        try
        {
            await _client.ConnectAsync(ip, port);
            _stream = _client.GetStream();
            _ = ReadMessagesAsync(); // 메시지 수신을 백그라운드에서 실행
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task ReadMessagesAsync()
    {
        byte[] buffer = new byte[1024];
        while (_client.Connected)
        {
            try
            {
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"[서버로부터 수신]: {message}");

                    // SignalR을 통해 Blazor 클라이언트에게 전달
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Server", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"서버 메시지 수신 오류: {ex.Message}");
                break;
            }
        }
    }

    public async Task<string> SendMessageAsync(string message)
    {
        if (_client.Connected && _stream != null)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
        return "연결되지 않음";
    }
}

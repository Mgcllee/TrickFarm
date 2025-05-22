using Microsoft.AspNetCore.SignalR;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace TrickFarmWebApp.Hubs;

public class ChatHub : Hub
{
    private static readonly Dictionary<string, TcpClient> TcpConnections = new();
    private static readonly Dictionary<string, ConnectionState> Connections = new();

    public async Task ConnectToTrickFarm()
    {
        var connectionId = Context.ConnectionId;

        if (TcpConnections.ContainsKey(connectionId))
        {
            return;
        }
        else
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("trickfarm-orleans.koreacentral.cloudapp.azure.com", 5000);
                TcpConnections[connectionId] = tcpClient;

                var cts = new CancellationTokenSource();
                var state = new ConnectionState
                {
                    TcpClient = tcpClient,
                    CancellationTokenSource = cts
                };

                state.ReceiveTask = Task.Run(() => ReceiveLoop(connectionId, tcpClient, cts.Token));
                Connections[connectionId] = state;
            }
            catch(Exception ex)
            {
                Console.WriteLine("TrickFarmWebApp에서 ConnectToTrickFarm 메서드 속 TcpClient 접속 실패");
                Console.WriteLine($"오류 발생: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("TrickFarmWebApp에서 ConnectToTrickFarm 메서드 종료");
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        if (Connections.TryGetValue(connectionId, out var state))
        {
            state.CancellationTokenSource.Cancel();

            try
            {
                await state.ReceiveTask!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{connectionId}] 수신 루프 종료 중 예외: {ex.Message}");
            }
            finally
            {
                state.TcpClient.Close();
                Connections.Remove(connectionId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task LoginToServer(string user_name)
    {
        var connectionId = Context.ConnectionId;

        if (TcpConnections.TryGetValue(connectionId, out var client))
        {
            var stream = client.GetStream();

            C2S_LOGIN_PACKET packet = new C2S_LOGIN_PACKET();
            packet.size = (byte)Marshal.SizeOf(typeof(C2S_LOGIN_PACKET));
            packet.type = (byte)PACKET_TYPE.C2S_LOGIN_USER;
            packet.user_name = Encoding.UTF8.GetBytes(user_name);

            byte[] buffer = StructureToByteArray(packet);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            await send_chat_to_webclient($"{user_name}님 접속 성공!");
        }
    }

    public async Task JoinToChatrrom(string request)
    {
        var connectionId = Context.ConnectionId;

        if (TcpConnections.TryGetValue(connectionId, out var client))
        {
            var stream = client.GetStream();

            string chatroom_name = request.Replace("join ", "");

            C2S_ENTER_CHATROOM_PACKET packet = new C2S_ENTER_CHATROOM_PACKET();
            packet.size = (byte)Marshal.SizeOf(typeof(C2S_ENTER_CHATROOM_PACKET));
            packet.type = (byte)PACKET_TYPE.C2S_ENTER_CHATROOM;
            packet.chatroom_name = Encoding.UTF8.GetBytes(chatroom_name);
            byte[] buffer = StructureToByteArray(packet);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            await send_chat_to_webclient($"{request.Replace("join ", "")}방에 어서오세요!");
        }
    }

    public async Task SendToServer(string message)
    {
        var connectionId = Context.ConnectionId;

        if (TcpConnections.TryGetValue(connectionId, out var client))
        {
            var stream = client.GetStream();

            C2S_MESSAGE_PACKET packet = new C2S_MESSAGE_PACKET();
            packet.size = (byte)Marshal.SizeOf(typeof(C2S_MESSAGE_PACKET));
            packet.type = (byte)PACKET_TYPE.C2S_CHAT_MESSAGE;
            packet.message = Encoding.UTF8.GetBytes(message);
            byte[] buffer = StructureToByteArray(packet);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }

    public async Task LogoutToServer()
    {
        var connectionId = Context.ConnectionId;

        if(TcpConnections.TryGetValue(connectionId, out var client))
        {
            var stream = client.GetStream();

            C2S_LEAVE_CHATROOM_PACKET packet = new C2S_LEAVE_CHATROOM_PACKET();
            packet.size = (byte)Marshal.SizeOf(typeof(C2S_LEAVE_CHATROOM_PACKET));
            packet.type = (byte)PACKET_TYPE.C2S_LEAVE_CHATROOM;
            packet.chatroom_name = Encoding.UTF8.GetBytes("leave");
            byte[] buffer = StructureToByteArray(packet);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            client.Dispose();
            Console.WriteLine($"{connectionId}님이 나가셨습니다.");
        }
    }

    private async Task ReceiveLoop(string ConnectionId, TcpClient client, CancellationToken token)
    {
        if (client is null || client.GetStream() is null && false == client.Connected)
        {
            Console.WriteLine($"[Log][ReceiveLoop] {ConnectionId} 연결이 끊어졌습니다.");
            return;
        }

        try
        {
            byte[] buffer = new byte[1024];
            while (!token.IsCancellationRequested)
            {
                int recv_len = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                if (recv_len <= 0) break;

                string response = Encoding.UTF8.GetString(buffer, 0, recv_len);

                if (GlobalHubContext.HubContext != null)
                {
                    // await GlobalHubContext.HubContext.Clients.Client(ConnectionId).SendAsync("ReceiveFromServer", response);
                    await send_chat_to_webclient(response);
                }
            }
        }
        catch (Exception ex)
        {
            client.Dispose();
            Console.WriteLine($"[Log][ReceiveLoop] {ex.Message}");
        }
    }

    private byte[] StructureToByteArray<T>(T obj) where T : struct
    {
        int size = Marshal.SizeOf(obj);
        byte[] bytes = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);

        try
        {
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return bytes;
    }


    private async Task send_chat_to_webclient(string formmat_message)
    {
        var connectionId = Context.ConnectionId;
        if (TcpConnections.TryGetValue(connectionId, out var client))
        {
            var stream = client.GetStream();
            var buffer = Encoding.UTF8.GetBytes(formmat_message);
            await Clients.Client(connectionId).SendAsync("ReceiveFromServer", formmat_message);
        }
    }
}
using Microsoft.AspNetCore.SignalR;
using System.Net.Sockets;

namespace TrickFarmWebApp.Hubs;

public class ChatHub : Hub
{
    public async Task ConnectToTrickFarm()
    {
        var connectionId = Context.ConnectionId;
        if (false == GlobalClientManager.ClientConnections.ContainsKey(connectionId))
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("trickfarm-orleans.koreacentral.cloudapp.azure.com", 5000);

                var cts = new CancellationTokenSource();
                var state = new ClientConnection
                {
                    TcpClient = tcpClient,
                    CancellationTokenSource = cts
                };

                GlobalClientManager.ClientConnections[connectionId] = state;
                Console.WriteLine($"[ChatHub] {connectionId} connected to TrickFarm and ReceiveLoop started.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatHub] ConnectToTrickFarm for {connectionId} failed: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"[ChatHub] ConnectToTrickFarm method finished for {connectionId}.");
            }
        }
    }

    public async Task LoginToServer(string user_name)
    {
        var connectionId = Context.ConnectionId;
        if (GlobalClientManager.ClientConnections.TryGetValue(connectionId, out ClientConnection? clientConnection))
        {
            clientConnection.TcpClient.Client.Send(StructureToByteArray(new C2S_LOGIN_PACKET
            {
                type = (byte)PACKET_TYPE.C2S_LOGIN_USER,
                size = (byte)Marshal.SizeOf(typeof(C2S_LOGIN_PACKET)),
                user_name = Encoding.UTF8.GetBytes(user_name)
            }));
        }
    }

    public async Task send_chat_to_webclient(string formmat_message)
    {
        var connectionId = Context.ConnectionId;
        if (GlobalClientManager.ClientConnections.ContainsKey(connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveChatFromServer", formmat_message);
        }
    }
}
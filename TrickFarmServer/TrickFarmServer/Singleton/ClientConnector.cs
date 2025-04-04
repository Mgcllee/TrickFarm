﻿using System.Collections.Concurrent;
using System.Net.Sockets;

public class ClientConnector : IClientConnector
{
    private readonly ConcurrentDictionary<Guid, TcpClient> clients = new();    

    public Guid add_client_connector(TcpClient client_socket)
    {
        Guid new_client_guid = Guid.NewGuid();
        clients.TryAdd(new_client_guid, client_socket);
        return new_client_guid;
    }
    public TcpClient? get_client_connector(Guid user_guid)
    {
        return clients.TryGetValue(user_guid, out var client_socket) ? client_socket : null;
    }

    public void disconnect_all_clients()
    {
        foreach (var client_socket in clients.Values)
        {
            client_socket.Dispose();
        }
    }
}
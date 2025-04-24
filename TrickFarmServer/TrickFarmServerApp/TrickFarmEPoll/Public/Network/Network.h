#pragma once

#include "stdafx.h"
#include "Public/FileDescriptor/FileDescriptor.h"

#define BLAZOR_ADDRESS "127.0.0.1"
#define BLAZOR_PORT 8081

#include <sys/socket.h>
#include <arpa/inet.h>
#include <netinet/in.h>
#include <sys/epoll.h>

#define LISTEN_BACKLOG 15

class Networker {
public:
    Networker();
    ~Networker();

    bool init_network_settings();
    void process_packet();
    bool accept_client();
    void process_request(int client_socket_fd);

private:
    int server_socket_fd;
    int epoll_fd;
};
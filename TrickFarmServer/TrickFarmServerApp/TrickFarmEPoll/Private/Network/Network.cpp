#pragma once

#include "Public/Network/Network.h"
#include "Network.h"


Networker::Networker()
{
    
}

Networker::~Networker()
{
    close(epoll_fd);
}

bool Networker::init_network_settings()
{
    server_socket_fd = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

    int flags = fcntl(server_socket_fd, F_GETFL);
    flags = flags | O_NONBLOCK; 
    if(fcntl(server_socket_fd, F_SETFL, flags) < 0) {
        printf("[Error][Networker::init_network_settings]: fail fcntl(server_socket_fd, ..)\n");
        return false;
    }

    struct sockaddr_in server_addr;
    memset(&server_addr, 0, sizeof(server_addr));
    server_addr.sin_family = AF_INET;
    server_addr.sin_port = htons(-1);
    server_addr.sin_addr.s_addr = htonl(INADDR_ANY);

    if(bind(server_socket_fd, (struct sockaddr*)&server_addr, sizeof(server_addr)) < 0) {
        close(server_socket_fd);
        printf("[Error][Networker::init_network_settings]: fail bind server_socket_fd\n");
        return false;
    }

    if(listen(server_socket_fd, LISTEN_BACKLOG) < 0) {
        close(server_socket_fd);
        printf("[Error][Networker::init_network_settings]: fail listen\n");
        return false;
    }

    epoll_fd = epoll_create1(EPOLL_CLOEXEC);
    if(epoll_fd == -1) {
        close(epoll_fd);
        close(server_socket_fd);
        printf("[Error][Networker::init_network_settings]: fail epoll_create1\n");
        return false;
    }

    return true;
}

void Networker::process_packet()
{
    epoll_event events;
    events.events = EPOLLIN | EPOLLET;
    events.data.fd = server_socket_fd;

    if(epoll_ctl(epoll_fd, EPOLL_CTL_ADD, server_socket_fd, &events) < 0) {
        close(epoll_fd);
        close(server_socket_fd);
        printf("[Error][Networker::process_packet]: fail epoll_ctl method\n");
        return;
    }

    const int MAX_EVENT = 1024;
    epoll_event epoll_events[MAX_EVENT];
    int event_cnt;
    int timeout = -1;

    while(true) {
        event_cnt = epoll_wait(epoll_fd, epoll_events, MAX_EVENT, timeout);
        if(event_cnt < 0) {
            printf("[Error][Network::process_packet]: fail epoll_wait method\n");
            break;
        }

        for(int i = 0; i < event_cnt; ++i) {
            if(epoll_events[i].data.fd == server_socket_fd) {
                accept_client();
            } else {
                process_request(epoll_events[i].data.fd);
            }
        }
    }

    close(epoll_fd);
    close(server_socket_fd);
}

bool Networker::accept_client()
{
    int tmp_client_fd;
    int tmp_client_len;
    sockaddr_in client_addr;

    tmp_client_len = sizeof(client_addr);
    tmp_client_fd = accept(server_socket_fd, (sockaddr*)&client_addr, (socklen_t*)&tmp_client_len);

    int flags = fcntl(tmp_client_fd, F_GETFL);
    flags = flags | O_NONBLOCK; 
    if(fcntl(tmp_client_fd, F_SETFL, flags) < 0) {
        printf("[Error][Networker::accept_client]: fail tmp_client_socket_fd flags\n");
        return false;
    }

    if(tmp_client_fd < 0) {
        printf("[Error][Networker::accept_client]: Invalid tmp_client_fd value\n");
        return false;
    }

    epoll_event client_event;
    client_event.events = EPOLLIN | EPOLLET;
    client_event.data.fd = tmp_client_fd;

    if(epoll_ctl(epoll_fd, EPOLL_CTL_ADD, tmp_client_fd, &client_event) < 0) {
        close(tmp_client_fd);
        printf("[Error][Network::accept_client]: fail epoll_ctl method\n");
        return false;
    }

    return true;
}

void Networker::process_request(int client_socket_fd)
{
    char request_buffer[1024];
    // int request_len = recv(client_socket_fd, request_buffer, sizeof(request_buffer));
    int request_len = read(client_socket_fd, request_buffer, sizeof(request_buffer));
    
    if(request_len < 0) {
        close(client_socket_fd);
        epoll_ctl(epoll_fd, EPOLL_CTL_DEL, client_socket_fd, NULL);
        printf("[Error][Networker::process_request]: Disconnect client socket\n");
        return;
    }

    int request_type;
    switch(request_type) {
        default: {
            break;
        }
    }
}

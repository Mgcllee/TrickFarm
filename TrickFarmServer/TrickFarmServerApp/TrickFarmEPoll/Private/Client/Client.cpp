#pragma once

#include "TrickFarmEPoll/Public/Client/Client.h"


Client::Client()
{
}

Client::~Client()
{
    close(m_socketfd);
}

void Client::Connect()
{
    m_socketfd = socket(AF_INET, SOCK_STREAM, 0);
    if (m_socketfd < 0) {
        std::cerr << "Error creating socket\n";
        return;
    }

    struct sockaddr_in server_addr;
    memset(&server_addr, 0, sizeof(server_addr));
    server_addr.sin_family = AF_INET;
    server_addr.sin_port = htons(BLAZOR_PORT);
    inet_pton(AF_INET, BLAZOR_ADDRESS, &server_addr.sin_addr);

    if (connect(m_socketfd, (struct sockaddr*)&server_addr, sizeof(server_addr)) < 0) {
        std::cerr << "Error connecting to server\n";
        close(m_socketfd);
        return;
    }
}

void Client::Disconnect()
{
    close(m_socketfd);
}


#pragma once

#include "Public/stdafx.h"
#include "Public/Network/Network.h"

class Client {
public:
    Client();
    ~Client();

    void Connect();
    void Disconnect();

    void Send(const std::string& message);
    std::string Receive();

private:
    int m_socketfd;
    
};
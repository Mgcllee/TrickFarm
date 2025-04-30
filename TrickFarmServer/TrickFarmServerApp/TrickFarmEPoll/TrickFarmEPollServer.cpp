#pragma once

#include "Public/FileDescriptor/SharedMemory.h"
#include "Public/Network/Network.h"

/*
[연결 순서]
1. 같은 컨테이너에서 동작하는 Orelans와 Posix Shared memory 연결
2. EPoll로 Blazor Server에서 요청하는 Client 접속 및 요청 처리
*/

int main() {
    // [Posix Shared memory]
    SharedMemory shm;
    
    // [EPoll Server]
    Networker networker;
    if (!networker.init_network_settings()) {
        printf("Failed to initialize network settings.");
        return -1;
    }

    while (true) {
        networker.process_packet();
    }

    return 0;
}
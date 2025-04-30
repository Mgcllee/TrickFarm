#pragma once 

#include "stdafx.h"
#include "Common/SHMCommon.h"
#include "Common/SHMClient.h"
#include "Public/FileDescriptor/FileDescriptor.h"

#include <semaphore.h>
#include <sys/mman.h>

class SharedMemory {
public:
    SharedMemory();
    ~SharedMemory();

    void notify_message_to_csharp(int uid, std::string message);
    void notify_message_to_cpp(int uid, std::string message);

private:
    static int shared_memory_fd;
    static SHM_Client* shm_clients;
    
    // 세마포어 최적화 필요 (동기화 병목 문제)
    static sem_t* sem_csharp;
    static sem_t* sem_cpp;
};
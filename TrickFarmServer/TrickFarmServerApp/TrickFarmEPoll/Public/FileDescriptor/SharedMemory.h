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

    void PostCpp(int uid);
    void WaitCpp(int uid);

private:
    static int shared_memory_fd;
    static SHM_Client* shm_clients = nullptr;
    
    // 세마포어 최적화 필요 (동기화 병목 문제)
    static sem_t* sem_csharp;
    static sem_t* sem_cpp;
};
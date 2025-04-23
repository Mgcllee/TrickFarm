#pragma once 

#include "Common/Common.h"
#include "Public/FileDescriptor/FileDescriptor.h"

#include <semaphore.h>
#include <sys/mman.h>

class SharedMemory {
public:
    SharedMemory(const char* name, size_t size);
    ~SharedMemory();

    void* GetMemory();
    void Post();
    void Wait();

    static int shared_memory_fd;
    
    // 세마포어 최적화 필요 (동기화 병목 문제)
    sem_t* sem_csharp;
    sem_t* sem_cpp;
};
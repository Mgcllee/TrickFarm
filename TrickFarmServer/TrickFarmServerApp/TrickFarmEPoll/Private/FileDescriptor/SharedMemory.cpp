#pragma once

#include "Public/FileDescriptor/SharedMemory.h"


SharedMemory::SharedMemory(const char *name, size_t size)
{
    shared_memory_fd = shm_open(SHM_NAME, O_CREAT | O_RDWR, 0666);
    ftruncate(shared_memory_fd, SHM_SIZE);
    void* ptr = mmap(0, SHM_SIZE, PROT_READ | PROT_WRITE, MAP_SHARED, shared_memory_fd, 0);

    sem_csharp = sem_open(SEM_CS_TURN, O_CREAT, 0666, 0);
    sem_cpp = sem_open(SEM_CPP_TURN, O_CREAT, 0666, 0);
}
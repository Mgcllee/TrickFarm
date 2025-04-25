#pragma once

#include "Public/FileDescriptor/SharedMemory.h"


SharedMemory::SharedMemory()
{
    shared_memory_fd = shm_open(SHM_NAME, O_CREAT | O_RDWR, 0666);
    ftruncate(shared_memory_fd, SHM_SIZE);
    
    sem_csharp = sem_open(SEM_CS_TURN, O_CREAT, 0666, 0);
    sem_cpp = sem_open(SEM_CPP_TURN, O_CREAT, 0666, 0);
    
    data = (char*)mmap(0, SHM_SIZE, PROT_READ | PROT_WRITE, MAP_SHARED, shared_memory_fd, 0);
}

SharedMemory::~SharedMemory()
{
    munmap(data, SHM_SIZE);
    close(shared_memory_fd);
    shm_unlink(SHM_NAME);
    sem_close(sem_csharp);
    sem_close(sem_cpp);
    sem_unlink(SEM_CS_TURN);
    sem_unlink(SEM_CPP_TURN);
}

void SharedMemory::PostCpp(int uid)
{
    // TODO: Add Thread Safe logic
    int t = 0;
    for(; t < 10; ++t) {
        if(data[uid] != 0) usleep(100);
        else break;
    }
    if(t == 10 && data[uid] != 0) {
        printf("[Error][SharedMemory::PostCpp] data[uid]: %d\n", (int)data[uid]);
    }
    else {
        data[uid] = 1;
        
        uint32_t packet_len;
        memcpy(data + uid + 1, &packet_len, 4);
        uint32_t packet_type;
        memcpy(data + uid + 5, &packet_type, 4);

        // memcpy(data + uid + 9, &packet, packet_len);

        // Notify C#
        sem_post(sem_csharp);
    }
}

void SharedMemory::WaitCpp(int uid)
{
    // wait for C# response
    sem_wait(sem_cpp);

    uint32_t packet_len;
    memcpy(&packet_len, data + uid + 1, 4);
    
    uint32_t packet_type;
    memcpy(&packet_type, data + uid + 5, 4);

    switch(packet_type) {
        default: {
            break;
        }
    }

    data[uid] = 0;
}
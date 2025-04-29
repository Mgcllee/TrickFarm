#pragma once

#include "Public/FileDescriptor/SharedMemory.h"


SharedMemory::SharedMemory()
{
    shared_memory_fd = shm_open(SHM_NAME, O_CREAT | O_RDWR, 0666);
    ftruncate(shared_memory_fd, SHM_SIZE);
    
    sem_csharp = sem_open(SEM_CS_TURN, O_CREAT, 0666, 0);
    sem_cpp = sem_open(SEM_CPP_TURN, O_CREAT, 0666, 0);
    
    shm_clients = static_cast<SHM_Client*>(mmap(0, SHM_SIZE, PROT_READ | PROT_WRITE, MAP_SHARED, shared_memory_fd, 0));
}

SharedMemory::~SharedMemory()
{
    munmap(shm_clients, SHM_SIZE);
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
        if(shm_clients[uid] != 0) usleep(100);
        else break;
    }
    if(t == 10 && shm_clients[uid] != 0) {
        printf("[Error][SharedMemory::PostCpp] shm_clients[uid]: %d\n", uid);
    }
    else {
        shm_clients[uid] = 1;
        
        uint32_t packet_len;
        memcpy(shm_clients + uid + 1, &packet_len, 4);
        uint32_t packet_type;
        memcpy(shm_clients + uid + 5, &packet_type, 4);

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
    memcpy(&packet_len, shm_clients + uid + 1, 4);
    
    uint32_t packet_type;
    memcpy(&packet_type, shm_clients + uid + 5, 4);

    switch(packet_type) {
        default: {
            break;
        }
    }

    shm_clients[uid] = 0;
}
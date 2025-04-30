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

void SharedMemory::notify_message_to_csharp(int uid, std::string message)
{
    // TODO: Add Thread Safe logic
    int t = 0;
    for(; t < 10; ++t) {
        if(shm_clients[uid].turn != SHM_CLIENT_TYPE::turn_cpp) usleep(100);
        else break;
    }
    if(t == 10 && shm_clients[uid].turn != SHM_CLIENT_TYPE::turn_cpp) {
        printf("[Error][SharedMemory::PostCpp] shm_clients[uid]: %d\n", uid);
    }
    else {
        memcpy(&shm_clients[uid].message, &message, message.length() + 1);
        shm_clients[uid].turn = SHM_CLIENT_TYPE::turn_csharp;
        sem_post(sem_csharp); // Notify C#
    }
}

void SharedMemory::notify_message_to_cpp(int uid, std::string message)
{
    sem_wait(sem_cpp);  // wait for C# response
    strcpy(shm_clients[uid].username, message.c_str());
    shm_clients[uid].turn = SHM_CLIENT_TYPE::turn_csharp;
}
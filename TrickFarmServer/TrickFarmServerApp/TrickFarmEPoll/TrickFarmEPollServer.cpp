// cpp_epoll_ipc.cpp
#include <fcntl.h>
#include <sys/mman.h>
#include <semaphore.h>
#include <unistd.h>
#include <cstring>
#include <iostream>
#include <sys/epoll.h>

#include "Common/Common.h"

const char* sem_cpp2csharp = "/sem_c2csharp";
const char* sem_csharp2cpp = "/sem_csharp2c";

int main() {
    int shm_fd = shm_open(SHM_NAME, O_CREAT | O_RDWR, 0666);
    ftruncate(shm_fd, SHM_SIZE);
    void* ptr = mmap(0, SHM_SIZE, PROT_READ | PROT_WRITE, MAP_SHARED, shm_fd, 0);

    sem_t* sem_csharp = sem_open(sem_cpp2csharp, O_CREAT, 0666, 0);
    sem_t* sem_cpp = sem_open(sem_csharp2cpp, O_CREAT, 0666, 0);

    

    char* data = (char*)ptr;
    for (int i = 0; i < 10; ++i) {

        // Using Async EPoll
        
        // Recv TCP data packet

        while (data[0] != 0) usleep(100);
        data[0] = 1;  // writing

        std::string msg = "Hello from C++ #" + std::to_string(i);
        uint32_t len = msg.size();
        memcpy(data + 1, &len, 4);
        memcpy(data + 5, msg.c_str(), len);

        sem_post(sem_csharp);  // [notify C#]
        sem_wait(sem_cpp);     // [wait for response]

        uint32_t res_len;
        memcpy(&res_len, data + 1, 4);
        std::string response(data + 5, res_len);
        std::cout << "[C++] Got response: " << response << "\n";

        data[0] = 0;  // reset flag
    }

    munmap(ptr, SHM_SIZE);
    close(shm_fd);
    shm_unlink(SHM_NAME);
    sem_close(sem_csharp);
    sem_close(sem_cpp);
    sem_unlink(sem_cpp2csharp);
    sem_unlink(sem_csharp2cpp);
}
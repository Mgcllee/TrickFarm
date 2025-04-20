// cpp_epoll_ipc.cpp
#include <fcntl.h>
#include <sys/mman.h>
#include <semaphore.h>
#include <unistd.h>
#include <cstring>
#include <iostream>

const char* shm_name = "/ipc_shm";
const char* sem_c2csharp = "/sem_c2csharp";
const char* sem_csharp2c = "/sem_csharp2c";
const size_t shm_size = 1024;

int main() {
    int shm_fd = shm_open(shm_name, O_CREAT | O_RDWR, 0666);
    ftruncate(shm_fd, shm_size);
    void* ptr = mmap(0, shm_size, PROT_READ | PROT_WRITE, MAP_SHARED, shm_fd, 0);

    sem_t* sem_csharp = sem_open(sem_c2csharp, O_CREAT, 0666, 0);
    sem_t* sem_cpp = sem_open(sem_csharp2c, O_CREAT, 0666, 0);

    char* data = (char*)ptr;
    for (int i = 0; i < 10; ++i) {
        while (data[0] != 0) usleep(100);  // flag != 0이면 대기
        data[0] = 1;  // writing

        std::string msg = "Hello from C++ #" + std::to_string(i);
        uint32_t len = msg.size();
        memcpy(data + 1, &len, 4);
        memcpy(data + 5, msg.c_str(), len);

        sem_post(sem_csharp);  // notify C#
        sem_wait(sem_cpp);     // wait for response

        uint32_t res_len;
        memcpy(&res_len, data + 1, 4);
        std::string response(data + 5, res_len);
        std::cout << "[C++] Got response: " << response << "\n";

        data[0] = 0;  // reset flag
    }

    munmap(ptr, shm_size);
    close(shm_fd);
    shm_unlink(shm_name);
    sem_close(sem_csharp);
    sem_close(sem_cpp);
    sem_unlink(sem_c2csharp);
    sem_unlink(sem_csharp2c);
}

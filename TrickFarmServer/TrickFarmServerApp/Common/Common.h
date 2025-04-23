#ifndef SHARED_DEFS_H
#define SHARED_DEFS_H

#include <sys/time.h>

const char* SHM_NAME = "/EPollOrleans_shm";
const char* SEM_MUTEX_NAME = "/EPollOrleans_mutex";
const char* SEM_CPP_TURN = "/EPollOrleans_cpp_turn";
// const char* sem_csharp2cpp = "/sem_csharp2c";
const char* SEM_CS_TURN = "/EPollOrleans_cs_turn";
// const char* sem_cpp2csharp = "/sem_c2csharp";


const size_t SHM_SIZE = 1024;

const int MESSAGE_BUFFER_SIZE = 200;
const int USER_NAME_SIZE = 10;

enum class C2S_TYPE {
    c2s_request_login,
    c2s_send_chat_message,
};

enum class S2C_TYPE {
    s2c_result_login,
    s2c_multicast_chat_message,
};

struct BASICPacket {
    char type;
    char size;
};

struct C2SRequestLoginPacket : BASICPacket {
    char user_name[USER_NAME_SIZE];
};

struct S2CResultLoginPacket : BASICPacket {
    // reason for login fail
    char login_result[USER_NAME_SIZE];
};

struct C2SSendChatMessagePacket : BASICPacket {
    // logging for server database
    struct timeval timestamp;
    char message[MESSAGE_BUFFER_SIZE];
};

struct S2CMulticastChatMessagePacket : BASICPacket {
    char message[MESSAGE_BUFFER_SIZE];
};

#endif
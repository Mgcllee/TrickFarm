#pragma once

#include "Common/SHMCommon.h"

#pragma pack(push, 1)

enum class C2S_TYPE {
    c2s_request_login,
    c2s_send_chat_message,
};

enum class S2C_TYPE {
    s2c_result_login,
    s2c_multicast_chat_message,
};

#pragma pack(push, 1)

struct BASICPacket {
    char type;
    char size;
};

struct C2SRequestLoginPacket : BASICPacket {
    char user_name[USER_NAME_SIZE];
};

struct S2CResultLoginPacket : BASICPacket {
    char login_result[USER_NAME_SIZE];
};

struct C2SSendChatMessagePacket : BASICPacket {
    char message[MESSAGE_BUFFER_SIZE];
};

struct S2CMulticastChatMessagePacket : BASICPacket {
    char message[MESSAGE_BUFFER_SIZE];
};

#pragma pack(pop)
#pragma once

#include "Common/SHMCommon.h"

enum SHM_CLIENT_TYPE
{
    turn_cpp = 0,
    turn_csharp = 1
};

struct SHM_Client {
    char turn;
    char username[USER_NAME_SIZE];
    char message[MESSAGE_BUFFER_SIZE];
};

const int MAX_CLIENTS = 128;
const int SHM_SIZE = MAX_CLIENTS * sizeof(SHM_Client);
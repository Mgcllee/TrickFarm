#pragma once

#include "Common/SHMCommon.h"

struct SHM_Client {
    char username[USER_NAME_SIZE];
    char message[MESSAGE_BUFFER_SIZE];
};

const int MAX_CLIENTS = 128;
const int SHM_SIZE = MAX_CLIENTS * sizeof(SHM_Client);
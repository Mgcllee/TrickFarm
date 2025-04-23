#pragma once

#include "Public/FileDescriptor/FileDescriptor.h"

#define BLAZOR_ADDRESS "127.0.0.1"
#define BLAZOR_PORT 8081

#include <sys/socket.h>
#include <arpa/inet.h>
#include <netinet/in.h>
#include <sys/epoll.h>
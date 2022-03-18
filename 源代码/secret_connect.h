#ifndef SECRET_CONNECT
#define SECRET_CONNECT
#include <stdio.h>
#include <netdb.h>
#include <unistd.h>
#include <iostream>
#include <stdlib.h>
#include <errno.h>
#include <net/if.h>
#include <string.h>
#include <sys/types.h>
#include <netinet/in.h>
#include <sys/socket.h>
#include <sys/wait.h>
#include <arpa/inet.h>
#include <memory.h>
#include <sys/ioctl.h>
#include "DES.h"

#define BUFFERSIZE 1024
using namespace std;

void SecretChat(int nSock, char *pRemoteName, char *pKey);

size_t TotalRecv(int s, void *buf, size_t len, int flags);

#endif
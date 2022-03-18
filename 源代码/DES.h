#ifndef DES_H
#define DES_H
#include<iostream>
#include<string.h>
#include <stdio.h>
#include <sys/socket.h>
#include <sys/types.h>
#include <cstring>
using namespace std;
typedef int INT32;
typedef uint32_t ULONG32;
typedef uint8_t ULONG8;




class CDesOperate{
private:
    	ULONG32 m_arrOutKey[16][2];
        ULONG32 m_arrBufKey[2];

        INT32 HandleData(ULONG32 *left, ULONG8 choice);
        INT32 MakeData(ULONG32 *left, ULONG32 *right, ULONG32 number);
        INT32 MakeKey(ULONG32 *keyleft, ULONG32 *keyright, ULONG32 number);
        INT32 MakeFirstKey(ULONG32 *keyP);

public:
    CDesOperate();
    INT32 Encry(char *pPlaintext, int nPlaintextLength, 
    char *pCipherBuffer, int &nCipherBufferLength, char *pKey, int nKeyLength);
    INT32 Decry(char *pCipher, int nCipherBufferLength,
    char *pPlaintextBuffer, int &nPlaintextBufferLength, char *pKey, int nKeyLength);
};

#endif
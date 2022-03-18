#include"secret_connect.h"
using namespace std;

char strStdinBuffer[BUFFERSIZE];
char strEncryBuffer[BUFFERSIZE];
char strSocketBuffer[BUFFERSIZE];
char strDecryBuffer[BUFFERSIZE];

void SecretChat(int nSock, char *pRemoteName, char *pKey){

    //cout << pRemoteName << endl;
    CDesOperate cDes;
    //cout << pKey << endl;
    int klength = strlen(pKey);
    if(klength != 8){
        printf("Key length error\n");
        return;
    }

    pid_t nPid;
    nPid = fork();
    //cout << nPid << endl;
    if(nPid != 0){
        while(1){
            
            bzero(&strSocketBuffer, BUFFERSIZE);
            int nLength = 0;
            nLength = TotalRecv(nSock, strSocketBuffer, BUFFERSIZE, 0);
            if(nLength !=BUFFERSIZE)
            {
                break;
            }
            else{
                //cout << "come on\n" << endl;
                
                int nLen = BUFFERSIZE;
                cDes.Decry(strSocketBuffer,BUFFERSIZE,strDecryBuffer,nLen,pKey,8);
                strDecryBuffer[BUFFERSIZE-1]=0;
                if(strDecryBuffer[0]!=0&&strDecryBuffer[0]!='\n'){
                    printf("Receive message form <%s>: %s\n",
                    pRemoteName,strDecryBuffer);
                    if(0==memcmp("quit",strDecryBuffer,4))
                    {
                        printf("Quit!\n");
                        break;
                    }
                }

            }
        }
    }else{
        while(1){
            bzero(&strStdinBuffer, BUFFERSIZE);
            while(strStdinBuffer[0]==0)
            {
                //cout << "here?" << endl;
                if (fgets(strStdinBuffer, BUFFERSIZE, stdin) == NULL)
                {
                    //printf("say something");
                    continue;
                }
                //cout << "here?" << endl;
                int nLen = BUFFERSIZE;
                
                cDes.Encry(strStdinBuffer,BUFFERSIZE,strEncryBuffer,nLen,pKey,8);
                //cout << "here" << endl;
                if(send(nSock, strEncryBuffer, BUFFERSIZE,0)!=BUFFERSIZE)
                {
                    perror("send");
                }
                else
                {
                    if(0==memcmp("quit",strStdinBuffer,4))
                    {
                        printf("Quit!\n");
                        break;
                    }
                }
            }
        }
    }
}

size_t TotalRecv(int s, void *buf, size_t len, int flags){
    size_t nCurSize = 0;
    while(nCurSize <len)
    {
        ssize_t nRes = recv(s,((char*)buf)+nCurSize,len-nCurSize,flags);
        if(nRes<0||nRes+nCurSize>len)
        {
            return -1;
        }
        nCurSize+=nRes;
    }
    return nCurSize;
}

#include<iostream>
#include"secret_connect.h"
using namespace std;

int main(){
    cout << "Client or Server?" << endl;
    char choose;
    cin >> choose;
    char keyO[] = "benbenmi";
    if(choose == 's'){
        int nListenSocket,nAcceptSocket;
        struct sockaddr_in sLocalAddr, sRemoteAddr;
        bzero(&sLocalAddr, sizeof(sLocalAddr));
        sLocalAddr.sin_family = PF_INET;
        sLocalAddr.sin_port = htons(6000);
        sLocalAddr.sin_addr.s_addr = INADDR_ANY;
        if ((nListenSocket = socket(PF_INET, SOCK_STREAM, 0)) == -1){
            perror("socket");
            exit(1);
        }
        if(bind(nListenSocket, (struct sockaddr *)&sLocalAddr, sizeof(struct sockaddr)) == -1){
            perror("bind");
            exit(1);
        }
        if(listen(nListenSocket, 5) == -1){
            perror("listen");
            exit(1);
        }
        printf("Listening...\n");
        socklen_t nLength = 0;
        nAcceptSocket = accept(nListenSocket, (struct sockaddr*)&sRemoteAddr, &nLength);
        close(nListenSocket);
        printf("server: got connection from %s, port %d, socket %d\n",inet_ntoa(sRemoteAddr.sin_addr), ntohs(sRemoteAddr.sin_port), nAcceptSocket);
        SecretChat(nAcceptSocket, inet_ntoa(sRemoteAddr.sin_addr), "benbenmi");
        close(nAcceptSocket);
    }
    else if(choose == 'c'){
        cout << "Please input the server address:" << endl;
        char strIPAddr[16];
        cin >> strIPAddr;
        int nConnectSocket, nLength;
        struct sockaddr_in sDestAddr;
        if((nConnectSocket = socket(AF_INET,SOCK_STREAM, 0)) < 0){
            perror("Socket");
            exit(errno);
        }
        int SEVERPORT = 6000;

        sDestAddr.sin_family = AF_INET;
        sDestAddr.sin_port = htons(SEVERPORT);
        sDestAddr.sin_addr.s_addr = inet_addr(strIPAddr);
        if(connect(nConnectSocket, (struct sockaddr *) &sDestAddr, sizeof(sDestAddr)) != 0){
            perror("Connect");
            exit(errno);
        }else{
            printf("Connect Success! \nBegin to chat...\n");
            SecretChat(nConnectSocket, strIPAddr, "benbenmi");
        }
        close(nConnectSocket);
    }else{
        cout << "Error!" << endl;
    }
    return 0;
}
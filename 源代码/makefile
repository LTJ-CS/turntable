CFLAGS := -Wall -Wextra -std=c++11 # without debug

pro:
	g++ $(CFLAGS) -g chat.cpp secret_connect.cpp DES.cpp -o chat

secret:
	g++ $(CFLAGS) secret_connect.cpp -o secret.o

DES:
	g++ $(CFLAGS) DES.cpp -o DES.o

.PHONY: run
run:
	./chat

# 生成到Code目录下 (客户端用)
#protoc --csharp_out=./Code/C# *.proto
protoc --csharp_out=../../../game-journey-client/client/Assets/Sdk/Runtime/Net/protobuf *.proto

# 生成到项目目录下 （服务器）  
protoc --csharp_out=../../../game-journey-orleans/GameCommon/Message/Protobuf *.proto
protoc --go_out=../../../game-journey-go *.proto
# 生成服务器专用协议 
protoc --csharp_out=../../../game-journey-orleans/GameCommon/Message/Protobuf Server/*.proto
protoc --go_out=../../../game-journey-go Server/*.proto
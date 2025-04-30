# 配置文件生成工具
需要安装 dotnet7.0sdk
## 程序生成配置文件
运行 build.sh 
## 策划自测
- 进入 docker-compose 目录，在 powershell 下运行下列命令,关闭运行中的测试服务器
```shell
docker-compose down
```
- 之后回到上层目录，运行 build.bat，根据新的 xlsx 文件生成新的服务器客户端配置
- 之后再次进入 docker-compose 目录，在 powershell 下运行下列命令，开启测试服务器
```shell
docker-compose up -d
```
- 更新客户端，配置应该就生效了
- GM工具地址为localhost/swagger
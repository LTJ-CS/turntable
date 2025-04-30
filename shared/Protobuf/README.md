Protobuf 生成
&emsp;&emsp; 最外层是客户端使用的proto文件，里面有客户端需要用到的接口定义。

| 目录     | 说明         |
|--------|------------|
| Code   | 生成的C#和go代码 |
| Server | 服务器专用      |

| Proto文件         | 说明      | 消息ID范围        |
|-----------------|---------|---------------|
| common.proto    | 公共结构体   |               |
| login.proto     | 登录初始化相关 | 10001 ~ 10200 |
| level.proto     | 关卡      | 10201 ~ 10400 |
| activity.proto  | 活动      | 10401 ~ 10800 |
| cucoloris.proto | 剪影模式    | 10801 ~ 10900 |

## 使用

1. 本地安装Protobuf工具
2. 运行[build.sh](build.sh)


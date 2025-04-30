namespace Sdk.Runtime.Platforms
{
    /// <summary>
    /// 定义主机的运行平台
    /// </summary>
    public enum EHostPlatform
    {
        Invalid = -1, // 无效值
        Editor = 0,   // 编辑器
        Web,          // 网页 h5
        Android,      // 原生安卓
        IOS,          // 原生 ios
        Wx,           // 微信小游戏
        Tt,           // 字节小游戏（支持不全）
        Oppo,         // oppo 小游戏
        Vivo,         // vivo 小游戏
        Qq,           // qq 小游戏
        Ks,           // 快手 小游戏
        Hw,           // 华为 小游戏
        Alipay,       // 支付宝 小游戏
    }
}
/// <summary>
/// 资源的配置项
/// </summary>
// ReSharper disable once CheckNamespace
public static class ResCfg
{
    /// <summary>
    /// 定义资源包远程根路径, 不同项目肯定不一样
    /// </summary>
    public const string ResourceServerUrl =
        #if ENV_PUBLISH // 正式服的资源服务器地址
        "http://192.168.31.116:8080";
        #else // 测试服的资源服务器地址
        "http://192.168.31.116:8080";
        #endif

    /// <summary>
    /// 运行时 Addressable Asset 的根路径, 一般由补丁服务器返回后填充
    /// </summary>
    public static string AAUrl;

    /// <summary>
    /// cdn下载地图的 url
    /// </summary>
    public static string CdnMapUrl = "https://incubator-static.easygame2021.com/game-journey/";
    
    /// <summary>
    /// 根据给定的资源版本号返回对应的 Addressable Asset 配置文件名称
    /// </summary>
    /// <param name="resVer"></param>
    /// <returns></returns>
    public static string GetAASettingsPath(int resVer)
    {
        return $"settings_{resVer}.json";
    }
}
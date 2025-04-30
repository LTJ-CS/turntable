using System;
using Sdk.Runtime.Base;
using UnityEngine;

public static class LaunchSettings
{
    public enum EnvironmentType
    {
        Development, // 局域网
        Testing,     // 内网
        Beta,        // 测试服 测试appid
        Production,  // 正式服 正式appid
        Panda,       // 熊猫
    }

    public static readonly string RemoteCdnDomain = "https://test-static.easygame2021.com/game-journey/";

    public static readonly string localCdnDomain = "http://localhost:8082/";

#if UNITY_EDITOR
    public static readonly string EnvKey = "LaunchSettings.environmentType";
#endif

    static LaunchSettings()
    {
        Initialize();
        InitCdn();
    }
    /// <summary>
    /// 运行时环境类型，如果打包时需要环境类型，直接读取GameVersion.GameEnv;
    /// </summary>
    /// <exception cref="Exception"></exception>
    public static EnvironmentType environmentType
    {
        get
        {
#if UNITY_EDITOR
            int envType = UnityEditor.EditorPrefs.GetInt(EnvKey, 0);
            if (!Enum.IsDefined(typeof(EnvironmentType), envType))
                throw new Exception("EnvironmentType is not defined");
            return (EnvironmentType)envType;

#else
            return GameVersion.GameEnv;
#endif
        }
    }

    // 熊猫本地测试服
    public static EnvironmentSettings pandaSettings;

    public static EnvironmentSettings developmentSettings;

    public static EnvironmentSettings testingSettings;

    public static EnvironmentSettings betaSettings;

    public static EnvironmentSettings productionSettings;

    public static EnvironmentSettings GetCurEnvironmentSettings()
    {
        return GetEnvironmentSettings(environmentType);
    }

    public static EnvironmentSettings GetEnvironmentSettings(EnvironmentType type)
    {
        return type switch
        {
            EnvironmentType.Panda       => pandaSettings,
            EnvironmentType.Development => developmentSettings,
            EnvironmentType.Testing     => testingSettings,
            EnvironmentType.Beta        => betaSettings,
            EnvironmentType.Production  => productionSettings,
            _                           => throw new InvalidCastException("EnvironmentType is not defined")
        };
    }

    public static bool IsLocalCdn(string cdn)
    {
        if (!string.IsNullOrEmpty(cdn) && (cdn.Contains("192.") || cdn.Contains("127.0.0.1") || cdn.Contains("localhost")))
        {
            return true;
        }

        return false;
    }
    public static string GetCdnSuffix()
    {
        //如果是本地IP地址，则cdn后缀是空
        string cdn = GetCurEnvironmentSettings().cdnDomain;
        if (IsLocalCdn(cdn))
            return "";
        string suffix = string.Empty;

#if PLATFORM_WEIXIN
              suffix = "wx";
#elif PLATFORM_DOUYIN
              suffix = "tt";
#elif UNITY_EDITOR
        suffix = "";
#else
              Debug.LogError("检查下当前平台下cdn的后缀");//如果没有宏，代码走到这句，就会有编译问题，检查宏
#endif
        if (LaunchSettings.environmentType != LaunchSettings.EnvironmentType.Production)
        {
            if (!string.IsNullOrEmpty(suffix))
                suffix += "-test";
        }

        return suffix;
    }

    public static string GetEnvironmentName()
    {
        return environmentType switch
        {
            EnvironmentType.Development => "局域网",
            EnvironmentType.Testing     => "内网",
            EnvironmentType.Beta        => "测试服",
            EnvironmentType.Production  => "正式服",
            EnvironmentType.Panda       => "熊猫",
            _                           => null
        };
    }


    static void Initialize()
    {
        pandaSettings = new EnvironmentSettings
                        {
                            gameId = "6736eb89c2c8fb9f4692f05e",
                            loginDomain = "https://beta-game-account.easygame2021.com/user/login",
                            gameDomain = "http://192.168.31.216:8090/journey/"
                        };
        developmentSettings = new EnvironmentSettings
                              {
                                  gameId = "668273437cea09295c1074cd",
                                  loginDomain = "http://api.local.easygame2021.com/user/login",
                                  gameDomain = "http://192.168.31.62:8080/sheep/"
                              };

        testingSettings = new EnvironmentSettings
                          {
                              gameId = "6736eb89c2c8fb9f4692f05e",
                              loginDomain = "https://beta-game-account.easygame2021.com/user/login",
                              gameDomain = "http://game-pet.local.com/journey/"
                          };

        betaSettings = new EnvironmentSettings
                       {
                           gameId = "6736eb89c2c8fb9f4692f05e",
                           loginDomain = "https://beta-game-account.easygame2021.com/user/login",
                           gameDomain = "https://beta-journey.easygame2021.com/journey/"
                       };

        productionSettings = new EnvironmentSettings
                             {
                                 gameId = "6764db5ce5cc81180ed61d8a",
                                 loginDomain = "https://game-account.easygame2021.com/user/login",
                                 gameDomain = "https://game-journey.easygame2021.com/journey/"
                             };
    }
    /// <summary>
    ///   //#Start    //#End 行不要修改，也不要添加新的东西，这两行是为了代码修改脚本时，检测到需要修改代码的地方
    /// cdn的初始化单独拎出来，方便编辑器代码修改
    /// </summary>
    //#Start
    static void InitCdn()
    {
        pandaSettings.cdnDomain = "http://localhost:8082/";
        developmentSettings.cdnDomain = "http://localhost:8082/";
        testingSettings.cdnDomain = "http://192.168.31.15:8082/";
        betaSettings.cdnDomain = "https://test-static.easygame2021.com/game-journey/test/";
        productionSettings.cdnDomain = "https://incubator-static.easygame2021.com/game-journey/";
    }
    //#End
}

public class EnvironmentSettings
{
    public string gameId;
    public string cdnDomain;
    public string loginDomain;
    public string gameDomain;
    public override string ToString()
    {
        return $"gameId:{gameId},\ncdnDomain:{cdnDomain}, \nloginDomain:{loginDomain}, \ngameDomain:{gameDomain}";
    }
}

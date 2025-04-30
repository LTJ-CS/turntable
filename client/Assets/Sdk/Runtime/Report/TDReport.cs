using System;
using System.Collections.Generic;
using LitJson;
using Sdk.Runtime.Base;
using ThinkingAnalytics;
using ThinkingData.Analytics;
using UnityEngine;
using UnityEngine.PlayerLoop;


public class TdReport
{
    private static TdReport _instance;

    public static TdReport Instance => _instance ??= new TdReport();

    private GameObject target;

    public void Init()
    {
        if (target)
        {
            TDAnalytics.Logout();
            return;
        }

        const string appId = "2c7d7aad92d44a778366f9f93f59551f";
        const string serverUrl = "https://shu.easygame2021.com";
        target = new GameObject("ThinkingAnalytics", typeof(TDAnalytics));
        TDAnalytics.Init(appId, serverUrl);
        TDAnalytics.EnableLog(false);
        SetSuperProperties();
    }

    // private 

    public void Login(string account)
    {
#if (PLATFORM_DOUYIN || PLATFORM_WEIXIN) && !UNITY_EDITOR
        if (target == null) Init();
        TDAnalytics.Login(account);
#endif
    }

    /// <summary>
    /// 设置公共事件属性
    /// </summary>
    private void SetSuperProperties()
    {
        Dictionary<string, object> superProperties = new Dictionary<string, object>()
                                                     {
                                                         { "runtime_environment", LaunchSettings.environmentType },
                                                         { "client_version", GameVersion.GameVer },
                                                         { "gold", 0 },
                                                         { "energy", 0 },
                                                     };
// #if PLATFORM_DOUYIN
//         superProperties["platform"] = "tt";
// #elif PLATFORM_WEIXIN
//         superProperties["platform"] = "wx";
// #endif
        TDAnalytics.SetSuperProperties(superProperties);
    }

    public static void SetSuperAttr(long gold, long energy)
    {
        Dictionary<string, object> superProperties = new Dictionary<string, object>()
                                                     {
                                                         { "runtime_environment", LaunchSettings.environmentType },
                                                         { "client_version", GameVersion.GameVer },
                                                         { "gold", gold },
                                                         { "energy", energy },
                                                     };
// #if PLATFORM_DOUYIN
//         superProperties["platform"] = "tt";
// #elif PLATFORM_WEIXIN
//         superProperties["platform"] = "wx";
// #endif
        TDAnalytics.SetSuperProperties(superProperties);
    }


    /// <summary>
    /// 上报埋点
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="properties">参数</param>
    /// <param name="appId">appId，默认上报不需要填</param>
    public static void Report(string eventName, Dictionary<string, object> properties = null, string appId = "")
    {
        // TDAnalytics的上传log包含Send event Request，这里加这个是为了日志能一起过滤出来
#if UNITY_EDITOR
        Debug.Log($">>>>上报事件 Send event Request {eventName} {(properties == null ? "null" : JsonMapper.ToJson(properties))}");
#endif

#if (PLATFORM_DOUYIN || PLATFORM_WEIXIN) && !UNITY_EDITOR
        TDAnalytics.Track(eventName, properties, appId);
#endif
    }
}
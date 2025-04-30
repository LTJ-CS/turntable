using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AccountProto;
using Cysharp.Threading.Tasks;
using GameScript.Runtime.GameLogic;
using GameScript.Runtime.Platform;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using LitJson;
using MyUI;
using Sdk.Runtime;
using UnityEngine;
using UnityEngine.Networking;

public static class NetManager
{
    public static readonly string protocolVersion   = "v1/";
    public static readonly string logicProtocolPath = "client/lobby/v1";
    public static readonly string adReportPath      = "game/report/event";
    public static readonly string serverTimePath    = "server_time";

    public static string token;

    private static string GetUrl(string requestPath)
    {
        string domain = LaunchSettings.GetCurEnvironmentSettings().gameDomain;
        // return domain + protocolVersion + requestPath;
        return domain + requestPath;
    }

    private static string GetLoginUrl()
    {
        return LaunchSettings.GetCurEnvironmentSettings().loginDomain;
    }

    /// <summary>
    /// 获取上报地址
    /// </summary>
    /// <returns></returns>
    private static string GetReportUrl()
    {
        return LaunchSettings.GetCurEnvironmentSettings().gameDomain + protocolVersion + adReportPath;
    }

    /// <summary>
    /// 获取邀请码状态地址,删除了登录地址最后的login
    /// </summary>
    /// <returns></returns>
    private static string GetReqStatusUrl()
    {
        string url = GetLoginUrl();
        return url.Substring(0, url.Length - 5) + "promoter_invite_code/get_bind_status";
    }

    /// <summary>
    /// 获取绑定地址,删除了登录地址最后的login
    /// </summary>
    /// <returns></returns>
    private static string GetBindStatusUrl()
    {
        string url = GetLoginUrl();
        return url.Substring(0, url.Length - 5) + "promoter_invite_code/bind";
    }


    public static AccountProto.Login.Types.Platform GetPlatform()
    {
        var platform = AccountProto.Login.Types.Platform.Beta;
#if UNITY_EDITOR || UNITY_ANDROID && !PLATFORM_DOUYIN
        platform = AccountProto.Login.Types.Platform.Beta;
#elif PLATFORM_DOUYIN
        platform = AccountProto.Login.Types.Platform.Tt;
#elif PLATFORM_WEIXIN
        platform = AccountProto.Login.Types.Platform.Wx;
#endif
        return platform;
    }



    private static async UniTask<(WebRequestResult webRequestResult, string error, JsonData jsonData)> ReqLogin(string platformCode)
    {

        //多账号测试

        var loginReq = new JsonData();
        loginReq["gameId"] = LaunchSettings.GetCurEnvironmentSettings().gameId;
        loginReq["platform"] = (int)GetPlatform();
        loginReq["code"] = platformCode;
        loginReq["anonymousCode"] = "";
        
        Debug.Log(PlatformHandler.GameSettingData.TestLoginCode);
        if (PlatformHandler.GameSettingData.TestLoginCode != string.Empty)
        {
            loginReq["platform"] = 999;
            loginReq["code"] = PlatformHandler.GameSettingData.TestLoginCode;
        }

        var paramMap = new Dictionary<string, string>();
        var launchQuery = PlatformHandler.Instance.Platform.GetLaunchQuery();
        Debug.Log($"登录 launchQuery: {JsonMapper.ToJson(launchQuery)}");
        if (launchQuery.TryGetValue("scene", out var queryScene))
        {
            paramMap["query_scene"] = queryScene;
            Debug.Log($"登录二维码场景参数query_scene : {queryScene}");
        }
        var loginUrl = GetLoginUrl();
        var (webRequestResult, error, jsonData) = await HttpUtils.JsonAsync(loginReq, loginUrl, paramMap: paramMap);
        return (webRequestResult, error, jsonData);
    }

    public static async UniTask<LoginAck> Login(string platformCode)
    {
        UniTask ShowDialog(string err)
        {
            var result = new UniTaskCompletionSource();
            // 登录失败，暂时弹窗
            NetLoading.OnNetLoadFailedSelection?.Invoke(WaitNetTipType.LoadingAndTryConfirmWithoutClose, $"err:{err}").ContinueWith(async retry =>
            {
                if (retry)
                {
                    // 用户选择重试，重新发送请求
                    await Login(platformCode);
                    result.TrySetResult();
                }
            });
            return result.Task;
        }

        var (webRequestResult, error, jsonData) = await ReqLogin(platformCode);
        if (webRequestResult != WebRequestResult.Success)
        {
            Debug.Log("### login webRequestResult " + webRequestResult);
            await ShowDialog(error);
        }

        if ((int)jsonData["err_code"] != 0)
        {
            Debug.Log("login error " + jsonData["err_msg"]);
            await ShowDialog(jsonData["err_msg"].ToString());
        }

        if ((int)jsonData["err_code"] != 0)
        {
            Debug.Log("login error " + jsonData["err_msg"]);
        }

        var rep = new LoginAck
                  {
                      Uid = jsonData["data"]["user"]["uid"].ToString(),
                      Avatar = jsonData["data"]["user"]["avatar"].ToString(),
                      NickName = jsonData["data"]["user"]["nickname"].ToString(),
                      Time = (long)jsonData["data"]["expireMs"],
                      Token = jsonData["data"]["token"].ToString()
                  };
        token = rep.Token;
        return rep;
    }

    public static async UniTask<bool> SyncServerTimeUtc(Action<long> setAc, bool requestTimeout = false, int timeOut = 10)
    {
        var (webRequestResult, error, serverTimeResp) = await HttpUtils.JsonAsync(new JsonData(), GetUrl(serverTimePath), false, null, requestTimeout, timeOut);
        // TODO: 客户端需要处理下收到的值 {"utc_ms":1721734143139,"utc":"2024-07-23T19:29:03.139672+08:00","time":"2024-07-23T19:29:03.139672+08:00"}
        if (webRequestResult == WebRequestResult.Success)
        {
            var UtcMs = (long)serverTimeResp["utc_ms"];
            var Utc = serverTimeResp["utc"];
            var Time = serverTimeResp["time"];
            setAc?.Invoke(UtcMs / 1000);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取cdn地图数据
    /// </summary>
    /// <param name="mapId">地图 id</param>
    /// <param name="mapMd5">地图 md5</param>
    /// <returns>返回地图数据</returns>
    public static async UniTask<(byte[], bool, string)> GetCdnMapData(string mapId, string mapMd5, WaitNetTipType waitNetTipType)
    {

        byte[] mapDataBytes = null;
        string url = $"{ResCfg.CdnMapUrl}maps_screw_puzzle/maps/{mapMd5}.map";


        if (waitNetTipType == WaitNetTipType.None)
        {
            var (mapBytes, requestResult, responseCode, error) = await HttpUtils.GetCdnAsync(url);
            if (requestResult == UnityWebRequest.Result.Success)
            {
                mapDataBytes = mapBytes.Skip(21).ToArray();
                var mapDataStrBase64 = Encoding.UTF8.GetString(mapDataBytes.ToArray());
                mapDataBytes = Convert.FromBase64String(mapDataStrBase64);
                return (mapDataBytes, false, string.Empty);
            }

            return (null, true, $"err:{error} repCode:{responseCode}");
        }

        bool loadingDisplayed = false;
        CancellationTokenSource cts = null;

        // 显示加载提示
        async UniTask ShowLoading()
        {
            cts = new CancellationTokenSource();
            // 等待500ms，如果没有返回则显示loading加载动画
            await UniTask.Delay(500, cancellationToken: cts.Token);
            if (!cts.Token.IsCancellationRequested)
            {
                loadingDisplayed = true;
                NetLoading.ShowLoading(0);
            }
        }

        // 实际发送请求的本地函数
        async UniTask<(byte[], bool, string)> SendRequest()
        {
            // 启动显示加载提示的协程
            _ = ShowLoading();

            try
            {
                var (mapBytes, requestResult, responseCode, error) = await HttpUtils.GetCdnAsync(url);
                if (requestResult == UnityWebRequest.Result.Success)
                {
                    mapDataBytes = mapBytes.Skip(21).ToArray();
                    var mapDataStrBase64 = Encoding.UTF8.GetString(mapDataBytes.ToArray());
                    mapDataBytes = Convert.FromBase64String(mapDataStrBase64);

                    // 请求成功，隐藏加载提示
                    HideLoading();

                    return (mapDataBytes, false, string.Empty);
                }
                else
                {
                    HideLoading();
                    if (responseCode == 404)
                        return (mapDataBytes, true, $"err:{error} repCode:{responseCode}");
                    if (requestResult == UnityWebRequest.Result.ConnectionError || requestResult == UnityWebRequest.Result.ProtocolError)
                    {
                        // 弹出重试对话框
                        bool retry = await ShowRetryDialog(waitNetTipType, $"加载地图失败{error} code: {responseCode}");
                        if (retry)
                        {
                            // 用户选择重试，递归调用请求
                            return await SendRequest();
                        }

                        // 用户选择不重试，返回空数据
                        return (null, false, $"err:{error} repCode:{responseCode}");
                    }

                    return (null, true, $"err:{error} repCode:{responseCode}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LevelPlayer] 加载地图数据失败, mapId: {mapId}, mapMd5: {mapMd5}, 异常: {e}");

                // 请求失败，隐藏加载提示
                HideLoading();

                return (null, true, $"throw err:{e.Message}");
            }
        }

        // 发送请求并返回结果
        return await SendRequest();

        void HideLoading()
        {
            if (loadingDisplayed)
            {
                loadingDisplayed = false;
                NetLoading.HideLoading();
            }
            else
            {
                cts?.Cancel();
            }
        }
    }



    public static async UniTask<(bool success, TRet retData)> PostLogicProtoAsync<TSend, TRet>(TSend param, WaitNetTipType waitNetTipType = WaitNetTipType.None, bool showErrMsg = true)
        where TSend: IMessage<TSend>, new()
        where TRet: IMessage<TRet>, new()
    {
        PrintProtoMessage(param);
        string url = GetUrl(logicProtocolPath);
        if (waitNetTipType == WaitNetTipType.None)
        {
            var (webRequestResult, retData, errMsg) = await HttpUtils.PostProtoAsync<TSend, TRet>(url, param, token);
            PrintProtoMessage(retData);
            if (webRequestResult == WebRequestResult.Success)
            {
                return (true, retData);
            }

            if (showErrMsg)
            {
                ToastScreenPresenter.ShowError(errMsg);
            }

            return (false, default);
        }

        bool loadingDisplayed = false;

        CancellationTokenSource cts = null;

        // 显示加载提示
        async UniTask ShowLoading()
        {
            cts = new CancellationTokenSource();
            // 等待500ms，如果没有返回则显示loading加载动画
            await UniTask.Delay(500, cancellationToken: cts.Token);
            if (!cts.Token.IsCancellationRequested)
            {
                loadingDisplayed = true;
                NetLoading.ShowLoading(0);
            }
        }

        // 发送请求的本地函数
        async UniTask<(bool success, TRet retData)> SendRequest()
        {
            // 启动显示加载提示的协程
            _ = ShowLoading();

            var requestTask = HttpUtils.PostProtoAsync<TSend, TRet>(url, param, token);
            var result = await requestTask;
            PrintProtoMessage(result.retData);

            // 如果请求成功，取消加载提示
            if (loadingDisplayed)
            {
                loadingDisplayed = false;
                NetLoading.HideLoading();
            }
            else
            {
                cts?.Cancel();
            }

            if (result.webRequestResult == WebRequestResult.Success)
            {
                return (true, result.retData);
            }

            string errStr = result.errMsg;
            if (Enum.TryParse<EHttpErrorCode>(result.errMsg, out EHttpErrorCode code))
            {
                errStr = ((int)code).ToString();
            }

            bool retry = await ShowRetryDialog(waitNetTipType, errStr);
            if (retry)
            {
                if (code == EHttpErrorCode.Unauthorized || code == EHttpErrorCode.UserSignErr || code == EHttpErrorCode.JwttokenOutOfDateErr)
                {
                    var platformLoginResult = await PlatformHandler.Instance.Platform.Login();
                    var loginAck = await Login(platformLoginResult.code);
                    CommonUIEvents.RaiseReLogin(loginAck);
                    return await SendRequest();
                }
                else
                {
                    return await SendRequest();
                }
            }

            return (false, default);
        }


        // 发送请求并返回结果
        return await SendRequest();
    }

    public static async UniTask<string> PostTestProtoAsync(IMessage param, Type recvType)
    {
        var url = GetUrl(logicProtocolPath);
        var (webRequestResult, retData, errMsg) = await HttpUtils.PostTestProtoAsync(url, param, token, recvType);
        return retData;
    }

    // 失败处理的本地函数
    private static async UniTask<bool> ShowRetryDialog(WaitNetTipType waitNetTipType, string errMsg)
    {
        UniTaskCompletionSource<bool> resultTask = new UniTaskCompletionSource<bool>();
        // 弹出重试窗口
        NetLoading.OnNetLoadFailedSelection?.Invoke(waitNetTipType, errMsg).ContinueWith(retry => { resultTask.TrySetResult(retry); });

        return await resultTask.Task;
    }

    /// <summary>
    /// 上报广告
    /// </summary>
    /// <returns></returns>
    public static async void AdReport(string report)
    {
        var adReq = new JsonData
                    {
                        ["name"] = report,
                        ["clickid"] = GameInstance.Instance.clickid,
                        ["sources"] = PlatformHandler.Instance.Platform.GetLaunchString()
                    };
        var (webRequestResult, error, jsonData) = await HttpUtils.JsonAsyncWithToken(adReq, GetReportUrl(), token);
        Debug.LogFormat("AdReport:{0},{1},{2}", report, GameInstance.Instance.clickid, PlatformHandler.Instance.Platform.GetLaunchString());
        if (webRequestResult != WebRequestResult.Success)
        {
            Debug.Log("### AdReport " + webRequestResult);
        }
    }

    /// <summary>
    /// 测试账号或开发环境打印protobuf消息
    /// </summary>
    private static void PrintProtoMessage(IMessage message)
    {
        if (GameInstance.Instance.IsTestAccount || LaunchSettings.environmentType != LaunchSettings.EnvironmentType.Production)
        {
            if (message == null)
            {
                Debug.LogWarning("打印protobuf消息失败，参数为空");
                return;
            }
            var protoName = message.GetType().Name;
            var settings = new JsonFormatter.Settings(false);
            settings = settings.WithIndentation();
            var jsonFormatter = new JsonFormatter(settings);
            var json = jsonFormatter.Format(message);
            if (protoName.Contains("Request"))
            {
                Debug.Log($"请求消息 {protoName} \r\n{json}");
            }
            else
            {
                Debug.Log($"返回消息 {protoName} \r\n{json}");
            }

        }

    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitJson;
using MyUI;
using Protocol;
using UIFramework;
using UnityEngine;

#if PLATFORM_WEIXIN
using GameScript.Runtime.GameLogic;
using GameScript.Runtime.GameLogic.Events;
using Newtonsoft.Json;
using WeChatWASM;

namespace GameScript.Runtime.Platform
{
    // 获取基础库版本的API GetAppBaseInfo 需要基础库版本:`2.25.3`，
    //<=2.20.1 的基础库请使用 wx.getSystemInfo]((wx.getSystemInfo)) 或者 wx.getSystemInfoSync 获取基础库版本
    //由于两个API之间有断层，所以最低基础库版本是2.25.3
    public class WeChatPlatformAPI : IPlatformAPI
    {
        private WeChatWASM.WXRewardedVideoAd _rewardedVideoAd;

        private OnGameGyroscopeChanged _onGameGyroscopeChanged;

        private readonly Action<PlatformFunc, string, bool> _useApiErrorCallback;

        private const string                  ADUnitIdTt = "d19j093f102301jeca"; // 拔罐
        private const string                  ADUnitIdWx = "adunit-b20c9f914632396c";
        private       Action<AdState, int>    onVideoAdCallback;
        private       Action<bool>            onShareCallback;
        private       Action<bool>            onStrictShareCallback;
        private       DateTime                shareTime = DateTime.Now;
        private       CancellationTokenSource _shareAutoSuccessTokenSource;
        private       int                     _shareAutoSuccessTimeout = 2000; // 展示自动成功时长

        private string sdkVersion = "2.23.1";

        public WeChatPlatformAPI(Action<PlatformFunc, string, bool> useApiErrorCallback)
        {
            _useApiErrorCallback = useApiErrorCallback;
            // 注: 不再需要在CS中检测版本更新, 微信SDK中可以自动做版本检测了
            // CheckNewVersion();
            Init();
        }

        private void Init()
        {
            WeChatWASM.WX.OnShow(OnShow);
            WeChatWASM.WX.OnHide(OnHide);
            WeChatWASM.WXBase.OnShareAppMessage(RandomWxShareAppMessageParam(), OnShareAppMessage);
            WX.OnShareMessageToFriend(OnShareMessageToFriend);
            // sdkVersion = WX.GetAppBaseInfo().SDKVersion;  //这个API 需要基础库版本:`2.25.3`
            // Debug.Log("sdk1 version: " + sdkVersion );
            //这个API 需要基础库版本:2.20.1
            WX.GetSystemInfo(new GetSystemInfoOption()
                             {
                                 success = (msg) =>
                                 {
                                     if (!string.IsNullOrEmpty(msg.SDKVersion))
                                         sdkVersion = msg.SDKVersion;
                                 }
                             });
        }

        public Dictionary<string, string> GetLaunchQuery()
        {
            var launchOptions = WX.GetLaunchOptionsSync();
            return launchOptions.query;
        }

        public string GetLaunchString()
        {
            var launchOptions = WX.GetLaunchOptionsSync();
            return JsonMapper.ToJson(launchOptions);
        }

        public bool LaunchFromScene(FromSceneType type)
        {
            WXSceneID sceneID = (WXSceneID)WX.GetLaunchOptionsSync().scene;
            Debug.Log($"launch from scene {sceneID}");
            switch (type)
            {
                case FromSceneType.All:
                    return true;
                case FromSceneType.AdScene:
                    return sceneID == WXSceneID.WX_FRIENDS_AD
                           || sceneID == WXSceneID.WX_FRIENDS_DETAIL_AD
                           || sceneID == WXSceneID.WX_LOOK_AD
                           || sceneID == WXSceneID.WX_ARTICLE_AD
                           || sceneID == WXSceneID.WX_NEARBY_AD
                           || sceneID == WXSceneID.WX_FRIENDS_NATIVE_AD;
                case FromSceneType.Public:
                    return sceneID == WXSceneID.WX_MENU;
            }

            return false;
        }

        private void CheckNewVersion()
        {
            var manager = WX.GetUpdateManager();
            manager.OnCheckForUpdate(result => { Debug.Log($"app update {result.hasUpdate}"); });
            manager.OnUpdateReady(result =>
            {
                Debug.Log($"app update ready");
                WX.ShowModal(new ShowModalOption
                             {
                                 title = "更新提示",
                                 content = "新版本已经准备好，是否重启应用？",
                                 success = confirmResult =>
                                 {
                                     if (confirmResult.confirm)
                                     {
                                         manager.ApplyUpdate();
                                     }
                                     else
                                     {
                                         Debug.Log($"app update restart cancel");
                                     }
                                 }
                             });
            });
        }

        private void OnShareAppMessage(Action<WXShareAppMessageParam> action = null)
        {
            action?.Invoke(RandomWxShareAppMessageParam());
        }

        private WXShareAppMessageParam RandomWxShareAppMessageParam()
        {
            var index = UnityEngine.Random.Range(0, titles.Length);
            return new WeChatWASM.WXShareAppMessageParam()
                   {
                       title = TargetShareTitle(index),
                       imageUrl = TargetShareImageUrl(index),
                       // query = "from=onShareAppMessage"
                   };
        }

        private static readonly string[] titles = {
                                                      "不废眼的寻物游戏，我们有救了！",
                                                      "在？考考眼神？",
                                                      "今天去看中医，医生说我...",
                                                      "只差一个物品了，到底在哪儿？！",
                                                  };
        
        private string TargetShareTitle(int index)
        {
            return titles[index];
        }

        private string TargetShareImageUrl(int index)
        {
            return $"https://incubator-static.easygame2021.com/game-journey/share-img/share{index + 1}.png";
        }

        private void OnShow(WeChatWASM.OnShowListenerResult result)
        {
            // 分享相关处理
            if (onShareCallback != null) OnShareCallbackByShowHide();
            CancelShareAutoSuccessTask();
            // 处理卡片数据
            try
            {
                var jsonResult = JsonConvert.SerializeObject(result, Formatting.Indented); // 序列化为 JSON 字符串
                Debug.Log($"【启动参数】{jsonResult}");
            }
            catch (Exception e)
            {
                Debug.Log($"【启动参数】json格式化失败");
            }

            GameInstance.Instance.CheckLaunchQuery(false, result.query);
            GameInstance.Instance.CheckFromScene(CheckFromScene((WXSceneID)result.scene), true);
        }

        /// <summary>
        /// 判断来源场景的类型
        /// </summary>
        /// <param name="sceneID"></param>
        /// <returns></returns>
        private FromSceneType CheckFromScene(WXSceneID sceneID)
        {
            Debug.Log($"launch from scene {sceneID}");
            switch (sceneID)
            {
                case WXSceneID.WX_FRIENDS_AD:
                case WXSceneID.WX_FRIENDS_DETAIL_AD:
                case WXSceneID.WX_LOOK_AD:
                case WXSceneID.WX_ARTICLE_AD:
                case WXSceneID.WX_NEARBY_AD:
                case WXSceneID.WX_FRIENDS_NATIVE_AD:
                    return FromSceneType.AdScene;
                case WXSceneID.WX_MENU:
                    return FromSceneType.Public;
            }

            return FromSceneType.All;
        }

        private void OnHide(WeChatWASM.GeneralCallbackResult result)
        {
            CancelShareAutoSuccessTask();
        }

        private void OnShareMessageToFriend(OnShareMessageToFriendListenerResult result)
        {
            OnShareCallback(result.success);
            OnStrictShareCallback(result.success, false);
            CancelShareAutoSuccessTask();
        }

        public bool IsSupport(PlatformFunc func)
        {
            switch (func)
            {
                case PlatformFunc.Vibrate:
                    return true;
                case PlatformFunc.Gyroscope:
                    return true;
                case PlatformFunc.NotchFitter:
                    return true;
                case PlatformFunc.SliderBarNavigateToScene:
                    return false;
                case PlatformFunc.GetGameRecorder:
                    return false;
                case PlatformFunc.StartRecord:
                    return false;
                case PlatformFunc.StopRecord:
                    return false;
                case PlatformFunc.GetGameRecorderState:
                    return false;
                case PlatformFunc.Sharable:
                    return true;
                default:
                    return false;
            }
        }

        public UniTask<LoginResult> Login()
        {
            WeChatWASM.WXBase.PreloadConcurrent(10);

            var result = new UniTaskCompletionSource<LoginResult>();
            WeChatWASM.LoginOption callback = new WeChatWASM.LoginOption();
            callback.success = res =>
            {
                Debug.Log("Login success code " + res.code);
                LoginResult loginResult = new LoginResult();
                loginResult.code = res.code;
                result.TrySetResult(loginResult);
            };
            callback.fail = res =>
            {
                Debug.Log("Login fail errMsg " + res.errMsg);
                LoginResult loginResult = new LoginResult();
                loginResult.errCode = (int)res.errno;
                loginResult.errMsg = res.errMsg;
                result.TrySetResult(loginResult);
            };
            WeChatWASM.WX.Login(callback);
            return result.Task;
        }

        public void Share(string title, string imageUrl, Action<bool> callback = null)
        {
            try
            {
                shareTime = DateTime.Now;
                onShareCallback = callback;
                WeChatWASM.ShareAppMessageOption option = new WeChatWASM.ShareAppMessageOption();
                var index = UnityEngine.Random.Range(0, titles.Length);
                option.title = string.IsNullOrEmpty(title) ? TargetShareTitle(index) : title;
                option.imageUrl = string.IsNullOrEmpty(imageUrl) ? TargetShareImageUrl(index) : imageUrl;
                StartShareAutoSuccessTask();
                WeChatWASM.WX.ShareAppMessage(option);
            }
            catch (Exception e)
            {
                onShareCallback?.Invoke(false);
                onShareCallback = null;
                Debug.Log("Share Error: " + e.Message);
            }
        }

        /// <summary>
        /// 分享-带参数
        /// </summary>
        /// <param name="shareMessage"></param>
        /// <param name="callback"></param>
        public void ShareMessage(ShareMessageType shareMessage, Action<bool> callback = null)
        {
            try
            {
                shareTime = DateTime.Now;

                if (shareMessage.mode == ShareMessageMode.Strict && GameInstance.Instance.GetEFunctionTypeSwitch(EFunctionType.SubdomainShared))
                {
                    string query = shareMessage.query;
                    if (!string.IsNullOrEmpty(query))
                    {
                        onStrictShareCallback = callback;
                        // 设置子域数据
                        IWxRankingAPI.SetShareGiftName(shareMessage.itemName);
                        SetMessageToFriendQuery(WXShareScene.ShareGift, query[(query.IndexOf('=') + 1)..]);
                        // 严格模式，打开弹窗，使用子域分享
                        return;
                    }
                }

                onShareCallback = callback;
                WeChatWASM.ShareAppMessageOption option = new WeChatWASM.ShareAppMessageOption();
                var index = UnityEngine.Random.Range(0, titles.Length);
                option.title = string.IsNullOrEmpty(shareMessage.title) ? TargetShareTitle(index) : shareMessage.title;
                option.imageUrl = string.IsNullOrEmpty(shareMessage.imageUrl) ? TargetShareImageUrl(index) : shareMessage.imageUrl;
                option.query = string.IsNullOrEmpty(shareMessage.query) ? "" : shareMessage.query;
                StartShareAutoSuccessTask();
                WeChatWASM.WX.ShareAppMessage(option);
            }
            catch (Exception e)
            {
                callback?.Invoke(false);
                onShareCallback = null;
                onStrictShareCallback = null;
                Debug.Log("Share Error: " + e.Message);
            }
        }

        /// <summary>
        /// 来自Show/Hide的分享回调，需要判断时间
        /// </summary>
        private void OnShareCallbackByShowHide()
        {
            // 分享相关处理
            OnShareCallback((DateTime.Now - shareTime).TotalSeconds >= 1);
        }

        /// <summary>
        /// 严格模式分享页面触发的回调
        /// </summary>
        /// <param name="success"></param>
        private void OnStrictShareCallback() => OnStrictShareCallback(false, true);

        /// <summary>
        /// 严格模式的分享回调
        /// </summary>
        /// <param name="success"></param>
        /// <param name="fromUi">true：分享弹窗关闭触发的回调，false：OnShareMessageToFriend接口触发</param>
        private void OnStrictShareCallback(bool success, bool fromUi)
        {
            Debug.Log($"---- strict share callback {success} {fromUi} {onStrictShareCallback} ----");
            if (onStrictShareCallback == null) return;
            // 严格模式下，OnShareMessageToFriend的分享失败不处理，可能会取消选择其他好友
            if (!success && !fromUi) return;
            onStrictShareCallback.Invoke(success);
            onStrictShareCallback = null;
            // 重置子域数据
            IWxRankingAPI.SetShareGiftName("");
            SetMessageToFriendQuery(WXShareScene.Default, "");
        }

        /// <summary>
        /// 分享回调
        /// </summary>
        /// <param name="success"></param>
        private void OnShareCallback(bool success)
        {
            if (onShareCallback == null) return;
            onShareCallback.Invoke(success);
            onShareCallback = null;
        }

        private async void StartShareAutoSuccessTask()
        {
            _shareAutoSuccessTokenSource = new CancellationTokenSource();
            try
            {
                await UniTask.Delay(_shareAutoSuccessTimeout, cancellationToken: _shareAutoSuccessTokenSource.Token);
                _shareAutoSuccessTokenSource = null;
                OnShareCallback(true);
            }
            catch (Exception e)
            {
                // ignored
                // 提前Cancel，TokenSource会抛出一个异常保证不走后面的代码，这里不需要处理
            }
        }

        private void CancelShareAutoSuccessTask()
        {
            if (_shareAutoSuccessTokenSource == null) return;
            _shareAutoSuccessTokenSource.Cancel();
            _shareAutoSuccessTokenSource = null;
        }

        public void SetClipboardData(string text)
        {
            WeChatWASM.WX.SetClipboardData(new WeChatWASM.SetClipboardDataOption() { data = text });
        }

        public void StartGyroscope(double interval, Action<bool, string> statusCallback, OnGameGyroscopeChanged gyroscopeChanged)
        {
            WeChatWASM.WXBase.StartGyroscope(new WeChatWASM.StartGyroscopeOption() { interval = "game" });
            WeChatWASM.WXBase.OnGyroscopeChange(result => { gyroscopeChanged?.Invoke(result.x, result.y, result.z, 0, 0, 0, 0, 0); });

            _onGameGyroscopeChanged += gyroscopeChanged;
        }

        public void StopGyroscope(Action<bool, string> statusCallback)
        {
            WeChatWASM.WXBase.StopGyroscope(null);
            WeChatWASM.WXBase.OffGyroscopeChange();
        }

        public void Vibrate(long[] pattern, int repeat)
        {
            WeChatWASM.WX.VibrateShort(new WeChatWASM.VibrateShortOption()
                                       {
                                           type = "heavy",
                                       });
        }

        #region notch

        private float _notchBottomY = float.MaxValue;

        private float NotchBottomY
        {
            get
            {
                if (Math.Abs(_notchBottomY - float.MaxValue) < 0.001f)
                {
                    _notchBottomY = (float)WeChatWASM.WX.GetMenuButtonBoundingClientRect().bottom;
                }

                return _notchBottomY;
            }
        }

        public void NotchFitter(RectTransform fitterRectTransform)
        {
            if (fitterRectTransform == null)
                return;

            var anchoredPosition = fitterRectTransform.anchoredPosition;
            anchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y - NotchBottomY);
            fitterRectTransform.anchoredPosition = anchoredPosition;
        }

        public void SliderBarNavigation(GameSliderSceneEnum sceneEnum, Action successCallback, Action completeCallback, Action<int, string> errorCallback)
        {
        }

        public void StartRecord(bool isRecordAudio = true, int maxRecordTimeSec = 600, Action startCallback = null, Action<int, string> errorCallback = null, Action<string> timeoutCallback = null)
        {
        }

        public void StopRecord(Action<string> completeCallback, Action<int, string> errorCallback)
        {
        }

        public void ShareGameRecord(Action<Dictionary<string, object>> successCallback, Action<string> errorCallback, Action cancelCallback)
        {
        }

        public GameVideoRecordState GetVideoRecordState()
        {
            return GameVideoRecordState.RECORD_ERROR;
        }

        #endregion


        #region rank

        public void SetRankScore(int score)
        {
        }

        public void GetRankList(Action<bool, string> action, String type = null, String suffix = null)
        {
            if (type != "day" && type != "week" && type != "month")
            {
                type = "all";
            }
        }

        public string GetDevicePlatform()
        {
            var info = WeChatWASM.WX.GetSystemInfoSync();
            return info.platform;
        }

        #endregion

        #region video ad

        public void InitVideoAd()
        {
            Debug.Log("### InitVideoAd");

            _rewardedVideoAd = WeChatWASM.WXBase.CreateRewardedVideoAd(new WeChatWASM.WXCreateRewardedVideoAdParam()
                                                                       {
                                                                           adUnitId = ADUnitIdWx,
                                                                           multiton = true,
                                                                       });
            _rewardedVideoAd.OnLoad(res => { Debug.Log("video ad OnLoad success:" + JsonUtility.ToJson(res)); });
            _rewardedVideoAd.OnError(OnLoadError);
            _rewardedVideoAd.OnClose(res => { CloseCallback(res.isEnded); });
            _rewardedVideoAd.Load();
            _isReCreateingAd = false;
        }

        void OnLoadError(WXADErrorResponse err)
        {
            Debug.Log($"video ad OnLoad fail: {err.errCode} msg:{err.errMsg}");
        }

        private bool _isReCreateingAd = false;

        async UniTask ReCreateAd()
        {
            _isReCreateingAd = true;
            await UniTask.DelayFrame(2);
            if (_rewardedVideoAd != null)
                _rewardedVideoAd.Destroy();
            await UniTask.DelayFrame(1);
            InitVideoAd();
        }

        private string curShowError     = string.Empty;
        private int    curShowErrorCode = 0;
        private bool   isShowVideo      = false;

        private UniTaskCompletionSource<bool> playVideoAdTask;

        public async UniTask<(AdState, string)> PlayVideoAd(CancellationToken cancellationToken)
        {
            //如果不是正式版本，则不用看广告
            if (LaunchSettings.environmentType != LaunchSettings.EnvironmentType.Production)
            {
                return (AdState.Success, string.Empty);
            }

            if (isShowVideo)
            {
                return (AdState.AdTryShowing, string.Empty);
            }

            isShowVideo = true;
            curShowErrorCode = 0;

            AdState adState = AdState.Error;
            try
            {
                playVideoAdTask = new UniTaskCompletionSource<bool>();
                adState = await TryShowAd(cancellationToken);

                isShowVideo = false;
                if (adState == AdState.AdShowSuccess)
                {
                    bool playComplete = await playVideoAdTask.Task;
                    adState = playComplete ? AdState.Success : AdState.Exit;
                }
            }
            catch (Exception e)
            {
                isShowVideo = false;
            }

            if (!string.IsNullOrEmpty(curShowError))
            {
                Debug.Log($">> play ad error {curShowError}");
            }

            return (adState, curShowError);
        }


        async UniTask<AdState> TryShowAd(CancellationToken cancellationToken)
        {
            AdState adState = AdState.Error;
            for (int i = 0; i < 3; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return AdState.AdShowCancel;
                float time = Time.realtimeSinceStartup;
                adState = await RealShowAd(cancellationToken);
                if (adState == AdState.AdShowSuccess)
                    return adState;
                if (!string.IsNullOrEmpty(curShowError))
                    Log($"showAd:{curShowError}", LogType.Debug);
                float offTime = 1 - (Time.realtimeSinceStartup - time);
                if (offTime > 0)
                    await UniTask.Delay((int)(offTime * 1000f), cancellationToken: cancellationToken);
            }


            if (adState == AdState.AdComponentBroken)
            {
                //重建广告组件 
                await ReCreateAd();
                //加载广告没有那么快，这里先等待0.1s
                await UniTask.Delay(100, cancellationToken: cancellationToken);

                adState = await RealShowAd(cancellationToken);

                if (adState == AdState.AdShowSuccess)
                {
                    return adState;
                }
            }

            //判断是否连着网络
            bool isConnectServer = await RequestServer();
            Debug.Log($"isConnectServer {isConnectServer}");
            //如果没连着网，则返回网络原因播放失败
            if (!isConnectServer)
            {
                curShowError = $"网络异常，广告加载失败 {curShowErrorCode}";
                return AdState.AdNoNetShowFail;
            }

            await UniTask.Delay(100, cancellationToken: cancellationToken);
            //会存在先连上网，又返回播放失败的问题，再次播放一次
            adState = await RealShowAd(cancellationToken);
            //用重建创建的广告组件播放广告，依然返回了广告组件错误的情况，则设置为转分享 
            if (adState == AdState.AdComponentBroken)
                adState = AdState.AdToShare;

            return adState;
        }

        UniTask<bool> RequestServer()
        {
            //验证消息，设置3秒超时
            return NetManager.SyncServerTimeUtc(null, true, 3);
        }

        UniTask<AdState> RealShowAd(CancellationToken cancellationToken)
        {
            UniTaskCompletionSource<AdState> result = new UniTaskCompletionSource<AdState>();
            try
            {
                isShowVideo = true;

                _rewardedVideoAd.Show((msg) =>
                {
                    result.TrySetResult(AdState.AdShowSuccess);
                    isShowVideo = false;
                    curShowError = string.Empty;
                    curShowErrorCode = 0;
                }, failed: err =>
                {
                    //判断是否需要转分享前，需要先检测网络是否是连通的
                    //从羊里得到的需要转分享的错误码
                    if (err.errCode is 1000 or 1002 or 1004 or 1005 or 1006 or 5020 or 5032)
                    {
                        result.TrySetResult(AdState.AdToShare);
                    }
                    else if (err.errMsg.Contains("(2,-1)") || err.errMsg.Contains("timeout") || err.errCode is 0) //广告组件坏掉了
                    {
                        result.TrySetResult(AdState.AdComponentBroken);
                    }
                    else
                    {
                        result.TrySetResult(AdState.Error);
                    }

                    isShowVideo = false;
                    Log($"show ad err:{err.errMsg} {err.errCode}", LogType.Error);
                    curShowErrorCode = err.errCode;
                    curShowError = $"广告播放失败，请重试 {err.errCode}";
                });
            }
            catch (Exception e)
            {
                Log($"show ad err:{e.Message}", LogType.Error);
                curShowErrorCode = 0;
                result.TrySetResult(AdState.AdThrowError);
                isShowVideo = false;
            }

            return result.Task;
        }


        private void CloseCallback(bool isComplete)
        {
            Log(isComplete ? "video ad is complete" : "video ad is close", LogType.Info);
            // 用户点击 关闭广告 后会去拉取下一条广告。不需要手动拉取
            //_rewardedVideoAd.Load();
            playVideoAdTask.TrySetResult(isComplete);
            onVideoAdCallback?.Invoke(isComplete ? AdState.Success : AdState.Exit, 0);
            onVideoAdCallback = null;
        }

        private void ErrCallback(int errCode, string errMsg)
        {
            Debug.Log($"video ad err {errCode} {errMsg}");

            var state = AdState.Error;
            if (errCode is 1000 or 1002 or 1004 or 1005 or 1006 or 5020 or 5032)
            {
                state = AdState.AdToShare;
            }

            onVideoAdCallback?.Invoke(state, errCode);
            onVideoAdCallback = null;
            // _rewardedVideoAd.Load();
        }

        #endregion

        public void IsAuthor(Action<bool> getResultCallback, Action<string> failCallback)
        {
            bool isSupport = IsSupport("1.2.0");
            if (!isSupport)
            {
                failCallback?.Invoke("GetSetting can't support");
                return;
            }

            try
            {
                var settingOption = new GetSettingOption()
                                    {
                                        success = (result) =>
                                        {
                                            bool isAuthor = result.authSetting.TryGetValue("scope.userInfo", out bool isAuth) && isAuth;
                                            getResultCallback?.Invoke(isAuthor);
                                        },
                                        fail = (result) => { failCallback?.Invoke(result.errMsg); }
                                    };
                WX.GetSetting(settingOption);
            }
            catch (Exception e)
            {
                failCallback?.Invoke($"throw error {e.Message}");
            }
        }


        private WXUserInfoButton _userInfoButton;

        /// <summary>
        /// 创建授权透明按钮
        /// </summary>
        /// <param name="x">按钮位置</param>
        /// <param name="y">按钮位置</param>
        /// <param name="width">按钮宽度</param>
        /// <param name="height">按钮高度</param>
        /// <param name="language">语言</param>
        /// <param name="withCredentials">是否带有登陆信息</param>
        /// <param name="tabCallback">点击回调</param>
        private const string RefuseError = "privacy permission is not authorized";

        public void CreateUserInfoBtn(int x, int y, int width, int height, string language, bool withCredentials, Action<PlatformUserInfo, GetPlatformUserInfoState, string> tabCallback)
        {
            bool isSupport = IsSupport("2.0.1");
            if (!isSupport)
            {
                tabCallback?.Invoke(default, GetPlatformUserInfoState.Error, "CreateUserInfoBtn can't support");
                return;
            }

            try
            {
                if (_userInfoButton != null)
                {
                    return;
                }

                _userInfoButton = WeChatWASM.WXBase.CreateUserInfoButton(x, y, width, height, "zh_CN", true);


                _userInfoButton.Show();
                _userInfoButton.OnTap((res) =>
                {
                    if (res.errMsg.IndexOf(":ok") > -1 && !string.IsNullOrEmpty(res.userInfoRaw))
                    {
                        var userinfo = res.userInfo;
                        PlatformUserInfo platformUserInfo = new PlatformUserInfo
                                                            {
                                                                NickName = userinfo.nickName,
                                                                Avatar = userinfo.avatarUrl
                                                            };
                        tabCallback?.Invoke(platformUserInfo, GetPlatformUserInfoState.Agree, string.Empty);
                    }
                    else
                    {
                        if (res.errMsg.Contains(RefuseError))
                        {
                            tabCallback?.Invoke(default, GetPlatformUserInfoState.Refuse, res.errMsg);
                        }
                        else
                        {
                            tabCallback?.Invoke(default, GetPlatformUserInfoState.Error, res.errMsg);
                        }
                    }

                    _userInfoButton?.Destroy();
                });
            }
            catch (Exception e)
            {
                tabCallback?.Invoke(default, GetPlatformUserInfoState.Error, $"throw error {e.Message}");
            }
        }

        /// <summary>
        /// 获取平台用户信息
        /// </summary>
        /// <returns></returns>
        public void GetPlatformUserInfo(Action<PlatformUserInfo, GetPlatformUserInfoState, string> userInfoCallback)
        {
            try
            {
                var getUserInfoOption = new GetUserInfoOption()
                                        {
                                            success = (userinfo) =>
                                            {
                                                PlatformUserInfo platformUserInfo = new PlatformUserInfo
                                                                                    {
                                                                                        NickName = userinfo.userInfo.nickName,
                                                                                        Avatar = userinfo.userInfo.avatarUrl
                                                                                    };
                                                userInfoCallback?.Invoke(platformUserInfo, GetPlatformUserInfoState.Agree, string.Empty);
                                            },
                                            fail = (result) => { userInfoCallback?.Invoke(default, GetPlatformUserInfoState.Agree, result.errMsg); },
                                            complete = (result) => { }
                                        };
                WeChatWASM.WX.GetUserInfo(getUserInfoOption);
            }
            catch (Exception e)
            {
                userInfoCallback?.Invoke(default, GetPlatformUserInfoState.Refuse, $"throw error {e.Message}");
            }
        }

        public GameWindowInfo GetWindowInfo()
        {
            bool isSupport = IsSupport("2.25.3");
            try
            {
                if (isSupport)
                {
                    var windowInfo = WX.GetWindowInfo();

                    return new GameWindowInfo()
                           {
                               pixelRatio = windowInfo.pixelRatio,
                               screenWidth = windowInfo.screenWidth,
                               screenHeight = windowInfo.screenHeight
                           };
                }
            }
            catch (Exception e)
            {
            }

            return new GameWindowInfo()
                   {
                       pixelRatio = 1,
                       screenWidth = Screen.width,
                       screenHeight = Screen.height,
                   };
        }

        /// <summary>
        /// 设置帧率 帧率，有效范围 1 - 60。
        /// </summary>
        public void SetTargetFrame(int frame)
        {
            try
            {
                WX.SetPreferredFramesPerSecond(frame);
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        /// <summary>
        /// 是否需要打开隐私授权界面，微信需要
        /// </summary>
        /// <returns></returns>
        public void GetPrivacyAuthorize(Action<bool> callback)
        {
            bool isSupport = IsSupport("2.32.3");
            if (!isSupport)
            {
                callback?.Invoke(true);
                return;
            }

            //try catch 捕捉不到报错，还是会卡住游戏
            try
            {
                GetPrivacySettingOption option = new GetPrivacySettingOption()
                                                 {
                                                     success = (msg) =>
                                                     {
//msg.needAuthorization = true 时 需要打开授权，意思是没授权
                                                         callback?.Invoke(msg.needAuthorization);
                                                     },
                                                     fail = (msg) => { callback?.Invoke(true); }
                                                 };
                WX.GetPrivacySetting(option);
            }
            catch (Exception e)
            {
                Log($"GetPrivacySetting>>> fail:{e.Message}", LogType.Error);
                callback?.Invoke(true);
            }
        }

        /// <summary>
        /// 打开隐私授权界面，微信需要
        /// </summary>
        /// <param name="callback">是否同意授权</param>
        public void OpenPrivacyAuthorize(Action<bool> callback)
        {
            bool isSupport = IsSupport("2.32.3");
            if (!isSupport)
            {
                callback?.Invoke(false);
                return;
            }

            try
            {
                RequirePrivacyAuthorizeOption option = new RequirePrivacyAuthorizeOption()
                                                       {
                                                           success = (msg) =>
                                                           {
                                                               callback?.Invoke(true); //同意//requirePrivacyAuthorize:ok
                                                           },
                                                           fail = (msg) =>
                                                           {
                                                               callback?.Invoke(false); //拒绝//privacy permission is not authorized
                                                           }
                                                       };
                WX.RequirePrivacyAuthorize(option);
            }
            catch (Exception e)
            {
                callback?.Invoke(false);
            }
        }

        bool IsSupport(string funcVersion)
        {
            return CompareVersion(sdkVersion, funcVersion) >= 0;
        }

        static int CompareVersion(string v1, string v2)
        {
            if (string.IsNullOrEmpty(v1) || string.IsNullOrEmpty(v2))
                return -1;
            string[] version1 = v1.Split('.');
            string[] version2 = v2.Split('.');
            int len = Math.Max(version1.Length, version2.Length);

            Array.Resize(ref version1, len);
            Array.Resize(ref version2, len);

            for (int i = 0; i < len; i++)
            {
                int num1 = 0;
                if (i < version1.Length)
                    int.TryParse(version1[i], out num1);
                int num2 = 0;
                if (i < version2.Length)
                    int.TryParse(version2[i], out num2);
                // int num1 = i < version1.Length ? int.Parse(version1[i]) : 0;
                // int num2 = i < version2.Length ? int.Parse(version2[i]) : 0;

                if (num1 > num2)
                {
                    return 1;
                }
                else if (num1 < num2)
                {
                    return -1;
                }
            }

            return 0;
        }

        public bool IsShortcutExist()
        {
            return false;
        }

        public void AddShortcut()
        {
            Debug.Log("添加桌面快捷方式");
        }

        /// <summary>
        /// 游戏版本号
        /// </summary>
        /// <returns></returns>
        public string GetGameVersion()
        {
            return Application.version;
        }

        /// <summary>
        /// 检查session 是否有效，用于判断登陆
        /// 仅利用微信wx.login做快速登录,且本地已经做了维护登陆状态， 是不需要用到checkSession的。
        ///如果需要算数据签名等流程 需要用到session_key时，此时用checkSession检查其是否过期会对流程有一定的性能优化。
        /// 由于抖音的iOS系统下切换账号时，没有清除缓存，所以需要检查session
        /// </summary>
        /// <returns></returns>
        public UniTask<bool> CheckSession()
        {
            return new UniTask<bool>(true);
        }

        /// <summary>
        /// 获取启动场景值
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public double GetLaunchSceneCode()
        {
            return WX.GetLaunchOptionsSync().scene;
        }

        /// <summary>
        /// 判断是否是公众号自定义菜单路径跳转启动
        /// </summary>
        /// <returns></returns>
        public bool IsLaunchSceneFromWxPublicAccount()
        {
            return LaunchFromScene(FromSceneType.Public); // 公众号自定义菜单途径
        }

        #region 调试日志

        /// <summary>
        /// 平台输出debug日志：每次调用的参数的总大小不超过100Kb
        /// 使用平台的输出日志方法，可以将日志输出到玩家的反馈日志里，但是不会输出到测试版的控制台
        /// 使用Debug.Log将日志输出到控制台
        /// </summary>
        public void Log(string logContent, LogType logType)
        {
            switch (logType)
            {
                case LogType.Debug:
                    WXBase.LogManagerDebug(logContent);
                    Debug.Log(logContent);
                    break;
                case LogType.Log:
                    WXBase.LogManagerLog(logContent);
                    Debug.Log(logContent);
                    break;
                case LogType.Info:
                    WXBase.LogManagerInfo(logContent);
                    Debug.Log(logContent);
                    break;
                case LogType.Warn:
                    WXBase.LogManagerWarn(logContent);
                    Debug.LogWarning(logContent);
                    break;
                case LogType.Error:
                    WXBase.LogManagerWarn(logContent);
                    Debug.LogError(logContent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

        #endregion

        /// <summary>
        /// 创建朋友圈按钮 目前是给游戏圈弹窗使用 参数固定
        /// </summary>
        public static WXGameClubButton CreateGameClubButton(WXCreateGameClubButtonParam param)
        {
            return WeChatWASM.WXBase.CreateGameClubButton(param);
        }

        /// <summary>
        /// 设置 wx.shareMessageToFriend 接口 query 字段的值
        /// 从子域分享的话，分享卡片不能带query参数，故拉起开放数据域前，开发者需要先通过wx.setMessageToFriendQuery接口设置
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="query"></param>
        private void SetMessageToFriendQuery(WXShareScene scene, string query)
        {
            var option = new SetMessageToFriendQueryOption
                         {
                             shareMessageToFriendScene = (long)scene,
                             query = query
                         };
            WX.SetMessageToFriendQuery(option);
        }
    }

    enum WXShareScene
    {
        Default   = 0,
        ShareGift = 1,
    }

    enum WXSceneID
    {
        /** 1000-其他 */
        WX_OTHER = 1000,

        /** 1001-发现页小程序「最近使用」列表（基础库2.2.4-2.29.0版本包含「我的小程序」列表，2.29.1版本起仅为「最近使用」列表） */
        WX_RECENT = 1001,

        /** 1005-微信首页顶部搜索框的搜索结果页 */
        WX_SEARCH = 1005,

        /** 1006-发现栏小程序主入口搜索框的搜索结果页 */
        WX_SEARCH_MAIN = 1006,

        /** 1007-单人聊天会话中的小程序消息卡片 */
        WX_CHAT = 1007,

        /** 1008-群聊会话中的小程序消息卡片 */
        WX_GROUP = 1008,

        /** 1010-收藏夹 */
        WX_FAVORITE = 1010,

        /** 1011-扫描二维码 */
        WX_QRCODE = 1011,

        /** 1012-长按图片识别二维码 */
        WX_LONG_QRCODE = 1012,

        /** 1013-扫描手机相册中选取的二维码 */
        WX_ALBUM = 1013,

        /** 1014-小程序订阅消息（与1107相同） */
        WX_SUBSCRIBE = 1014,

        /** 1017-前往小程序体验版的入口页 */
        WX_EXPERIENCE = 1017,

        /** 1018-openWeApp JSAPI，网页打开进入小程序 */
        WX_OPEN = 1018,

        /** 1019-微信钱包（微信客户端7.0.0版本改为支付入口） */
        WX_PAY = 1019,

        /** 1020-公众号 profile 页相关小程序列表（已废弃） */
        WX_PROFILE = 1020,

        /** 1022-聊天顶部置顶小程序入口（微信客户端6.6.1版本起废弃） */
        WX_TOP = 1022,

        /** 1023-安卓系统桌面图标 */
        WX_DESKTOP = 1023,

        /** 1024-小程序 profile 页 */
        WX_PROFILE_PAGE = 1024,

        /** 1025-扫描一维码 */
        WX_SCAN = 1025,

        /** 1026-发现栏小程序主入口，「附近的小程序」列表 */
        WX_NEARBY = 1026,

        /** 1027-微信首页顶部搜索框搜索结果页「使用过的小程序」列表 */
        WX_SEARCH_NEARBY = 1027,

        /** 1028-卡包里的券和礼品卡，打开小程序 */
        WX_CARD = 1028,

        /** 1029-小程序中的卡券详情页 */
        WX_CARD_DETAIL = 1029,

        /** 1030-自动化测试下打开小程序 */
        WX_AUTOMATION = 1030,

        /** 1031-长按图片识别一维码 */
        WX_LONG_SCAN = 1031,

        /** 1032-扫描手机相册中选取的一维码 */
        WX_ALBUM_SCAN = 1032,

        /** 1034-微信支付完成页 */
        WX_PAY_COMPLETE = 1034,

        /** 1035-公众号自定义菜单 */
        WX_MENU = 1035,

        /** 1036-App 分享消息卡片 */
        WX_SHARE = 1036,

        /** 1037-小程序打开小程序 */
        WX_OPEN_MINI = 1037,

        /** 1038-从另一个小程序返回 */
        WX_RETURN = 1038,

        /** 1039-摇电视 */
        WX_TV = 1039,

        /** 1042-添加好友搜索框的搜索结果页 */
        WX_SEARCH_FRIEND = 1042,

        /** 1043-公众号模板消息 */
        WX_TEMPLATE = 1043,

        /** 1044-带 shareTicket 的小程序消息卡片 详情 */
        WX_SHARE_TICKET = 1044,

        /** 1045-朋友圈广告 */
        WX_FRIENDS_AD = 1045,

        /** 1046-朋友圈广告详情页 */
        WX_FRIENDS_DETAIL_AD = 1046,

        /** 1047-扫描小程序码 */
        WX_QRCODE_SCAN = 1047,

        /** 1048-长按图片识别小程序码 */
        WX_LONG_QRCODE_SCAN = 1048,

        /** 1049-扫描手机相册中选取的小程序码 */
        WX_ALBUM_SCAN_QRCODE = 1049,

        /** 1052-卡券的适用门店列表 */
        WX_STORE = 1052,

        /** 1053-发现页进入搜一搜的搜索结果页 */
        WX_SEARCH_FROM_DISCOVER = 1053,

        /** 1054-顶部搜索框小程序快捷入口（微信客户端版本6.7.4起废弃） */
        WX_SEARCH_SHORTCUT = 1054,

        /** 1055-JSAPI网页打开小程序 */
        WX_JSAPI = 1055,

        /** 1056-聊天顶部音乐播放器右上角菜单 */
        WX_MUSIC = 1056,

        /** 1057-钱包中的银行卡详情页 */
        WX_BANK = 1057,

        /** 1058-公众号文章调用openWeApp */
        WX_OPEN_WEAPP = 1058,

        /** 1059-体验版小程序绑定邀请页 */
        WX_EXPERIMENT = 1059,

        /** 1064-微信首页连Wi-Fi状态栏 */
        WX_WIFI = 1064,

        /** 1065-URL scheme 详情 */
        WX_SCHEME = 1065,

        /** 1067-公众号文章广告 */
        WX_ARTICLE_AD = 1067,

        /** 1068-附近小程序列表广告（已废弃） */
        WX_NEARBY_AD = 1068,

        /** 1069-移动应用通过 openSDK 进入微信，打开小程序 */
        WX_OPENSDK = 1069,

        /** 1071-钱包中的银行卡列表页 */
        WX_BANKLIST = 1071,

        /** 1072-二维码收款页面 */
        WX_QR_PAY = 1072,

        /** 1073-客服消息列表下发的小程序消息卡片 */
        WX_CUSTOMER = 1073,

        /** 1074-公众号会话下发的小程序消息卡片 */
        WX_CUSTOMER_SESSION = 1074,

        /** 1077-摇周边 */
        WX_SHOP = 1077,

        /** 1078-微信连Wi-Fi成功提示页 */
        WX_WIFI_SUCCESS = 1078,

        /** 1081-客服消息下发的文字链 */
        WX_CUSTOMER_TEXT = 1081,

        /** 1082-公众号会话下发的文字链 */
        WX_CUSTOMER_TEXT_SESSION = 1082,

        /** 1084-朋友圈广告原生页 */
        WX_FRIENDS_NATIVE_AD = 1084,

        /** 1088-会话中查看系统消息，打开小程序 */
        WX_SESSION = 1088,

        /** 1089-微信聊天主界面下拉，「最近使用」栏（基础库2.2.4-2.29.0版本包含「我的小程序」栏，2.29.1版本起仅为「最近使用」栏 */
        WX_RECENT_TOP = 1089,

        /** 1090-长按小程序右上角退出键唤出最近使用历史 */
        WX_RECENT_EXIT = 1090,

        /** 1091-公众号文章商品卡片 */
        WX_ARTICLE_CARD = 1091,

        /** 1092-城市服务入口 */
        WX_CITYSERVICE = 1092,

        /** 1095-小程序广告组件 */
        WX_MICRO_AD = 1095,

        /** 1096-聊天记录，打开小程序 */
        WX_CHATRECORD = 1096,

        /** 1097-微信支付签约原生页，打开小程序 */
        WX_PAY_SIGN = 1097,

        /** 1099-页面内嵌插件 */
        WX_PLUGIN = 1099,

        /** 1100-红包封面详情页打开小程序 */
        WX_REDPACKET = 1100,

        /** 1101-远程调试热更新（开发者工具中，预览 -> 自动预览 -> 编译并预览） */
        WX_DEBUG = 1101,

        /** 1102-公众号profile页服务预览模块，打开小程序 */
        WX_PROFILE_PREVIEW = 1102,

        /** 1103-发现页小程序「我的小程序」列表（基础库2.2.4-2.29.0版本废弃，2.29.1版本起生效） */
        WX_MYAPP = 1103,

        /** 1104-微信聊天主界面下拉，「我的小程序」栏（基础库2.2.4-2.29.0版本废弃，2.29.1版本起生效） */
        WX_MYAPP_RECENT = 1104,

        /** 1106-聊天主界面下拉，从顶部搜索结果页，打开小程序 */
        WX_SEARCH_TOP = 1106,

        /** 1107-订阅消息，打开小程序（与1014相同） */
        WX_SUBSCRIBE_2 = 1107,

        /** 1113-安卓手机负一屏，打开小程序（三星） */
        WX_SAMSUNG = 1113,

        /** 1114-安卓手机侧边栏，打开小程序（三星） */
        WX_SAMSUNG_SIDE = 1114,

        /** 1124-扫“一物一码”打开小程序 */
        WX_QRCODE_ONE = 1124,

        /** 1125-长按图片识别“一物一码” */
        WX_QRCODE_LONG = 1125,

        /** 1126-扫描手机相册中选取的“一物一码” */
        WX_QRCODE_ALBUM = 1126,

        /** 1129-微信爬虫访问 详情 */
        WX_SPIDER = 1129,

        /** 1131-浮窗（8.0版本起仅包含被动浮窗） */
        WX_FLOAT = 1131,

        /** 1133-硬件设备打开小程序 详情 */
        WX_DEVICE = 1133,

        /** 1135-小程序profile页其他小程序列表，打开小程序 */
        WX_PROFILE_OTHER = 1135,

        /** 1144-公众号文章 - 视频贴片 */
        WX_ARTICLE_VIDEO = 1144,

        /** 1145-发现栏 - 发现小程序 */
        WX_DISCOVER = 1145,

        /** 1146-地理位置信息打开出行类小程序 */
        WX_LOCATION = 1146,

        /** 1148-卡包-交通卡，打开小程序 */
        WX_TRAFFIC = 1148,

        /** 1150-扫一扫商品条码结果页打开小程序 */
        WX_BARCODE = 1150,

        /** 1152-订阅号视频打开小程序 */
        WX_VIDEO = 1152,

        /** 1153-“识物”结果页打开小程序 */
        WX_IDENTITY = 1153,

        /** 1154-朋友圈内打开“单页模式” */
        WX_FRIEND = 1154,

        /** 1155-“单页模式”打开小程序 */
        WX_SINGLE = 1155,

        /** 1158-群工具打开小程序 */
        WX_GROUPTOOL = 1158,

        /** 1160-群待办 */
        WX_GROUPTOOL_TODO = 1160,

        /** 1167-H5 通过开放标签打开小程序 */
        WX_TAG = 1167,

        /** 1168-移动/网站应用直接运行小程序 */
        WX_DIRECT = 1168,

        /** 1177-视频号直播商品 */
        WX_LIVE = 1177,

        /** 1178-在电脑打开手机上打开的小程序 */
        WX_PC = 1178,

        /** 1179-#话题页打开小程序 */
        WX_TOPIC = 1179,

        /** 1181-网站应用打开PC小程序 */
        WX_PC_WEB = 1181,

        /** 1184-视频号链接打开小程序 */
        WX_VIDEO_LINK = 1184,

        /** 1187-浮窗（8.0版本起） */
        WX_FLOAT_NEW = 1187,

        /** 1193-视频号主页服务菜单打开小程序 */
        WX_HOME = 1193,

        /** 1194-URL Link */
        WX_URL = 1194,

        /** 1197-视频号主播从直播间返回小游戏 */
        WX_LIVE_BACK = 1197,

        /** 1198-视频号开播界面打开小游戏 */
        WX_LIVE_OPEN = 1198,

        /** 1199-小游戏内“更多游戏”入口打开小程序 */
        WX_MORE = 1199,

        /** 1200-视频号广告打开小程序 */
        WX_CHANNEL_AD = 1200,

        /** 1201-视频号广告详情页打开小程序 */
        WX_CHANNEL_DETAIL_AD = 1201,

        /** 1205-非广告进入视频号直播间打开游戏卡片 */
        WX_LIVE_CARD = 1205,

        /** 1206-视频号小游戏直播间打开小游戏 */
        WX_LIVE_GAME = 1206,

        /** 1219-视频号直播间小游戏一键上车 */
        WX_LIVE_UP = 1219,

        /** 1223-安卓桌面Widget打开小程序 */
        WX_WIDGET = 1223,

        /** 1228-视频号原生广告组件打开小程序 */
        WX_CHANNEL_NATIVE_AD = 1228,

        /** 1230-订阅号H5广告进入小程序 */
        WX_SUBSCRIBE_H5_AD = 1230,

        /** 1231-动态消息提醒入口打开小程序 */
        WX_REMIND = 1231,

        /** 1232-搜一搜竞价广告打开小程序 */
        WX_SEARCH_AD = 1232,

        /** 1233-小程序搜索推荐模块打开小游戏 */
        WX_SEARCH_GAME = 1233,

        /** 1238-看一看信息流广告打开小程序 */
        WX_LOOK_AD = 1238,

        /** 1239-视频号小游戏直播间气泡浮窗打开小游戏 */
        WX_LIVE_BUBBLE = 1239,
    }
}

#endif
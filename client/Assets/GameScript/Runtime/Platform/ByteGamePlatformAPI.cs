using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameScript.Runtime.Platform;
using MyUI;
using UIFramework;
using UnityEngine;
using Application = UnityEngine.Application;

#if PLATFORM_DOUYIN
using GameScript.Runtime.GameLogic;
using StarkSDKSpace;
using StarkSDKSpace.UNBridgeLib.LitJson;

namespace GameScript.Runtime.Platform
{
    public class ByteGamePlatformAPI : IPlatformAPI
    {
        private StarkGyroscope         _gyroscope;
        private OnGameGyroscopeChanged _onGameGyroscopeChanged;

        private readonly Action<PlatformFunc, string, bool> _useApiErrorCallback;

        private const string               ADUnitIdTt = "d19j093f102301jeca"; // 拔罐
        private       Action<AdState, int> onVideoAdCallback;

        public ByteGamePlatformAPI(Action<PlatformFunc, string, bool> useApiErrorCallback)
        {
            _useApiErrorCallback = useApiErrorCallback;
            Init();
        }
        
        
        private void Init()
        {
            StarkSDKSpace.StarkSDK.API.GetStarkAppLifeCycle().OnShowWithDict+= OnShowOneParam;
            StarkSDKSpace.StarkSDK.API.GetStarkAppLifeCycle().OnHide += OnHide;
        }
        
        private void OnShowOneParam(Dictionary<string, object> param)
        {
            try
            {
                // PrintTextAppended($"OnShowOneParam-->${param.ToJson()}");
                // 获取卡片数据
                Dictionary<string, string> data = null;
                if (param.ContainsKey("query"))
                {
                    data =  (Dictionary<string, string>)param["query"];
                }
                // 处理卡片数据
                GameInstance.Instance.CheckLaunchQuery(false,data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void OnHide()
        {
            //
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


        public bool IsSupport(PlatformFunc func)
        {
            switch (func)
            {
                case PlatformFunc.Vibrate:
                    return CanIUse.Vibrate;
                case PlatformFunc.Gyroscope:
                    return CanIUse.GetStarkGyroscope;
                case PlatformFunc.NotchFitter:
                    return true;
                case PlatformFunc.SliderBarNavigateToScene:
                    return CanIUse.StarkSideBar.NavigateToScene;
                case PlatformFunc.GetGameRecorder:
                    return CanIUse.GetStarkGameRecorder;
                case PlatformFunc.StartRecord:
                    return CanIUse.StarkGameRecorder.StartRecord;
                case PlatformFunc.StopRecord:
                    return CanIUse.StarkGameRecorder.StopRecord;
                case PlatformFunc.GetGameRecorderState:
                    return CanIUse.StarkGameRecorder.GetVideoRecordState;
                case PlatformFunc.Sharable:
                    return CanIUse.GetStarkShare;
                case PlatformFunc.AddShortcut:
                    return CanIUse.CreateShortcut;
                default:
                    return false;
            }
        }

        public UniTask<LoginResult> Login()
        {
            var result = new UniTaskCompletionSource<LoginResult>();
            StarkSDKSpace.StarkSDK.API.GetAccountManager().Login(
                (code, anonymousCode, isLogin) =>
                {
                    Debug.Log("Login success code " + code);
                    LoginResult loginResult = new LoginResult();
                    loginResult.code = code;
                    result.TrySetResult(loginResult);
                },
                (errMsg) =>
                {
                    Debug.Log("Login fail errMsg " + errMsg);
                    LoginResult loginResult = new LoginResult();
                    loginResult.errCode = -1;
                    loginResult.errMsg = errMsg;
                    result.TrySetResult(loginResult);
                }, false);

            return result.Task;
        }

        public void Share(string title = "", string imageUrl = "", Action<bool> callback = null)
        {
            //如果不支持该接口，认为分享成功
            if (!CanIUse.GetStarkShare)
            {
                callback?.Invoke(true);
                return;
            }

            var json = new StarkSDKSpace.UNBridgeLib.LitJson.JsonData();
            var index = UnityEngine.Random.Range(0, titles.Length);
            json["title"] = string.IsNullOrEmpty(title) ? TargetShareTitle(index) : title;
            json["imageUrl"] = string.IsNullOrEmpty(imageUrl) ? TargetShareImageUrl(index) : imageUrl;
            StarkSDKSpace.StarkSDK.API.GetStarkShare().ShareAppMessage(data => { callback?.Invoke(true); }, errMsg => { callback?.Invoke(false); }, () => { callback?.Invoke(false); }, json);
        }
        
        /// <summary>
        /// 分享-带参数
        /// </summary>
        /// <param name="shareMessage"></param>
        /// <param name="callback"></param>
        public void ShareMessage(ShareMessageType shareMessage, Action<bool> callback = null)
        {
                //如果不支持该接口，认为分享成功
                if (!CanIUse.GetStarkShare)
                {
                    callback?.Invoke(true);
                    return;
                }

                var json = new StarkSDKSpace.UNBridgeLib.LitJson.JsonData();
                var index = UnityEngine.Random.Range(0, titles.Length);
                json["title"] = string.IsNullOrEmpty(shareMessage.title) ? TargetShareTitle(index) : shareMessage.title;
                json["imageUrl"] = string.IsNullOrEmpty(shareMessage.imageUrl) ? TargetShareImageUrl(index) : shareMessage.imageUrl;
                json["query"] = string.IsNullOrEmpty(shareMessage.query) ? "" : shareMessage.query;
                StarkSDKSpace.StarkSDK.API.GetStarkShare().ShareAppMessage(data => { callback?.Invoke(true); }, errMsg => { callback?.Invoke(false); }, () => { callback?.Invoke(false); }, json);
        }

        public void SetClipboardData(string text)
        {
            StarkSDK.API.GetStarkClipboard().SetClipboardData(text, (isSuccess, errMsg) =>
            {
                if (isSuccess)
                {
                }
                else
                {
                }
            });
        }

        public void StartGyroscope(double interval, Action<bool, string> statusCallback, OnGameGyroscopeChanged gyroscopeChanged)
        {
            if (!CanIUse.GetStarkGyroscope)
            {
                _useApiErrorCallback?.Invoke(PlatformFunc.Gyroscope, null, false);
                return;
            }

            if (_gyroscope == null)
            {
                _gyroscope = StarkSDK.API.GetStarkGyroscope();
                _gyroscope.StartGyroscope(interval, statusCallback);
                _gyroscope.OnGyroscopeChangedHandler += OnGyroscopeChanged;
            }

            _onGameGyroscopeChanged += gyroscopeChanged;
        }

        void OnGyroscopeChanged(double x, double y, double z, double t, double roll, double pitch, double yaw, double result)
        {
            _onGameGyroscopeChanged?.Invoke(x, y, z, t, roll, pitch, yaw, result);
        }

        public void StopGyroscope(Action<bool, string> statusCallback)
        {
            if (!CanIUse.GetStarkGyroscope)
            {
                _useApiErrorCallback?.Invoke(PlatformFunc.Gyroscope, null, false);
                return;
            }

            if (_gyroscope != null)
            {
                _gyroscope.StopGyroscope(statusCallback);
            }

            _onGameGyroscopeChanged = null;
            _gyroscope = null;
        }

        public void Vibrate(long[] pattern, int repeat)
        {
            if (!CanIUse.Vibrate)
            {
                _useApiErrorCallback?.Invoke(PlatformFunc.Vibrate, null, false);
                return;
            }

            StarkSDK.API.Vibrate(pattern, repeat);
        }

        #region notch

        private float _notchBottomY = float.MaxValue;

        private float NotchBottomY
        {
            get
            {
                if (Math.Abs(_notchBottomY - float.MaxValue) < 0.001f)
                {
                    var layoutData = StarkSDKSpace.StarkSDK.API.GetMenuButtonLayout();
                    _notchBottomY = layoutData.OptGetInt("bottom");
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

        #endregion

        public void SliderBarNavigation(GameSliderSceneEnum sceneEnum, Action successCallback, Action completeCallback, Action<int, string> errorCallback)
        {
            if (!CanIUse.StarkSideBar.NavigateToScene)
            {
                errorCallback?.Invoke(-1, "not support");
                return;
            }

            StarkSideBar.SceneEnum sliderEnum = (StarkSideBar.SceneEnum)(int)sceneEnum;
            StarkSDK.API.GetStarkSideBarManager().NavigateToScene(sliderEnum, () => { successCallback?.Invoke(); },
                                                                  () => { completeCallback?.Invoke(); }, (eCode, eMsg) =>
                                                                  {
                                                                      if (errorCallback != null)
                                                                          errorCallback?.Invoke(eCode, eMsg);
                                                                  });
        }

        #region video record

        private StarkGameRecorder _recorder;
        private object            _starkGameRecorder;

        void DoGetRecord()
        {
            if (_recorder == null)
            {
                if (!CanIUse.GetStarkGameRecorder)
                {
                    return;
                }

                _recorder = StarkSDK.API.GetStarkGameRecorder();
            }
        }

        public void StartRecord(bool isRecordAudio = true, int maxRecordTimeSec = 600, Action startCallback = null, Action<int, string> errorCallback = null, Action<string> timeoutCallback = null)
        {
            DoGetRecord();
            if (!CanIUse.StarkGameRecorder.StartRecord)
            {
                errorCallback?.Invoke(-2, "not support");
                return;
            }

            if (_recorder.GetVideoRecordState() != StarkGameRecorder.VideoRecordState.RECORD_STARTED)
            {
                _recorder.StartRecord(isRecordAudio, maxRecordTimeSec, () => { startCallback?.Invoke(); }, (errorCode, errorStr) =>
                {
                    if (errorCallback != null)
                        errorCallback?.Invoke(errorCode, errorStr);
                    else
                    {
                        _useApiErrorCallback?.Invoke(PlatformFunc.StartRecord, errorStr, true);
                    }
                }, (videoPath) => { timeoutCallback?.Invoke(videoPath); });
            }
        }

        public void StopRecord(Action<string> completeCallback, Action<int, string> errorCallback)
        {
            DoGetRecord();
            if (!CanIUse.StarkGameRecorder.GetVideoRecordState)
            {
                errorCallback?.Invoke(-1, "not support");
                return;
            }

            if (_recorder != null)
                _recorder.StopRecord((videoPath) => { completeCallback?.Invoke(videoPath); }, (errCode, errMsg) =>
                {
                    if (errorCallback != null)
                        errorCallback?.Invoke(errCode, errMsg);
                    else
                    {
                        _useApiErrorCallback?.Invoke(PlatformFunc.StopRecord, errMsg, false);
                    }
                });
        }

        public void ShareGameRecord(Action<Dictionary<string, object>> successCallback, Action<string> errorCallback, Action cancelCallback)
        {
            DoGetRecord();
            if (!CanIUse.StarkGameRecorder.ShareVideo)
            {
                errorCallback?.Invoke("not support");
                return;
            }

            if (_recorder != null)
            {
                _recorder.ShareVideo(result => { successCallback?.Invoke(result); }, errMsg => { errorCallback?.Invoke(errMsg); }, () => { cancelCallback?.Invoke(); });
            }
        }

        public GameVideoRecordState GetVideoRecordState()
        {
            if (CanIUse.StarkGameRecorder.GetVideoRecordState)
            {
                if (_recorder != null)
                    return (GameVideoRecordState)((int)_recorder.GetVideoRecordState());
            }

            return GameVideoRecordState.RECORD_STOPED;
        }

        #endregion

        #region rank

        public void SetRankScore(int score)
        {
            StarkSDK.API.GetStarkRank().SetImRankDataV2(
                new JsonData
                {
                    ["dataType"] = 0,
                    ["value"] = $"{score}",
                    ["zoneId"] = "default"
                },
                (isSuccess, errMsg) =>
                {
                    Debug.Log("========>>>>Set Score : [" + isSuccess + "] " + errMsg + "!");
                    if (isSuccess)
                    {
                    }
                    else
                    {
                    }
                });
        }

        public void GetRankList(Action<bool, string> action, String type = null, String suffix = null)
        {
            if (type != "day" && type != "week" && type != "month")
            {
                type = "all";
            }

            StarkSDK.API.GetStarkRank().GetImRankListV2(
                new JsonData
                {
                    ["rankType"] = type,
                    ["dataType"] = 0,
                    ["relationType"] = "default",
                    ["suffix"] = suffix,
                    ["zoneId"] = "default",
                },
                (isSuccess, errMsg) => { action?.Invoke(isSuccess, errMsg); });
        }

        public string GetDevicePlatform()
        {
            if (CanIUse.GetSystemInfo)
            {
                return StarkSDK.API.GetSystemInfo().platform; // "ios"、"android"...
            }

            return Application.platform.ToString().ToLower();
        }

        #endregion

        #region video ad

        public void InitVideoAd()
        {
            // TT好像不需要Create视频广告
        }


        public async UniTask<(AdState, string)> PlayVideoAd(CancellationToken cancellationToken)
        {
            UniTaskCompletionSource<(AdState, string)> taskCompletionSource = new UniTaskCompletionSource<(AdState, string)>();
            StarkSDK.API.GetStarkAdManager().ShowVideoAdWithId(ADUnitIdTt, (isComplete) => { taskCompletionSource.TrySetResult((isComplete ? AdState.Success : AdState.Exit, string.Empty)); },
                                                               (errCode, errMsg) =>
                                                               {
                                                                   var state = AdState.Error;
                                                                   // 1004 无合适的广告 1005 广告组件审核中 1006 广告组件被驳回
                                                                   if (errCode is 1000 or 1002 or 1004 or 1005 or 1006)
                                                                   {
                                                                       state = AdState.AdToShare;
                                                                   }

                                                                   taskCompletionSource.TrySetResult((state, $"err:{errMsg} {errCode}"));
                                                               });
            return await taskCompletionSource.Task;
        }

        private void CloseCallback(bool isComplete)
        {
            onVideoAdCallback?.Invoke(isComplete ? AdState.Success : AdState.Exit, 0);
            onVideoAdCallback = null;
        }

        private void ErrCallback(int errCode, string errMsg)
        {
            Debug.Log($"video ad err {errCode} {errMsg}");
            var state = AdState.Error;
            // 1004 无合适的广告 1005 广告组件审核中 1006 广告组件被驳回
            if (errCode is 1000 or 1002 or 1004 or 1005 or 1006)
            {
                state = AdState.AdToShare;
            }

            onVideoAdCallback?.Invoke(state, errCode);
            onVideoAdCallback = null;
        }

        #endregion


        /// <summary>
        /// 获取平台用户信息
        /// </summary>
        /// <returns></returns>
        public void GetPlatformUserInfo(Action<PlatformUserInfo, GetPlatformUserInfoState, string> userInfoCallback)
        {
            //  ScUserInfo scUserInfo;
            if (CanIUse.StarkAccount.GetScUserInfo)
            {
                StarkSDK.API.GetAccountManager().GetScUserInfo((ref ScUserInfo scUserInfo) =>
                {
                    PlatformUserInfo platformUserInfo = new PlatformUserInfo()
                                                        {
                                                            NickName = scUserInfo.nickName,
                                                            Avatar = scUserInfo.avatarUrl,
                                                        };
                    userInfoCallback?.Invoke(platformUserInfo, GetPlatformUserInfoState.Agree, string.Empty);
                }, (string errorMsg) => { userInfoCallback?.Invoke(default, GetPlatformUserInfoState.Error, string.Empty); });
            }
            else
            {
                userInfoCallback?.Invoke(default, GetPlatformUserInfoState.Error, "GetScUserInfo can't support");
            }
        }

        public void IsAuthor(Action<bool> getResultCallback, Action<string> failCallback)
        {
            //StarkSDK.API.GetAccountManager().GetSetting 返回的授权始终是false,所以只能通过GetUserInfoAuth获取
            //但是GetUserInfoAuth方法，如果从没有授权过，会返回error:invalid json，因为无法判断所以error也视为 未授权状态
            if (CanIUse.StarkAccount.GetUserInfoAuth)
            {
                StarkSDK.API.GetAccountManager().GetUserInfoAuth((isAuthor) => { getResultCallback?.Invoke(isAuthor); }, (error) =>
                {
                    //从未申请过授权返回 invalid json
                    if (error.Contains("invalid json"))
                    {
                        getResultCallback?.Invoke(false);
                    }
                    else
                    {
                        failCallback?.Invoke(error);
                    }

                    getResultCallback?.Invoke(false);
                });
            }
            else
            {
                failCallback?.Invoke("GetUserInfoAuth can't support");
            }
        }

        /// <summary>
        /// 创建授权透明按钮
        /// </summary>
        /// <param name="x">按钮位置</param>
        /// <param name="y">按钮位置</param>
        /// <param name="width">按钮宽度</param>
        /// <param name="height">按钮高度</param>
        /// <param name="language">语言</param>
        /// <param name="withCredentials">是否带有登陆信息</param>
        /// <param name="tabCallback">点击回调，用户数据，是否点击了同意，错误信息</param>
        private const string RefuseError = "auth deny";

        public void CreateUserInfoBtn(int x, int y, int width, int height, string language, bool withCredentials, Action<PlatformUserInfo, GetPlatformUserInfoState, string> tabCallback)
        {
            if (CanIUse.StarkAccount.GetScUserInfo)
            {
                StarkSDK.API.GetAccountManager().GetScUserInfo((ref ScUserInfo scUserInfo) =>
                {
                    PlatformUserInfo platformUserInfo = new PlatformUserInfo()
                                                        {
                                                            NickName = scUserInfo.nickName,
                                                            Avatar = scUserInfo.avatarUrl,
                                                        };
                    tabCallback?.Invoke(platformUserInfo, GetPlatformUserInfoState.Agree, string.Empty);
                }, (string errorMsg) =>
                {
                    if (errorMsg.Contains(RefuseError))
                    {
                        //拒绝也要设置已经请求过授权
                        tabCallback?.Invoke(default, GetPlatformUserInfoState.Refuse, errorMsg);
                    }
                    else
                    {
                        tabCallback?.Invoke(default, GetPlatformUserInfoState.Error, errorMsg);
                    }
                });
            }
            else
            {
                tabCallback?.Invoke(default, GetPlatformUserInfoState.Error, "GetScUserInfo can't support");
            }
        }

        public GameWindowInfo GetWindowInfo()
        {
            return new GameWindowInfo()
                   {
                       pixelRatio = 1,
                       screenHeight = Screen.height,
                       screenWidth = Screen.width
                   };
        }

        /// <summary>
        /// 设置帧率
        /// </summary>
        public void SetTargetFrame(int targetFrame)
        {
            //没从api 里找到设置帧率的方法，先用unity 的，不一定生效
            Application.targetFrameRate = targetFrame;
        }

        /// <summary>
        /// 是否需要打开隐私授权界面，微信需要
        /// </summary>
        /// <returns></returns>
        public void GetPrivacyAuthorize(Action<bool> callback)
        {
            callback?.Invoke(false);
        }

        /// <summary>
        /// 打开隐私授权界面，微信需要
        /// </summary>
        /// <param name="callback">是否同意授权</param>
        public void OpenPrivacyAuthorize(Action<bool> callback)
        {
            callback?.Invoke(true);
        }

        public bool IsShortcutExist()
        {
            return CanIUse.IsShortcutExist;
        }

        public void AddShortcut()
        {
            StarkSDK.API.CreateShortcut(OnCreateShortcut);
        }

        private void OnCreateShortcut(bool bSuccess)
        {
            Debug.Log($"OnCreateShortcut : {bSuccess}");
        }

        public Dictionary<string, string> GetLaunchQuery()
        {
            var launchOption = StarkSDK.API.GetLaunchOptionsSync();
            return launchOption.Query;
        }

        public string GetLaunchString()
        {
            var launchOption = StarkSDK.API.GetLaunchOptionsSync();
            return JsonMapper.ToJson(launchOption);
        }

        public bool LaunchFromScene(FromSceneType type)
        {
            string sceneID = StarkSDK.API.GetLaunchOptionsSync().Scene;
            Debug.Log($"launch from scene {sceneID}");
            switch (type)
            {
                case FromSceneType.All:
                    return true;
                case FromSceneType.AdScene:
                    return sceneID == TTSceneID.DY_AD
                           || sceneID == TTSceneID.DYS_AD
                           || sceneID == TTSceneID.TT_AD
                           || sceneID == TTSceneID.TT_AD_START
                           || sceneID == TTSceneID.TT_AD_FEED;
            }
            return false;
        }

        /// <summary>
        /// 游戏版本号
        /// </summary>
        /// <returns></returns>
        public string GetGameVersion()
        {
            var version = StarkSDK.API.GameVersion;
            if (string.IsNullOrEmpty(version))
                return Application.version;
            return version;
        }

        /// <summary>
        /// 检查登陆session 是否有效果
        /// </summary>
        /// <returns></returns>
        public UniTask<bool> CheckSession()
        {
            UniTaskCompletionSource<bool> taskCompletionSource = new UniTaskCompletionSource<bool>();
            if (!CanIUse.StarkAccount.CheckSession)
            {
                taskCompletionSource.TrySetResult(true);
                return  taskCompletionSource.Task;
            }
            StarkSDK.API.GetAccountManager().CheckSession(() =>
            {
                taskCompletionSource.TrySetResult(true);
            }, (errMsg) =>
            {
                taskCompletionSource.TrySetResult(false);
            });
            return taskCompletionSource.Task;
        }
        

        /// <summary>
        /// 平台输出debug日志
        /// </summary>
        public void Log(string logContent,LogType logType)
        {
            switch (logType)
            {
                case LogType.Debug:
                case LogType.Log:
                case LogType.Info:
                    Debug.Log(logContent);
                    break;
                case LogType.Warn:
                    Debug.LogWarning(logContent);
                    break;
                case LogType.Error:
                    Debug.LogError(logContent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }
        
        /// <summary>
        /// 获取启动场景值
        /// </summary>
        /// <returns></returns>
        public double GetLaunchSceneCode()
        {
            return 0; // 默认平台没有场景值
        }

        /// <summary>
        /// 是否来自公众号（默认是否）
        /// </summary>
        /// <returns></returns>
        public bool IsLaunchSceneFromWxPublicAccount()
        {
            return false;
        }
        
    }

    class TTSceneID {
        /* ******************************** *
         * ************开发者工具场景值********
         * ******************************** */
        /** 抖音开发者工具启动场景-990001 */
        public static readonly string DEV_START = "990001";

        /* ******************************** *
         * ************今日头条场景值********
         * ******************************** */
        /** 固定入口 011-搜索页固定入口-上方最近使用-011001 */
        public static readonly string TT_RECENTLY_USED = "011001";
        /** 固定入口 011-搜索页固定入口-上方推荐-011002 */
        public static readonly string TT_RECOMMEND = "011002";
        /** 固定入口 011-搜索页固定入口-下方推荐-011003 */
        public static readonly string TT_RECOMMEND_BOTTOM = "011003";
        /** 固定入口 011-小程序中心-最近使用-011004 */
        public static readonly string TT_MY_RECENTLY = "011004";
        /** 固定入口 011-小程序中心-收藏-011005 */
        public static readonly string TT_COLLECTION = "011005";
        /** 固定入口 011-小程序中心-发现-011006 */
        public static readonly string TT_DISCOVER = "011006";
        /** 固定入口 011-扫一扫-011007 */
        public static readonly string TT_SCAN = "011007";
        /** 固定入口 011-业务固定入口-011008 */
        public static readonly string TT_BUSINESS = "011008";
        /** 固定入口 011-小程序跳小程序-011009 */
        public static readonly string TT_JUMP = "011009";
        /** 固定入口 011-小程序返回小程序-011010 */
        public static readonly string TT_BACK = "011010";
        /** 固定入口 011-我的-发现 12 宫格-011014 */
        public static readonly string TT_MY_DISCOVER = "011014";
        /** 固定入口 011-小游戏频道页-011015 */
        public static readonly string TT_GAME = "011015";
        /** 固定入口 011-小游戏底 tab-011016 */
        public static readonly string TT_GAME_TAB = "011016";
        /** 固定入口 011-我的-头条小游戏盒子-011017 */
        public static readonly string TT_GAME_BOX = "011017";
        /** 固定入口 011-音频入口浮窗-011018 */
        public static readonly string TT_AUDIO_FLOAT = "011018";
        /** 固定入口 011-音频入口通知栏-011019 */
        public static readonly string TT_AUDIO_NOTIFY = "011019";
        /** 固定入口 011-小程序桌面快捷方式-011020 */
        public static readonly string TT_DESKTOP = "011020";
        /** 固定入口 011-我的-订单入口-011021 */
        public static readonly string TT_MY_ORDER = "011021";
        /** 搜索入口 012-小程序卡片搜索结果，直接搜索名字出来的结果-012001 */
        public static readonly string TT_SEARCH = "012001";
        /** 搜索入口 012-小程序中心搜索结果-012002 */
        public static readonly string TT_SEARCH_CENTER = "012002";
        /** 搜索入口 012-搜索阿拉丁-012003 */
        public static readonly string TT_SEARCH_ALARDING = "012003";
        /** 搜索入口 012-自然搜索结果-012004 */
        public static readonly string TT_SEARCH_NATURAL = "012004";
        /** 搜索入口 012-搜索出 feed 中小程序内部文章-012005 */
        public static readonly string TT_SEARCH_FEED = "012005";
        /** 内容入口 013-分享的微头条-013001 */
        public static readonly string TT_SHARE = "013001";
        /** 内容入口 013-小视频详情页入口-右侧 icon 样式-013002 */
        public static readonly string TT_VIDEO_RIGHT_ICON = "013002";
        /** 内容入口 013-小视频详情页入口-左侧链接样式-013003 */
        public static readonly string TT_VIDEO_LEFT_URL = "013003";
        /** 内容入口 013-小视频详情页评论区-013004 */
        public static readonly string TT_VIDEO_COMMENT = "013004";
        /** 内容入口 013-文章详情页入口-013005 */
        public static readonly string TT_ARTICLE = "013005";
        /** 内容入口 013-话题详情页入口-013007 */
        public static readonly string TT_TOPIC = "013007";
        /** 内容入口 013-头条号个人主页入口-013008 */
        public static readonly string TT_PERSONAL = "013008";
        /** 内容入口 013-游戏频道置顶小游戏卡片-013009 */
        public static readonly string TT_GAME_TOP_CARD = "013009";
        /** 内容入口 013-发布的微头条-013010 */
        public static readonly string TT_PUBLISH_MICRO_ARTICLE = "013010";
        /** 内容入口 013-feed 流小游戏小卡片-013011 */
        public static readonly string TT_FEED_CARD_SMALL = "013011";
        /** 内容入口 013-feed 流小游戏大卡片-013012 */
        public static readonly string TT_FEED_CARD_BIG = "013012";
        /** 内容入口 013-推荐-feed 流小程序卡片-013013 */
        public static readonly string TT_FEED_CARD = "013013";
        /** 分享 014-微信对话-014001 */
        public static readonly string TT_SHARE_WECHAT = "014001";
        /** 分享 014-微信朋友圈-014002 */
        public static readonly string TT_SHARE_WECHAT_FRIEND = "014002";
        /** 分享 014-QQ 对话-014003 */
        public static readonly string TT_SHARE_QQ = "014003";
        /** 分享 014-Qzone-014004 */
        public static readonly string TT_SHARE_QZONE = "014004";
        /** 分享 014-钉钉-014005 */
        public static readonly string TT_SHARE_DING = "014005";
        /** 分享 014-系统分享-014006 */
        public static readonly string TT_SHARE_SYSTEM = "014006";
        /** 分享 014-复制链接-014007 */
        public static readonly string TT_SHARE_COPY = "014007";
        /** 分享 014-私信-014008 */
        public static readonly string TT_SHARE_PRIVATE = "014008";
        /** 分享 014-口令分享-014009 */
        public static readonly string TT_SHARE_COMMAND = "014009";
        /** 推广或其他入口 015-本地频道-频道顶部 widget-015001 */
        public static readonly string TT_LOCAL_CHANNEL = "015001";
        /** 推广或其他入口 015-钱包入口-015002 */
        public static readonly string TT_WALLET = "015002";
        /** 广告 016-文章详情页匹配广告-016001 */
        public static readonly string TT_AD = "016001";
        /** 广告 016-Feed 流广告-016002 */
        public static readonly string TT_AD_FEED = "016002";
        /** 广告 016-开屏广告-016003 */
        public static readonly string TT_AD_START = "016003";
        /** 消息触达 017-app 的推送-017001 */
        public static readonly string TT_PUSH = "017001";
        /** 消息触达 017-系统消息通知-017002 */
        public static readonly string TT_NOTIFY = "017002";
        /** 消息触达 017-客服能力；开发者与用户沟通-017003 */
        public static readonly string TT_SERVICE = "017003";
        /** 消息触达 017-短信链接跳转小程序-017004 */
        public static readonly string TT_SMS = "017004";

        /* ******************************** *
         * ************抖音场景值********
         * ******************************** */
        /** 固定入口 021-我的-小程序-021001 */
        public static readonly string DY_MY_MICRO = "021001";
        /** 固定入口 021-全局扫一扫-021002 */
        public static readonly string DY_SCAN = "021002";
        /** 固定入口 021-我的-收藏 tab 入口-021003 */
        public static readonly string DY_COLLECT = "021003";
        /** 固定入口 021-业务固定入口-021008 */
        public static readonly string DY_BUSINESS = "021008";
        /** 固定入口 021-小程序跳小程序-021009 */
        public static readonly string DY_JUMP = "021009";
        /** 固定入口 021-小程序返回小程序-021010 */
        public static readonly string DY_BACK = "021010";
        /** 固定入口 021-订单综合入口-021013 */
        public static readonly string DY_ORDER = "021013";
        /** 固定入口 021-发布页锚点入口-021014 */
        public static readonly string DY_PUBLISH = "021014";
        /** 固定入口 021-发布页入口-021017 */
        public static readonly string DY_PUBLISH_PAGE = "021017";
        /** 固定入口 021-发布页添加团购组件入口-021018 */
        public static readonly string DY_PUBLISH_GROUP = "021018";
        /** 固定入口 021-小程序桌面快捷方式-021020 */
        public static readonly string DY_SHORTCUT = "021020";
        /** 固定入口 021-优惠券入口-021025 */
        public static readonly string DY_COUPON = "021025";
        /** 固定入口 021-钱包其他服务入口-021027 */
        public static readonly string DY_WALLET = "021027";
        /** 固定入口 021-钱包银行卡管理入口-021028 */
        public static readonly string DY_WALLET_BANK = "021028";
        /** 固定入口 021-福利中心-021031 */
        public static readonly string DY_WELFARE = "021031";
        /** 固定入口 021-个人页搜索小程序入口-021036 */
        public static readonly string DY_SEARCH = "021036";
        /** 固定入口 021-电影宣发工具-021037 */
        public static readonly string DY_MOVIE = "021037";
        /** 固定入口 021-钱包福利专区入口-021039 */
        public static readonly string DY_WELFARE_ENTRY = "021039";
        /** 固定入口 021-抖音ipad版-021042 */
        public static readonly string DY_IPAD = "021042";
        /** 固定入口 021-抖音小游戏中心-021043 */
        public static readonly string DY_GAME = "021043";
        /** 固定入口 021-抖音金币任务页-021046 */
        public static readonly string DY_COIN = "021046";
        /** 搜索入口 022-综合搜索-小程序入口-022001 */
        public static readonly string DY_SEARCH_MICRO = "022001";
        /** 搜索入口 022-搜索阿拉丁-022002 */
        public static readonly string DY_SEARCH_ALADDIN = "022002";
        /** 内容入口 023-视频详情页-023001 */
        public static readonly string DY_VIDEO = "023001";
        /** 内容入口 023-视频详情页评论区-023002 */
        public static readonly string DY_VIDEO_COMMENT = "023002";
        /** 内容入口 023-企业号主页-023003 */
        public static readonly string DY_ENTERPRISE_HOME = "023003";
        /** 内容入口 023-垂直话题入口-023004 */
        public static readonly string DY_VERTICAL = "023004";
        /** 内容入口 023-POI 详情页入口-023005 */
        public static readonly string DY_POI = "023005";
        /** 内容入口 023-影视锚点详情页入口-023006 */
        public static readonly string DY_MOVIE_DISPERSION = "023006";
        /** 内容入口 023-直播间主播侧-023009 */
        public static readonly string DY_LIVE = "023009";
        /** 内容入口 023-直播间用户侧-023010 */
        public static readonly string DY_LIVE_USER = "023010";
        /** 内容入口 023-直播间用户侧活动 banner 入口-023011 */
        public static readonly string DY_LIVE_USER_BANNER = "023011";
        /** 内容入口 023-直播间用户侧底部互动玩法入口-023012 */
        public static readonly string DY_LIVE_USER_INTERACTIVE = "023012";
        /** 内容入口 023-视频小说-023014 */
        public static readonly string DY_VIDEO_NOVEL = "023014";
        /** 内容入口 023-视频文章-023015 */
        public static readonly string DY_VIDEO_ARTICLE = "023015";
        /** 内容入口 023-视频小程序卡片-023016 */
        public static readonly string DY_VIDEO_MICRO = "023016";
        /** 内容入口 023-POI 六分屏-023020 */
        public static readonly string DY_POI_SIX = "023020";
        /** 内容入口 023-抖音搜索结果视频锚点-023023 */
        public static readonly string DY_SEARCH_VIDEO = "023023";
        /** 内容入口 023-城市POI入口-023028 */
        public static readonly string DY_POI_CITY = "023028";
        /** 内容入口 023-同城地图-023029 */
        public static readonly string DY_MAP = "023029";
        /** 内容入口 023-关注页挂件-023030 */
        public static readonly string DY_FOLLOW = "023030";
        /** 内容入口 023-小程序异形卡-023040 */
        public static readonly string DY_MICRO_CARD = "023040";
        /** 分享 024-私信-024001 */
        public static readonly string DY_SHARE = "024001";
        /** 分享 024-微信对话-024002 */
        public static readonly string DY_SHARE_WECHAT = "024002";
        /** 分享 024-微信朋友圈-024003 */
        public static readonly string DY_SHARE_WECHAT_FRIEND = "024003";
        /** 分享 024-QQ 对话-024004 */
        public static readonly string DY_SHARE_QQ = "024004";
        /** 分享 024-Qzone-024005 */
        public static readonly string DY_SHARE_QZONE = "024005";
        /** 分享 024-分享回流-024007 */
        public static readonly string DY_SHARE_BACKFLOW = "024007";
        /** 分享 024-口令分享-024008 */
        public static readonly string DY_SHARE_COMMAND = "024008";
        /** 广告 025-广告-025001 */
        public static readonly string DY_AD = "025001";
        /** 运营位置 026-banner-026001 */
        public static readonly string DY_BANNER = "026001";
        /** 运营位置 026-话题-026002 */
        public static readonly string DY_TOPIC = "026002";
        /** 运营位置 026-push-026003 */
        public static readonly string DY_PUSH = "026003";
        /** 运营位置 026-notice，消息通知-026004 */
        public static readonly string DY_NOTICE = "026004";
        /** 运营位置 026-企业号-026006 */
        public static readonly string DY_ENTERPRISE = "026006";
        /** 运营位置 026-我的 banner 入口-026007 */
        public static readonly string DY_MY = "026007";
        /** 运营位置 026-系统通知-026018 */
        public static readonly string DY_SYSTEM = "026018";
        /** 电商 027-种草页-027001 */
        public static readonly string ECOM_CROWN = "027001";
        /** 电商 027-橱窗入口-027002 */
        public static readonly string ECOM_CHAMBER = "027002";
        /** 电商 027-个人主页商品橱窗入口-027002 */
        public static readonly string ECOM_CHAMBER_PERSON = "027002";
        /** 电商 027-商品卡入口-027003 */
        public static readonly string ECOM_CARD = "027003";
        /** 电商 027-直播间购物车商品-027004 */
        public static readonly string ECOM_LIVE_CART = "027004";
        /** 电商 027-直播间购物车卡片-027005 */
        public static readonly string ECOM_LIVE_CARD = "027005";
        /** 电商 027-订单详情页-027006 */
        public static readonly string ECOM_ORDER = "027006";
        /** 电商 027-抖音商城-027007 */
        public static readonly string ECOM_DOUYIN = "027007";
        /** 电商 027-售后页入口-027008 */
        public static readonly string ECOM_AFTER = "027008";

        /* ******************************** *
         * ************今日头条速版场景值********
         * ******************************** */
        /** 固定入口 061-搜索页固定入口-上方最近使用-061001 */
        public static readonly string TTS_RECENTLY_USED = "061001";
        /** 固定入口 061-搜索页固定入口-上方推荐-061002 */
        public static readonly string TTS_RECOMMEND = "061002";
        /** 固定入口 061-搜索页固定入口-下方推荐-061003 */
        public static readonly string TTS_RECOMMEND_BOTTOM = "061003";
        /** 固定入口 061-我的-小程序列表最近使用-061004 */
        public static readonly string TTS_MY_RECENTLY = "061004";
        /** 固定入口 061-业务固定入口-061008 */
        public static readonly string TTS_BUSINESS = "061008";
        /** 固定入口 061-小程序跳小程序-061009 */
        public static readonly string TTS_JUMP = "061009";
        /** 固定入口 061-小程序返回小程序-061010 */
        public static readonly string TTS_BACK = "061010";
        /** 固定入口 061-小程序桌面快捷方式-061020 */
        public static readonly string TTS_DESKTOP = "061020";
        /** 固定入口 061-我的-订单入口-061021 */
        public static readonly string TTS_ORDER = "061021";
        /** 搜索入口 062-自然搜索结果，直接搜索名字出来的结果-062001 */
        public static readonly string TTS_SEARCH = "062001";
        /** 搜索入口 062-小程序列表搜索结果 -> 小程序盒子搜索结果-062002 */
        public static readonly string TTS_SEARCH_BOX = "062002";
        /** 搜索入口 062-搜索阿拉丁-062003 */
        public static readonly string TTS_SEARCH_ALADDIN = "062003";
        /** 搜索入口 062-站外搜索跳小程序-062004 */
        public static readonly string TTS_SEARCH_JUMP = "062004";
        /** 内容入口 063-生活频道-feed 流小游戏小卡片-063011 */
        public static readonly string TTS_FEED_GAME = "063011";
        /** 内容入口 063-生活频道-小程序卡片-063012 */
        public static readonly string TTS_LIFE = "063012";
        /** 内容入口 063-feed 流小程序卡片-063013 */
        public static readonly string TTS_FEED = "063013";
        /** 推广或其他 065-频道顶部 widget-065001 */
        public static readonly string TTS_TOP = "065001";
        /** 推广或其他 065-钱包入口-065002 */
        public static readonly string TTS_WALLET = "065002";
        /** 推广或其他 065-任务页-banner-065003 */
        public static readonly string TTS_TASK = "065003";
        /** 消息触达-067001 */
        public static readonly string TTS_MESSAGE = "067001";

        /* ******************************** *
         * ********抖音极速版(DYS)场景值********
         * ******************************** */
        /** 固定入口 101-设置页面入口-101001 */
        public static readonly string DYS_SETTING = "101001";
        /** 固定入口 101-扫一扫入口-101002 */
        public static readonly string DYS_SCAN = "101002";
        /** 固定入口 101-售后页入口-101008 */
        public static readonly string DYS_AFTER = "101008";
        /** 固定入口 101-小程序互跳-101009 */
        public static readonly string DYS_JUMP = "101009";
        /** 固定入口 101-返回小程序-101010 */
        public static readonly string DYS_BACK = "101010";
        /** 固定入口 101-抖极金币任务页-101011 */
        public static readonly string DYS_TASK = "101011";
        /** 固定入口 101-抖音小游戏盒子-101012 */
        public static readonly string DYS_BOX = "101012";
        /** 固定入口 101-抖极金币游戏中心-101013 */
        public static readonly string DYS_GAME = "101013";
        /** 固定入口 101-金币游戏中心 H5 版-101014 */
        public static readonly string DYS_GAME_H5 = "101014";
        /** 固定入口 101-金币游戏中心新版-101015 */
        public static readonly string DYS_GAME_NEW = "101015";
        /** 固定入口 101-桌面图标-101020 */
        public static readonly string DYS_DESKTOP = "101020";
        /** 固定入口 101-抖极电商-101021 */
        public static readonly string DYS_ECOMMERCE = "101021";
        /** 固定入口 101-抖极首页侧边栏-101036 */
        public static readonly string DYS_HOME = "101036";
        /** 搜索入口 102-抖极游戏搜索-102001 */
        public static readonly string DYS_SEARCH_GAME = "102001";
        /** 搜索入口 102-综合搜索-102005 */
        public static readonly string DYS_SEARCH = "102005";
        /** 内容入口 103-搜索视频-小程序锚点入口-103001 */
        public static readonly string DYS_SEARCH_VIDEO = "103001";
        /** 内容入口 103-抖极评论页锚点-103002 */
        public static readonly string DYS_COMMENT = "103002";
        /** 内容入口 103-同城顶部推荐入口-103021 */
        public static readonly string DYS_TOP = "103021";
        /** 内容入口 103-百科六分屏-103025 */
        public static readonly string DYS_SIX = "103025";
        /** 分享 104-微信分享-104001 */
        public static readonly string DYS_SHARE = "104001";
        /** 分享 104-分享回流-104007 */
        public static readonly string DYS_SHAREBACK = "104007";
        /** 广告 105-激励广告-105001 */
        public static readonly string DYS_AD = "105001";
        /** 运营位置 108-订单综合入口-108013 */
        public static readonly string DYS_ORDER = "108013";
        /** 运营位置 108-发布页锚点入口-108014 */
        public static readonly string DYS_PUBLISH = "108014";

        /* ******************************** *
         * ********番茄小说(FQ_XS)场景值********
         * ******************************** */
        /** 固定入口 181-我的页面-常用功能-小游戏 list-181003 */
        public static readonly string FQ_XS_GAME = "181003";
        /** 固定入口 181-小程序跳小程序-181009 */
        public static readonly string FQ_XS_JUMP = "181009";
        /** 固定入口 181-小程序返回小程序-181010 */
        public static readonly string FQ_XS_BACK = "181010";
        /** 固定入口 181-金币游戏中心-181011 */
        public static readonly string FQ_XS_GAME_CENTER = "181011";
        /** 固定入口 181-桌面图标-181020 */
        public static readonly string FQ_XS_DESKTOP = "181020";

        /* ******************************** *
         * ********番茄畅听(FQ_CT)场景值********
         * ******************************** */
        /** 固定入口 141-小程序跳小程序-141009 */
        public static readonly string FQ_CT_JUMP = "141009";
        /** 固定入口 141-小程序返回小程序-141010 */
        public static readonly string FQ_CT_BACK = "141010";
        /** 固定入口 141-金币游戏中心-141011 */
        public static readonly string FQ_CT_GAME_CENTER = "141011";
        /** 固定入口 141-金币游戏中心新版-141012 */
        public static readonly string FQ_CT_GAME_CENTER_NEW = "141012";
        /** 固定入口 141-桌面图标-141020 */
        public static readonly string FQ_CT_DESKTOP = "141020";
        /** 分享 142-口令分享-142001 */
        public static readonly string FQ_CT_SHARE = "142001";
    }

}
#endif
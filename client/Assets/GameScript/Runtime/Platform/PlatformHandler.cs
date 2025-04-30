using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameScript.Runtime.GameLogic;
using MyUI;
using Sdk.Runtime.Base;
using UIFramework;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;

namespace GameScript.Runtime.Platform
{
    public  class PlatformHandler : ComponentSingleton<PlatformHandler>,IInitializable
    {
        private IPlatformAPI platform;

        public IPlatformAPI Platform => platform;

        //平台获取的窗口信息，为了适配授权按钮
        private GameWindowInfo _windowInfo;
        public  GameWindowInfo WindowInfo => _windowInfo;
        
  
        
        //是否弹出过隐私授权，如果弹出过且已经授权过，如果没有头像授权过，则打开头像授权
        public bool HasPopupPrivacySetting { get; private set; } = false;
        
        //是否授权过隐私授权
        public bool HasAuthPrivacySetting { get; private set; } = false;
        
        private  const int UnKnowErrorCode = -100;
        
        /// <summary>
        /// 是否开启震动，保存在本地
        /// </summary>
        private bool _isOpenShake;

        public bool IsOpenShake => _isOpenShake;

        /// <summary>
        /// 用户保存的一些本地数据
        /// </summary>
        private GameSettingsData _gameSettings;

        public static GameSettingsData GameSettingData
        {
            get
            {
                if (Instance._gameSettings == null)
                    Instance._gameSettings = GameSettingLocalStorage.LoadSettings();
                return Instance._gameSettings;
            }
        }
        
        
        /// <summary>
        /// 是否需要重新登陆
        /// </summary>
        public bool NeedReLogin { get; private set; }
        
        public void SetShakeOpen(bool isOpen)
        {
            _isOpenShake = isOpen;
        }

        //设置已经向用户请求过平台权限信息
        public void SetHasRequestPlatformUserInfo()
        {
            GameSettingData.IsRequestAuthorPlatformUserInfo = true;
            GameSettingData.Save();
        }

        void UseFuncError(PlatformFunc platformFunc, string error, bool showToast = false)
        {
            if (string.IsNullOrEmpty(error))
                error = $"can't support {platformFunc.ToString()}";
            if (showToast)
                UIManager.OpenScreen<ToastScreenPresenter, string>(error);
        }

        public void InitPlatform()
        {
            if (platform != null)
                return;

            platform = new DefaultPlatformAPI(UseFuncError);
#if PLATFORM_WEIXIN && !UNITY_EDITOR
            platform = new WeChatPlatformAPI(UseFuncError);
         
#elif PLATFORM_DOUYIN && !UNITY_EDITOR
            platform = new ByteGamePlatformAPI(UseFuncError);
#endif
            Log($"AppVersion:{GameVersion.GameVer}");
        }

        public void InitPlatformInfo()
        {
            _windowInfo = platform.GetWindowInfo();
            InitVideoAd();
        }

        public UniTask<LoginResult> Login()
        {
            return platform.Login();
        }

        public bool IsSupport(PlatformFunc func)
        {
            return platform.IsSupport(func);
        }

        public void StartGyroscope(double interval, Action<bool, string> statusCallback, OnGameGyroscopeChanged gyroscopeChanged)
        {
            platform.StartGyroscope(interval, statusCallback, gyroscopeChanged);
        }

        public void StopAccelerometer(Action<bool, string> statusCallback)
        {
            platform.StopGyroscope(statusCallback);
        }

        public void Vibrate(long[] pattern, int repeat = -1)
        {
            if (!IsOpenShake)
                return;
            platform.Vibrate(pattern, repeat);
        }

        private readonly long[] shortVibrate = new long[500];

        public void VibrateShort(int repeat = -1)
        {
            if (!IsOpenShake)
                return;
            platform.Vibrate(shortVibrate, repeat);
        }

        public void NotchFitter(RectTransform rectTransform)
        {
            platform.NotchFitter(rectTransform);
        }

        public void SliderBarNavigation(GameSliderSceneEnum sceneEnum, Action successCallback = null, Action completeCallback = null, Action<int, string> errorCallback = null)
        {
            platform.SliderBarNavigation(sceneEnum, successCallback, completeCallback, errorCallback);
        }

        public void StartVideoRecord(bool isRecordAudio = true, int maxRecordTimeSec = 300, Action startCallback = null, Action<int, string> errorCallback = null,
                                     Action<string> timeoutCallback = null)
        {
            platform.StartRecord(isRecordAudio, maxRecordTimeSec, startCallback, errorCallback, timeoutCallback);
        }

        public void StopVideoRecord(Action<string> completeCallback = null, Action<int, string> errorCallback = null)
        {
            platform.StopRecord(completeCallback, errorCallback);
        }

        public void ShareVideoRecord(Action<Dictionary<string, object>> successCallback = null, Action<string> errorCallback = null, Action cancelCallback = null)
        {
            platform.ShareGameRecord(successCallback, errorCallback, cancelCallback);
        }

        public GameVideoRecordState GetVideoRecordState()
        {
            return platform.GetVideoRecordState();
        }

        public void SetRankScore(int score)
        {
            platform.SetRankScore(score);
        }
        
        public double GetLaunchSceneCode()
        {
            return platform.GetLaunchSceneCode();
        }

        public void GetRankList()
        {
            platform.GetRankList((b, s) => { Debug.Log("========>>>>Rank List " + b + " : " + s); }, "all", "分");
        }

        /// <summary>
        /// 返回平台名字，默认全小写"ios"、"android"...
        /// </summary>
        /// <returns></returns>
        public string GetPlatform()
        {
            return platform.GetDevicePlatform();
        }

        public void InitVideoAd()
        {
            try
            {
                platform.InitVideoAd();
            }
            catch (Exception )
            {
                TdReport.Report(Reportkey.AdvStatus, new Dictionary<string, object>() { {"adv_from",1},{ "adv_progress", 3 } });
            }
        }

        private int playVideoTryTimes = 0; 
        /// <summary>
        /// 清除广告尝试次数，在可转分享的广告在部分情况播放失败后，且满足失败次数的条件下调起分享来代替广告，需要在进入广告场景时调用
        /// </summary>
        public void ClearPlayVideoTryTimes()
        {
            playVideoTryTimes = 0;
        }

        private bool isPlayVideoAd = false;

        /// <summary>
        /// 播放视频广告
        /// </summary>
        /// <param name="callback">回调</param>
        /// <param name="cancellationToken"></param>
        /// <param name="enableShare">支持转分享</param>
        public async void PlayVideoAd(Action<AdState> callback,CancellationToken cancellationToken, bool enableShare = false)
        {
            if (GameInstance.Instance.GetUType() == 3)
            {
                TdReport.Report(Reportkey.AdvStatus, new Dictionary<string, object>() { {"adv_from",1},{ "adv_progress", 1 } });
                callback?.Invoke(AdState.Success);
                return;
            }
            try
            {
                if (isPlayVideoAd)
                {
                    ToastScreenPresenter.Show($"广告尝试获取中，请稍后");
                    return;
                }
                isPlayVideoAd = true;
               var (adState,errorMsg) = await platform.PlayVideoAd(cancellationToken);
               isPlayVideoAd = false;
               if (!string.IsNullOrEmpty(errorMsg))
               {
                   ToastScreenPresenter.ShowError(errorMsg);
               }
               if (!cancellationToken.IsCancellationRequested)
               {
                   TdReport.Report(Reportkey.AdvStatus, new Dictionary<string, object>() { {"adv_from",1},{ "adv_progress", 1 } });
                   if (adState == AdState.Success || adState == AdState.Exit)
                   {
                       callback?.Invoke(adState);
                   }else if (adState == AdState.AdToShare || adState == AdState.AdThrowError)
                   {
                       if (enableShare)
                       {
                           platform.Share("", "", (result) =>
                           {
                               callback?.Invoke(result ? AdState.Success : AdState.Error);
                           });
                       }
                   }
                   else 
                   {
                       callback?.Invoke(adState);
                   }
                  
               }
               else
               {
                   TdReport.Report(Reportkey.AdvStatus, new Dictionary<string, object>() { {"adv_from",1},{ "adv_progress", 2 } });
               }
               
            }
            catch (Exception e)
            {
                Debug.Log("PlayVideoAd " + e.Message);
                isPlayVideoAd = false;
                callback?.Invoke(AdState.AdThrowError);
            }
        }

        public async UniTask<(bool isAuthor,bool isError)> IsAuthorPlatformUserInfo()
        {
            UniTaskCompletionSource<(bool, bool)> task = new UniTaskCompletionSource<(bool, bool)>();
            platform.IsAuthor((bool isAuthor) =>
            {
                task.TrySetResult((isAuthor, false));
            }, (error) =>
            {
                task.TrySetResult((false, true));
               
            });
            return await task.Task;
        }

        /// <summary>
        /// 获取平台用户数据
        /// </summary>
        public void GetPlatformUserInfo(Action<PlatformUserInfo,GetPlatformUserInfoState,string> userInfoCallback)
        {
            platform.GetPlatformUserInfo((info, state, error) => { userInfoCallback?.Invoke(info, state, error); });
           
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
        /// <param name="tabCallback">点击回调</param>
        public void CreateUserInfoBtn(int x,int y,int width,int height,string language,bool withCredentials,Action<PlatformUserInfo,GetPlatformUserInfoState,string> tabCallback)
        {
            platform.CreateUserInfoBtn(x,y,width,height, language,withCredentials,(info,state,error) =>
            {
                tabCallback?.Invoke(info,state,error);
            } );
        }

        /// <summary>
        /// 设置设备帧率
        /// </summary>
        /// <param name="frame"></param>
        public void SetTargetFrame(int frame)
        {
            platform.SetTargetFrame(frame);
        }
        
        /// <summary>
        /// 是否已经授权隐私授权，微信需要
        /// </summary>
        /// <returns></returns>
        public void GetPrivacyAuthorize(Action<bool> callback)
        {
            platform.GetPrivacyAuthorize(callback);
        }

        public  void OpenPrivacyAuthorize(Action<bool> callback)
        {
            platform.OpenPrivacyAuthorize((agree) =>
            {
                HasPopupPrivacySetting = true;
                HasAuthPrivacySetting = agree;
                GameSettingData.HasRequestPrivacySetting = true;
                GameSettingData.Save();
                callback?.Invoke(agree);
            });
        }

        public void GetPreloadPath(ref List<string> pathList)
        {
            
        }

        void InitLocalData()
        {
            _gameSettings = GameSettingLocalStorage.LoadSettings();

            //震动开关是否打开了
            _isOpenShake = _gameSettings.ShakeOpenOn != GameConstant.SaveInvalidValue;
            //如果本地token是空的，需要登陆
            var token = _gameSettings.LoginToken;
            var expiration =_gameSettings.LoginTokenExpiration;
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(expiration))
            {
                _gameSettings.HasRequestPrivacySetting = false;
                NeedReLogin = true;
            }
            else
            {
                //如果缓存的token，离过期不足2天，需要重新登陆
                var expirationDateTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(expiration)).ToLocalTime();
                TimeSpan interval = expirationDateTime - DateTimeOffset.Now;
                if (interval <= TimeSpan.FromDays(2))
                {
                    _gameSettings.HasRequestPrivacySetting = false;
                    NeedReLogin = true;
                }
                else
                {
                    NeedReLogin = false;
                    NetManager.token = token;
                }
                
            }
        }

        public UniTask InitializeAsync()
        {
            //在游戏初始化前就必须初始化平台信息，因为抖音平台native 下必须用平台接口获取版本号
            // //初始化平台api
            InitPlatform();
            //初始化保存的数据
            InitLocalData();
        
            GetPrivacyAuthorize((needOpenAuthorize) =>
            {
                //需要打开
                if (needOpenAuthorize)
                {
                    if (!_gameSettings.HasRequestPrivacySetting)
                    {
                        OpenPrivacyAuthorize(null);
                    }
                    else
                    {
                        HasPopupPrivacySetting = true;
                        HasAuthPrivacySetting = false;
                    }
                   
                }
                else
                {
                    HasPopupPrivacySetting = true;
                    HasAuthPrivacySetting = true;
                }
            });
            return UniTask.FromResult(true);
        }

        public void Register()
        {
            DontDestroyOnLoad(gameObject);
        }
        /// <summary>
        /// 检查session 是否有效，用于判断登陆
        /// </summary>
        /// <returns></returns>
        public UniTask<bool> CheckSession()
        {
            return platform.CheckSession();
        }
        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="content">日志内容</param>
        /// <param name="logType">日志类型</param>
        public void Log(string content, LogType logType = LogType.Info)
        {
            platform.Log(content, logType);
        }

    }
}
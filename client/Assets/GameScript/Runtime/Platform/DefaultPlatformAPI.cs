using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameScript.Runtime.GameLogic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameScript.Runtime.Platform
{
    public class DefaultPlatformAPI : IPlatformAPI
    {
        private Action<PlatformFunc, string, bool> _useApiErrorCallback;

        public DefaultPlatformAPI(Action<PlatformFunc, string, bool> useApiErrorCallback)
        {
            _useApiErrorCallback = useApiErrorCallback;
        }

        public bool IsSupport(PlatformFunc func)
        {
            switch (func)
            {
                case PlatformFunc.Sharable:
                    return false;
            }
            return false;
        }

        public UniTask<LoginResult> Login()
        {
            var loginResult = new LoginResult
                              {
                                  code = Random.Range(10000, 99999).ToString()
                              };
            return UniTask.FromResult(loginResult);
        }

        public void Share(string title, string imageUrl, Action<bool> callback = null)
        {
            callback?.Invoke(true);
        }
        
        public void ShareMessage(ShareMessageType shareMessage, Action<bool> callback = null)
        {
            callback?.Invoke(true);
        }
        
        public void SetClipboardData(string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }

        public void StartGyroscope(double interval, Action<bool, string> statusCallback, OnGameGyroscopeChanged gyroscopeChanged)
        {
        }

        public void StopGyroscope(Action<bool, string> statusCallback)
        {
        }

        public void Vibrate(long[] pattern, int repeat)
        {
        }

        public void NotchFitter(RectTransform fitterRectTransform)
        {
        }

        public void SliderBarNavigation(GameSliderSceneEnum sceneEnum, Action successCallback, Action completeCallback, Action<int, string> errorCallback)
        {
            errorCallback?.Invoke(-1, "not support");
        }

        public void StartRecord(bool isRecordAudio = true, int maxRecordTimeSec = 600, Action startCallback = null, Action<int, string> errorCallback = null, Action<string> timeoutCallback = null)
        {
            errorCallback?.Invoke(-1, "not support");
        }

        public void StopRecord(Action<string> completeCallback, Action<int, string> errorCallback)
        {
            errorCallback?.Invoke(-1, "not support");
        }

        public void ShareGameRecord(Action<Dictionary<string, object>> successCallback, Action<string> errorCallback, Action cancelCallback)
        {
        }

        public GameVideoRecordState GetVideoRecordState()
        {
            return GameVideoRecordState.RECORD_STOPED;
        }

        public void SetRankScore(int score)
        {
            Debug.Log("set rank score:" + score);
        }

        public void GetRankList(Action<bool, string> action, String type, String suffix)
        {
            Debug.Log("get rank list!");
            action?.Invoke(false, "平台暂未实现排行榜");
        }

        public string GetDevicePlatform()
        {
            return Application.platform.ToString().ToLower();
        }

        public void InitVideoAd()
        {
            Debug.Log("初始化视频广告!");
        }

        public async UniTask<(AdState,string)> PlayVideoAd(CancellationToken cancellationToken)
        {
            Debug.Log("模拟广告成功!");
            return (AdState.Success, string.Empty);
        }

        /// <summary>
        /// 获取平台用户信息
        /// </summary>
        /// <returns></returns>
        public void GetPlatformUserInfo(Action<PlatformUserInfo,GetPlatformUserInfoState,string> userInfoCallback)
        {
            string uid = GameInstance.Instance.GetUid();
            userInfoCallback?.Invoke(new PlatformUserInfo()
                                     {
                                         NickName = GetLastSixChars(uid),
                                         Avatar = "",
                                         // Avatar = $"https://cat-match-static.easygame2021.com/head/{(int)(Random.Range(0, 20) + 1)}.jpg",
                                     },GetPlatformUserInfoState.Agree,string.Empty);
        }

        static string GetLastSixChars(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            if (s.Length <= 6)
            {
                return s; // 如果字符串长度不足6个字符，返回整个字符串
            }
            else
            {
                return s.Substring(s.Length - 6); // 截取最后6个字符
            }
        }
        public void IsAuthor(Action<bool> getResultCallback, Action<string> failCallback)
        {
            getResultCallback?.Invoke(true);
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
        public void CreateUserInfoBtn(int x, int y, int width, int height, string language, bool withCredentials, Action<PlatformUserInfo, GetPlatformUserInfoState, string> tabCallback)
        {
            string uid = GameInstance.Instance.GetUid();
            tabCallback?.Invoke(new PlatformUserInfo()
                                {
                                    NickName = GetLastSixChars(uid),
                                    Avatar = "",
                                    // Avatar = $"https://cat-match-static.easygame2021.com/head/{(int)(Random.Range(0, 20) + 1)}.jpg",
                                },GetPlatformUserInfoState.Agree,string.Empty);
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
            return false;
        }
        
        public void AddShortcut()
        {
            Debug.Log("添加桌面快捷方式");
        }

        public Dictionary<string, string> GetLaunchQuery()
        {
            return new Dictionary<string, string>();
        }
        
        public string GetLaunchString()
        {
            return "";
        }
        
        public bool LaunchFromScene(FromSceneType type)
        {
            switch (type)
            {
                case FromSceneType.All:
                    return true;
                case FromSceneType.AdScene:
                    return false;
            }
            return false;
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
        /// </summary>
        /// <returns></returns>
        public UniTask<bool> CheckSession()
        {
            return  new UniTask<bool>(true);
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
    }
}
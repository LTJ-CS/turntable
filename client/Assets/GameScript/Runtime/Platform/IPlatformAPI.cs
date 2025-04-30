using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameScript.Runtime.Platform
{
    public delegate void OnGameGyroscopeChanged(
        double x,
        double y,
        double z,
        double t,
        double roll,
        double pitch,
        double yaw,
        double result);

    public enum PlatformFunc
    {
        Vibrate,                  //震动
        Gyroscope,                //陀螺仪
        NotchFitter,              //顶部适配
        SliderBarNavigateToScene, //侧边栏导航到scene
        GetGameRecorder,          //获取录屏对象
        StartRecord,              //开始录屏
        StopRecord,               //停止录屏
        GetGameRecorderState,     //获取录屏状态
        Sharable,                 //分享
        AddShortcut,              //添加快捷方式（桌面）
    }

    public enum FromSceneType
    {
        All,                      //默认场景
        AdScene,                  //广告场景
        Public,                   //公众号
    }

    /// <summary>录屏状态枚举</summary>
    public enum GameVideoRecordState
    {
        /// <summary>录制开始中</summary>
        RECORD_STARTING,

        /// <summary>录制已开始</summary>
        RECORD_STARTED,

        /// <summary>录制暂停中</summary>
        RECORD_PAUSING,

        /// <summary>录制已暂停</summary>
        RECORD_PAUSED,

        /// <summary>录制停止中</summary>
        RECORD_STOPING,

        /// <summary>录制已停止</summary>
        RECORD_STOPED,

        /// <summary>录制结束</summary>
        RECORD_COMPLETED,

        /// <summary>录制错误</summary>
        RECORD_ERROR,

        /// <summary>录制的视频时长太短</summary>
        RECORD_VIDEO_TOO_SHORT,
    }

    /// <summary>视频分享状态枚举</summary>
    public enum GameVideoShareState
    {
        /// <summary>未进行视频分享</summary>
        NONE,

        /// <summary>视频分享中</summary>
        SHARING,

        /// <summary>等待分享器回调</summary>
        WAITING_CALLBACK,

        /// <summary>视频分享成功</summary>
        SHARE_SUCCESS,

        /// <summary>视频分享失败</summary>
        SHARE_FAILED,

        /// <summary>视频分享取消</summary>
        SHARE_CANCELLED,
    }

    public enum GameSliderSceneEnum
    {
        SideBar,
    }

    public class LoginResult
    {
        public string code;
        public int    errCode;
        public string errMsg;
    }

    public class LaunchOptions
    {
        public int scene; // 客户端启动时的场景值
        // public Map query; // 客户端启动参数
    }

    public enum AdState
    {
        /// <summary>
        /// 广告拉取失败
        /// </summary>
        Error,

        /// <summary>
        /// 广告播放成功
        /// </summary>
        Success,

        /// <summary>
        /// 广告播放中，关闭视频
        /// </summary>
        Exit,

        /// <summary>
        /// 无广告实例, 版本不支持
        /// </summary>
        NoAd,

        /// <summary>
        /// 广告重试，拉取失败
        /// </summary>
        RepeatFail,

        /// <summary>
        /// 无广告填充，走分享逻辑
        /// </summary>
        AdToShare,

        /// <summary>
        /// 广告代码，抛出异常
        /// </summary>
        AdThrowError,

        /// <summary>
        /// 广告尝试加载中
        /// </summary>
        AdTryShowing,

        /// <summary>
        /// 广告播放成功
        /// </summary>
        AdShowSuccess,

        /// <summary>
        /// 取消播放
        /// </summary>
        AdShowCancel,

        /// <summary>
        /// 没有网络播放失败
        /// </summary>
        AdNoNetShowFail,

        /// <summary>
        /// 网络组件坏掉了
        /// </summary>
        AdComponentBroken,

        /// <summary>
        /// 白名单用户
        /// </summary>
        WhiteList,
    }

    /// <summary>
    /// 平台用户信息
    /// </summary>
    public struct PlatformUserInfo
    {
        public string NickName;
        public string Avatar;
    }

    //获取玩家信息返回的状态
    public enum GetPlatformUserInfoState
    {
        Error,  //返回错误
        Agree,  //同意
        Refuse, //拒绝
    }

    public class GameWindowInfo
    {
        /// <summary>设备像素比</summary>
        public double pixelRatio;

        /// <summary>屏幕高度，单位px</summary>
        public double screenHeight;

        /// <summary>窗口上边缘的y值</summary>
        public double screenTop;

        /// <summary>屏幕宽度，单位px</summary>
        public double screenWidth;
    }

    public enum ShareMessageMode
    {
        Default = 0,
        Strict = 1,
    }
    
    /// <summary>
    /// 分享消息类型
    /// </summary>
    public class ShareMessageType
    {
        public string title;
        public string imageUrl;
        /// <summary>
        /// 查询字符串，从这条转发消息进入后，可通过 wx.getLaunchOptionsSync() 或 wx.onShow() 获取启动参数中的
        /// query。必须是 key1=val1&key2=val2 的格式。
        /// </summary>
        public string query;
        public string imageUrlId;
        public string toCurrentGroup;
        public string path;
        public string shareCode;
        public string itemName;
        public ShareMessageMode mode;

        public ShareMessageType(string mTitle, string mImageUrl, string mQuery)
        {
            title = mTitle;
            imageUrl = mImageUrl;
            query = mQuery;
        }
    }

    /// <summary>
    /// 平台调试方法日志类型
    /// </summary>
    public enum LogType
    {
        Debug,
        Log,
        Info,
        Warn,
        Error,
    }
    public interface IPlatformAPI
    {
        // 是否支持某个API接口
        public bool IsSupport(PlatformFunc func);

        /// <summary>
        /// 平台登录，获取code，用于后台换取openid
        /// </summary>
        public UniTask<LoginResult> Login();

        /// <summary>
        /// 分享
        /// </summary>
        public void Share(string title = "", string imageUrl = "", Action<bool> callback = null);

        /// <summary>
        /// 分享
        /// </summary>
        public void ShareMessage(ShareMessageType shareMessage, Action<bool> callback = null);

        /// 设置剪切板内容
        public void SetClipboardData(string text);

        /// <summary>
        /// 开启陀螺仪
        /// </summary>
        public void StartGyroscope(double interval, Action<bool, string> statusCallback, OnGameGyroscopeChanged gyroscopeChanged);

        /// <summary>
        /// 关闭陀螺仪
        /// </summary>
        public void StopGyroscope(Action<bool, string> statusCallback);

        /// <summary>
        /// 开震动
        /// </summary>
        /// <param name="pattern">震动周期 like long[] pattern = {0, 100, 1000, 300}; 传入null则取消震动</param>
        /// <param name="repeat">重复次数，为-1则不重复</param>
        public void Vibrate(long[] pattern, int repeat);

        /// <summary>
        /// 顶部适配
        /// </summary>
        /// <param name="fitterRectTransform"></param>
        public void NotchFitter(RectTransform fitterRectTransform);

        /// <summary>
        /// 侧边栏导航
        /// </summary>
        public void SliderBarNavigation(GameSliderSceneEnum sceneEnum, Action successCallback, Action completeCallback, Action<int, string> errorCallback);

        /// <summary>
        /// 开始录屏
        /// </summary>
        public void StartRecord(bool isRecordAudio = true, int maxRecordTimeSec = 600, Action startCallback = null, Action<int, string> errorCallback = null, Action<string> timeoutCallback = null);

        /// <summary>
        /// 停止录屏
        /// </summary>
        public void StopRecord(Action<string> completeCallback, Action<int, string> errorCallback);

        /// <summary>
        /// 分享录屏
        /// </summary>
        public void ShareGameRecord(Action<Dictionary<string, object>> successCallback, Action<string> errorCallback, Action cancelCallback);

        /// <summary>
        /// 录屏状态
        /// </summary>
        /// <returns></returns>
        public GameVideoRecordState GetVideoRecordState();

        /// <summary>
        /// 设置分数，用于平台排行榜
        /// </summary>
        /// <param name="score"></param>
        public void SetRankScore(int score);

        /// <summary>
        /// 调起平台的排行榜
        /// </summary>
        /// <param name="type"></param>
        /// <param name="suffix"></param>
        /// <param name="action"></param>
        public void GetRankList(Action<bool, string> action, String type, String suffix);

        /// <summary>
        /// 获取设备平台类型 ios android ,返回值默认全小写
        /// </summary>
        /// <returns></returns>
        public string GetDevicePlatform();

        /// 初始化视频广告
        /// </summary>
        public void InitVideoAd();

        /// <summary>
        /// 播放视频广告
        /// </summary>
        /// <param name="cancellationToken"></param>
        public UniTask<(AdState, string)> PlayVideoAd(CancellationToken cancellationToken);

        /// <summary>
        /// 获取平台用户信息
        /// </summary>
        /// <returns></returns>
        public void GetPlatformUserInfo(Action<PlatformUserInfo, GetPlatformUserInfoState, string> userInfoCallback);

        /// <summary>
        /// 是否平台授权过
        /// </summary>
        /// <param name="getResultCallback">是否授权结果</param>
        /// <param name="failCallback">错误回调</param>
        public void IsAuthor(Action<bool> getResultCallback, Action<string> failCallback);

        /// <summary>
        /// 创建授权透明按钮
        /// </summary>
        /// <param name="x">按钮位置</param>
        /// <param name="y">按钮位置</param>
        /// <param name="width">按钮宽度</param>
        /// <param name="height">按钮高度</param>
        /// <param name="language">语言</param>
        /// <param name="withCredentials">是否带有登陆信息</param>
        /// <param name="tabCallback">点击回调，用户数据，返回状态，错误信息</param>
        public void CreateUserInfoBtn(int x, int y, int width, int height, string language, bool withCredentials, Action<PlatformUserInfo, GetPlatformUserInfoState, string> tabCallback);

        /// <summary>
        /// 获取屏幕信息
        /// </summary>
        /// <returns></returns>
        public GameWindowInfo GetWindowInfo();

        /// <summary>
        /// 设置设备帧率
        /// </summary>
        /// <param name="frame"></param>
        public void SetTargetFrame(int frame);

        /// <summary>
        /// 是否需要打开隐私授权界面，微信需要
        /// </summary>
        /// <returns></returns>
        public void GetPrivacyAuthorize(Action<bool> callback);

        /// <summary>
        /// 打开隐私授权界面，微信需要
        /// </summary>
        /// <param name="callback">是否同意授权</param>
        public void OpenPrivacyAuthorize(Action<bool> callback);

        /// <summary>
        /// (桌面)快捷方式是否存在
        /// </summary>
        /// <returns></returns>
        public bool IsShortcutExist();

        /// <summary>
        /// 添加(桌面)快捷方式
        /// </summary>
        public void AddShortcut();

        /// <summary>
        /// 获取登录参数
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetLaunchQuery();

        /// <summary>
        /// 获取登录参数
        /// </summary>
        /// <returns></returns>
        public string GetLaunchString();
        
        /// <summary>
        /// 是否从指定类型的场景进入
        /// </summary>
        /// <returns></returns>
        public bool LaunchFromScene(FromSceneType type);

        /// <summary>
        /// 获取游戏版本号
        /// </summary>
        /// <returns></returns>
        public string GetGameVersion();

        /// <summary>
        /// 检查session 是否有效，用于判断登陆
        /// </summary>
        /// <returns></returns>
        public UniTask<bool> CheckSession();
        
        /// <summary>
        /// 获取启动场景值参数
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public double GetLaunchSceneCode();

        /// <summary>
        /// 是否从微信公众号跳转打开游戏
        /// </summary>
        /// <returns></returns>
        public bool IsLaunchSceneFromWxPublicAccount();
        
        
        #region 调试 输出日志，方法对应了平台的方法

        /// <summary>
        /// 平台输出debug日志
        /// </summary>
        public void Log(string logContent, LogType logType);


        #endregion

    }
}
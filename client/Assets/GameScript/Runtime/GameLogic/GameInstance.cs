using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AccountProto;
using ClientCfg;
using ClientProto;
using Cysharp.Threading.Tasks;
using GameScript.Runtime.GameLogic.ServerData;
using GameScript.Runtime.Platform;
using GameScript.Runtime.UI;
using Google.Protobuf.Collections;
using LitJson;
using MyUI;
using Protocol;
using Sdk.Runtime.Base;
using Sdk.Runtime.Utility;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.Serialization;
using UserInfo = GameScript.Runtime.GameLogic.ServerData.UserInfo;

namespace GameScript.Runtime.GameLogic
{
    /// <summary>
    /// 游戏的主实例, 保存与服务器的数据同步及在游戏整个生命周期中一直存在的数据, 在游戏整个生命周期中一直存在!
    /// </summary>
    /// <remarks>
    /// 只应该在这里实现一些与服务器同步的常驻数据, 如玩家的名称, 经验, 等级, 金币等一些全局的数据, 方便其它逻辑直接访问  
    /// </remarks>
    public partial class GameInstance : ComponentSingleton<GameInstance>, IInitializable
    {
        /// <summary>
        /// 玩家状态, 与服务器保持同步的数据
        /// </summary>
        private PlayerState _playerState;


        private TimeUpdateControl _timeControl;


        /// <summary>
        /// 开始游戏时请求后台获得该局游戏的token
        /// </summary>
        public string gameToken;

        /// <summary>
        /// 等待后进入场景标志
        /// </summary>
        public bool m_WaitScene = false;

        /// <summary>
        /// 过渡结束
        /// </summary>
        public bool m_TransEnd = false;

        /// <summary>
        /// 临时demo用关卡数据
        /// </summary>
        public GameScoreData m_GameScoreData;

        /// <summary>
        /// 当前关卡
        /// </summary>
        public int m_NowJourney;

        /// <summary>
        /// 打点用，第一次关卡
        /// </summary>
        public bool m_FirstEnter;

        /// <summary>
        /// 是否在编辑模式
        /// </summary>
        public bool m_InEditMode = false;

        //登陆后进入的第一个关卡
        public string EnterGameSceneName { get; private set; }

        /// <summary>
        /// 本次登录已点击过添加(桌面)快捷方式的按钮
        /// </summary>
        private bool _hasClickAddShortcut = false;

        public bool HasClickAddShortcut => _hasClickAddShortcut;


        /// <summary>
        /// 在onShow判断进入场景值为公众号跳转时，设置状态机，强制进入微信公众号奖励状态
        /// </summary>
        private bool forceEnterWxPublicAccountRewardStatus = false;

        /// <summary>
        /// 广告点击回调ID
        /// </summary>
        public string clickid = "";
        
        /// <summary>
        /// 已点击指引列表
        /// </summary>
        public List<int> m_GuideList;

        public string[] m_Tags=new string[2];
        public Dictionary<EDressType, int> SelectCollection=new()
                                                            {
                                                                {EDressType.Body,2},
                                                                {EDressType.Clothes,99},
                                                                {EDressType.Pants,99},
                                                                {EDressType.Shoes,99},
                                                                {EDressType.Noses,99},
                                                                {EDressType.Eyes,99},
                                                                {EDressType.Mouth,99},
                                                                {EDressType.Hair,99},
                                                                {EDressType.Face,1},
                                                                {EDressType.Hat,0},
                                                                {EDressType.Ears,0},
                                                                {EDressType.Brow,0},
                                                                {EDressType.Navel,0},
                                                                {EDressType.Lip,0},
                                                                {EDressType.NosesSub,0},
                                                                {EDressType.EyesSub,0},
                                                            };

        public Color ClothesColor = Color.white;
        public Color PantsColor = Color.white;
        
        public List<int> UnlockChapters    => _playerState.LevelData.ChapterInfos;
        public bool      IsTestAccount     => _playerState != null && _playerState.UserInfo.TestAccount;

        private void Start()
        {
#if !UNITY_EDITOR
            // 屏蔽掉 PlayMaker 的日志输出, 减少gc及性能消耗
            HutongGames.PlayMaker.FsmLog.LoggingEnabled = false;
            HutongGames.PlayMaker.FsmLog.MirrorDebugLog = false;
            HutongGames.PlayMaker.FsmLog.EnableDebugFlow = false;
#endif

            CommonUIEvents.AfterReLogin += OnAfterReLogin;
        }

        /// <summary>
        /// 供 FirstScreenComponent 调用, 用于添加登录动作, UI 初始化等动作
        /// </summary>
        /// <param name="actions">用于添加登录动作的列表</param>
        public void RegisterLoginAction(List<Func<UniTask>> actions)
        {
            DontDestroyOnLoad(gameObject);
            actions.Add(Login);
        }

        /// <summary>
        /// 登录到服务器
        /// </summary>
        /// <returns> true 表示登录成功, false 表示登录失败</returns>
        private async UniTask Login()
        {
            // 创建玩家状态
            _playerState = new PlayerState();
            //设置时间管理器
            _timeControl = new TimeUpdateControl();
            _timeControl.Init();

            //初始化平台信息
            PlatformHandler.Instance.InitPlatformInfo();
            //检查token 是否存在或过期
            bool tokenNeedReLogin = PlatformHandler.Instance.NeedReLogin;
            //检查平台session 是否有效，无效需要重新登陆
            var checkSession = await PlatformHandler.Instance.CheckSession();
            //如果token 过期，或者平台session无效，则需要重新登录
            bool isNeedReLogin = !checkSession || tokenNeedReLogin;
            LoginResult platformLoginResult = null;
            //平台登陆
            if (isNeedReLogin)
            {
                platformLoginResult = await PlatformHandler.Instance.Login();
            }

            //获取用户授权信息
            (bool isAuthor, bool isError) = await PlatformHandler.Instance.IsAuthorPlatformUserInfo(); //获取用户授权信息
            IsNeedOpenAuthorizedPlatformUserInfo = !isAuthor;                                          //如果平台未授权状态,则需要在授权场景里等待授权
            bool isNeedSyncUserInfo = true;
            //是否需要向服务器同步用户信息
            // if (isNeedReLogin)
            // {
            //     await ReqLogin(platformLoginResult.code);
            // }
            // else
            // {
            //     //玩家token没过期，需要判断下是否已经向玩家请求过权限了，如果请求过了，就不应该每次登陆都请求
            //     //为了话题pk活动，如果玩家token没过期，但是从来没请求过权限，应该去请求下权限，
            //     var isRequestAuthorPlatformUserInfo = PlatformHandler.GameSettingData.IsRequestAuthorPlatformUserInfo;
            //     IsNeedOpenAuthorizedPlatformUserInfo = IsNeedOpenAuthorizedPlatformUserInfo && !isRequestAuthorPlatformUserInfo;
            //     isNeedSyncUserInfo = !PlatformHandler.GameSettingData.HasSyncPlatformUserInfo;
            // }
            //
            // // await ReqUserData();
            // //如果玩家已经弹出过隐私授权且已经授权，则需要打开 授权界面
            // IsNeedOpenAuthorizedPlatformUserInfo = IsNeedOpenAuthorizedPlatformUserInfo && PlatformHandler.Instance.HasAuthPrivacySetting;
            // //如果已经授权过，且走了重新登陆，则需要同步下玩家头像信息
            // if (isAuthor && isNeedSyncUserInfo)
            // {
            //     PlatformHandler.Instance.GetPlatformUserInfo((userInfo, state, error) =>
            //     {
            //         if (state == GetPlatformUserInfoState.Agree)
            //             SePlatformUserInfo(userInfo);
            //     });
            // }
        }


        // 请求用户服务器数据
        private async UniTask<LoginLoadDataResponse> ReqUserServerData()
        {
            // var req = new C2LS_UserBaseInfo
            //           {
            //               IsAdUser = PlatformHandler.Instance.Platform.LaunchFromScene(FromSceneType.AdScene)
            //           };
            // var (isSuccess, rep) = await NetManager.PostLogicProtoAsync<C2LS_UserBaseInfo, LS2C_UserBaseInfo_Ack>(req, WaitNetTipType.LoadingAndTryConfirmWithoutClose);
            var req = new LoginLoadDataRequest()
                      {
                          Platform = (ClientProto.Platform)NetManager.GetPlatform(),
                          ClientVersion = GameVersion.GameVer,
                          SceneId = PlatformHandler.Instance.GetLaunchSceneCode().ToString(CultureInfo.InvariantCulture)
                      };
            var (isSuccess, rep) = await NetManager.PostLogicProtoAsync<LoginLoadDataRequest, LoginLoadDataResponse>(req, WaitNetTipType.LoadingAndTryConfirmWithoutClose);
            return rep;
        }

        /// <summary>
        /// 请求用户活动数据
        /// </summary>
        /// <returns></returns>
        private async UniTask<ActivityDataResponse> ReqActivityData()
        {
            var (isSuccess, rep) = await NetManager.PostLogicProtoAsync<ActivityDataRequest, ActivityDataResponse>(new ActivityDataRequest());
            return rep;
        }

        async UniTask SyncAccountInfo(PlatformUserInfo userInfo)
        {
            // C2LS_SyncAccountInfo accountInfo = new C2LS_SyncAccountInfo()
            //                                    {
            //                                        Nickname = userInfo.NickName,
            //                                        Avatar = userInfo.Avatar,
            //                                        Platform = (int)NetManager.GetPlatform(),
            //                                        OpenId = PlatformHandler.GameSettingData.OpenId,
            //                                    };
            Debug.LogError("SyncAccountInfo");
            var accountInfo = new AccountInfoSyncRequest()
                              {
                                  Name = userInfo.NickName,
                                  HeadImage = userInfo.Avatar,
                                  Platform = (ClientProto.Platform)NetManager.GetPlatform(),
                                  OpenId = PlatformHandler.GameSettingData.OpenId,
                              };
            var (isSuccess, rep) = await NetManager.PostLogicProtoAsync<AccountInfoSyncRequest, AccountInfoSyncResponse>(accountInfo);
            //如果同步成功或者失败都要保存下标记
            PlatformHandler.GameSettingData.HasSyncPlatformUserInfo = isSuccess;
            PlatformHandler.GameSettingData.Save();
        }

        /// <summary>
        /// 登录到服务器
        /// </summary>
        /// <returns> true 表示登录成功, false 表示登录失败</returns>
        public async UniTask ReqUserData()
        {
            // 获取用户数据
            var repUserModel = await ReqUserServerData();

            var actModel = await ReqActivityData();

            // 登录打点服务器
            TdReport.Instance.Login(repUserModel.PlayerBaseInfo.Id);

            m_GuideList = new List<int>();
            m_GuideList.AddRange(repUserModel.GuideList);
            // 设置用户数据
            DownloadPlayerState(repUserModel.PlayerBaseInfo, repUserModel.LevelsInfo);

            // 登录后再解析卡片参数
            CheckLaunchQuery(false);
            // 获取本地关卡得分进度
            m_GameScoreData = GameScoreLocalStorage.LoadSettings();
        }

        /// <summary>
        /// 检查启动参数
        /// </summary>
        /// <param name="isStart"></param>
        /// <param name="query"></param>
        public void CheckLaunchQuery(bool isStart, Dictionary<string, string> query = null)
        {
            try
            {
                if (query == null)
                {
                    query = PlatformHandler.Instance.Platform.GetLaunchQuery();
                }

                if (query == null)
                {
                    Debug.Log("qr null");
                }
                else
                {
                    ProcessingParameterQuery(query, isStart);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"qr err {e.Message}");
            }
        }

        /// <summary>
        /// 检查卡片数据是否有效
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private bool CheckQueryEffective(string key, string value)
        {
            var save = true;
            var str1 = PlayerPrefs.GetString($"query-{key}");
            var str2 = value;
            if (str1 != null)
            {
                if (str1.Equals(str2))
                {
                    // 两次打开的 query 完全一致 后续不处理
                    save = false;
                }
            }

            if (save)
            {
                PlayerPrefs.SetString($"query-{key}", value);
                PlayerPrefs.Save();
            }

            return save;
        }

        /// <summary>
        /// 处理卡片参数
        /// </summary>
        /// <param name="query"></param>
        /// <param name="isStart"></param>
        private void ProcessingParameterQuery(Dictionary<string, string> query, bool isStart = false)
        {
            Debug.Log($" 卡片数据 query{JsonMapper.ToJson(query)}");
            foreach (var (key, value) in query)
            {
                switch (key)
                {
                    default:
                        Debug.Log($"未识别到的参数{key}{value}");
                        break;
                }
            }
        }

        void OnAfterReLogin(LoginAck result)
        {
            PlatformHandler.GameSettingData.LoginToken = result.Token;
            PlatformHandler.GameSettingData.LoginTokenExpiration = result?.Time.ToString();
            PlatformHandler.GameSettingData.OpenId = string.Empty;
            NetManager.token = result.Token;

            PlatformHandler.GameSettingData.Save();
        }

        private async UniTask ReqLogin(string platformCode)
        {
            var result = await NetManager.Login(platformCode);
            OnAfterReLogin(result);
        }

        /// <summary>
        /// 检查来源场景
        /// </summary>
        /// <param name="type">场景类型</param>
        /// <param name="isLaunch">是否位冷启动</param>
        public void CheckFromScene(FromSceneType type, bool isLaunch)
        {
        }

        /// <summary>
        /// 下载玩家的状态的数据
        /// </summary>
        private void DownloadPlayerState(PlayerBaseInfo userModel, LevelsInfo levelsInfo)
        {
            //设置进入主场景名称
            EnterGameSceneName = "Main";
        }

        /// <summary>
        /// 获得当前时间戳（秒）
        /// </summary>
        /// <returns></returns>
        public long CurrentTimeStamp()
        {
            if (_timeControl != null)
                return _timeControl.CurrentServerTime();
            else
            {
                return TimeUtil.GetUnixTimeStamp(DateTime.Now);
            }
        }

        /// <summary>
        /// 获得当前时间戳（秒）
        /// </summary>
        /// <returns></returns>
        public long CurrentMillTimeStamp()
        {
            if (_timeControl != null)
                return _timeControl.CurrentServerMillTime();
            else
            {
                return TimeUtil.GetUnixMillTimeStamp(DateTime.Now);
            }
        }

        /// <summary>
        /// 设置或获取用户是否已经同意了授权
        /// </summary>
        public bool IsAuthorized
        {
            set
            {
                _playerState.CharacterData.SetAuthorized(value);
                PlatformHandler.GameSettingData.IsAuthorPolicy = value;
                PlatformHandler.GameSettingData.Save();
            }
            get => _playerState.CharacterData.IsAuthorized;
        }

        /// <summary>
        /// 是否应该向用户请求玩家信息权限
        /// </summary>
        public bool IsNeedOpenAuthorizedPlatformUserInfo { private set; get; }

        public string GetUid()
        {
            return _playerState.GetUid();
        }

        public int GetUType()
        {
            return _playerState.GetUType();
        }

        public string GetNickName()
        {
            var nickname = _playerState.CharacterData.NickName;
            if (string.IsNullOrEmpty(nickname))
            {
                var uid = GetUid();
                nickname = $"玩家{(uid.Length < 6 ? uid : uid.Substring(uid.Length - 6, 6))}";
            }

            return nickname;
        }

        public string GetAvatar()
        {
            return _playerState.CharacterData.Avatar ?? "";
        }

        public string GetAdCode()
        {
            return _playerState.CharacterData.AdCode ?? "0"; // "0" 是 “未知”
        }

        public void GetPreloadPath(ref List<string> pathList)
        {
        }

        public void Register()
        {
            DontDestroyOnLoad(gameObject);
        }

        public async UniTask InitializeAsync()
        {
            await Login();
        }

        /// <summary>
        /// 标记已点击过添加快捷方式按钮
        /// </summary>
        public void SignHasClickAddShortcut()
        {
            _hasClickAddShortcut = true;
        }

        private void OnDestroy()
        {
            CommonUIEvents.AfterReLogin -= OnAfterReLogin;
        }

        public static bool CheckNoticeHasShow(int noticeId)
        {
            // 获取缓存中的字符串
            var cacheStr = PlatformHandler.GameSettingData.NoticeCache;

            List<int> cacheIds;

            if (string.IsNullOrEmpty(cacheStr))
            {
                // 如果缓存为空，初始化一个新的列表
                cacheIds = new List<int>();
            }
            else
            {
                // 如果缓存不为空，从字符串中反序列化得到列表
                cacheIds = cacheStr.Split(new[]
                                          {
                                              ','
                                          }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => int.Parse(s.Trim()))
                                   .ToList();
            }

            // 检查列表中是否已经存在该 noticeId
            // 如果存在，则返回 true 表示已经显示过
            if (cacheIds.Contains(noticeId))
            {
                return true;
            }

            // 如果不存在，则添加到列表中
            cacheIds.Add(noticeId);
            // 将更新后的列表序列化为字符串并存储回缓存
            cacheStr = string.Join(",", cacheIds);
            PlatformHandler.GameSettingData.NoticeCache = cacheStr;
            PlatformHandler.GameSettingData.Save();
            // 返回 false 表示之前没有显示过
            return false;
        }

        // 功能开关数据
        private readonly HashSet<EFunctionType> _functionTypeSwitchInfo = new();

        // 获取功能开关
        public bool GetEFunctionTypeSwitch(EFunctionType type)
        {
            if (_functionTypeSwitchInfo.Contains(type))
            {
                return true;
            }

            return false;
        }
    }
}
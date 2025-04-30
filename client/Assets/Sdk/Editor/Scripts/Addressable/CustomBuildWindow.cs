using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using LitJson;
using Sdk.Runtime.Base;
using Sdk.Runtime.Platforms;
#if PLATFORM_DOUYIN
using StarkSDKTool;
#endif
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;
using YooAsset.Editor;
using BuildResult = UnityEditor.Build.Reporting.BuildResult;
using Debug = UnityEngine.Debug;

namespace Sdk.Editor.Scripts.Addressable
{
    public enum PlatformType
    {
        WeChat,
        ByteGame,
        WebGL,
        Android,
    }

    [Serializable]
    public class CustomBuildWindowSettings
    {
        public int    appIdType;
        public string appVersion;
        public string resVersion;
        public int    environment;
        public bool   isOverrideCdn;
        public string environmentCdn;

        public bool developmentBuild;
        public bool autoConnectProfiler;
        public bool profilingFuncs;
        public bool fullWithStacktrace;
    }

    [Serializable]
    public class CustomBuildOptions
    {
        public PlatformType platformType;
        public string       appId;
        public string       appVersion;
        public string       resVersion;
        public string       uId;
        public bool isNative;
        public bool waitNativeCompile;
        public string webGlPath;
        public string apkPath;
    }

    [Serializable]
    public class CustomBuildBackUpOptions
    {
        public PlatformType platformType;
        
        public string       resVersion;
    }

    /// <summary>
    /// 自定义的构建窗口
    /// </summary>
    public partial class CustomBuildWindow : EditorWindow
    {
        private enum AppIdType
        {
            WebGL = 0,
            WeChatBeta,
            WeChatProduction,
            ByteGameBeta,
            ByteGameProduction,
        }

        private static Dictionary<AppIdType, string> _appIdDict
            = new()
              {
                  { AppIdType.WebGL, "" },
                  { AppIdType.WeChatBeta, "wxb46ef27c49110563" },
                  { AppIdType.WeChatProduction, "wx62c6c3cba4dd14db" },
                  { AppIdType.ByteGameBeta, "tt87f7f14a18fd7e7707" },
                  { AppIdType.ByteGameProduction, "tt07cc717198fa75b607" },
              };

        private static AppIdType _appIdType = AppIdType.WebGL;

        private static LaunchSettings.EnvironmentType _environmentType = LaunchSettings.EnvironmentType.Testing;

        private static string _cdnUrl;

        private static bool _isCndOverride        = false;
        private static bool _isDevelopmentBuild   = false;
        private static bool _isAutoProfile        = false;
        private static bool _isProfilingFuncs     = false;
        private static bool _isFullWithStacktrace = false;

        private static string _appId;
        private static string _appVersion;
        private static string _resVersion;

        private const string CustomBuildOptionsKey       = "CustomBuildOptionsKey";
        private const string CustomBuildBackOptionsKey       = "CustomBuildBackOptionsKey";//保存备份项所需要的key
        private const string CustomBuildWindowSettingKey = "CustomBuildOWindowSettingKey";

        private static string[] curEnvironmentSettingsArray;
        private static int      curSelectEnvironmentIndex;

        private static CustomBuildWindowSettings _windowSettings;

        /// <summary>
        /// 停靠窗口类型集合, 与 YooAsset 的窗口合并在一起显示, 以后再重构这块窗口显示逻辑吧.
        /// </summary>
        public static readonly Type[] DockedWindowTypes = WindowsDefine.DockedWindowTypes.Select(type => type).Append(typeof(CustomBuildWindow)).ToArray();

        [MenuItem("Tools/构建版本", false, -202)]
        private static void OpenWindow()
        {
            GetWindow<CustomBuildWindow>("Custom Build", true, DockedWindowTypes);
        }

        private string _localIp = string.Empty;
        private void OnEnable()
        {
            _windowSettings = null;
           // LaunchSettings.Instance.ResetSettings();
            var settingStr = EditorPrefs.GetString(CustomBuildWindowSettingKey);
            if (!string.IsNullOrEmpty(settingStr))
                _windowSettings = JsonMapper.ToObject<CustomBuildWindowSettings>(settingStr);
            _localIp = "http://" + GetLocalIPAddress() + ":8082/";
            // LaunchSettings.localCdnDomain = localIP;
            // LaunchSettings.developmentSettings.cdnDomain = localIP;
            // LaunchSettings.testingSettings.cdnDomain = localIP;
            InitSetting();
            if (_windowSettings == null)
                _windowSettings = new CustomBuildWindowSettings();
        }

        private void OnDestroy()
        {
            CheckBeforeClose();
           // LaunchSettings.Instance.ResetSettings();
            _windowSettings = null;
            EditorPrefs.DeleteKey(CustomBuildWindowSettingKey);
            
        }

        void CheckBeforeClose()
        {
            string curVersion = GameVersion.GameVer;
            var curEnv = GameVersion.GameEnv;
            
            string tip = string.Empty;
            bool isEnvOrVersionChange = false;
            if (curEnv != _environmentType)
            {
                tip = $"当前环境：{curEnv.ToString()}\n 新环境:{_environmentType.ToString()}\n";
                isEnvOrVersionChange = true;
            }

            if (!string.Equals(curVersion, _resVersion))
            {
                tip += $"当前版本：{curVersion}\n 新版本:{_resVersion}\n";
                isEnvOrVersionChange = true;
            }

            bool isCdnChanged = false;
            if (!string.IsNullOrEmpty(_cdnUrl) && !_cdnUrl.Equals(LaunchSettings.GetEnvironmentSettings(_environmentType).cdnDomain))
            {
                tip += $"当前cdn:{LaunchSettings.GetEnvironmentSettings(_environmentType).cdnDomain}\n 新cdn:{_cdnUrl}\n";

                isCdnChanged = true;
            }
            if (!string.IsNullOrEmpty(tip) && EditorUtility.DisplayDialog("有新的变更，是否保存", tip, "保存", "不保存"))
            {
                if (isEnvOrVersionChange)
                {
                    GenerateGameVersionTool.GenerateGameVersionCodeFile(_resVersion, _environmentType);
                    var settings = AddressableAssetSettingsDefaultObject.Settings;
                    settings.BuildRemoteCatalog = true;
                    settings.OverridePlayerVersion = _resVersion;
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssets();
                }
                if(isCdnChanged)
                    ModifyLaunchSettingScript(_environmentType,_cdnUrl);
            }

            isEnvOrVersionChange = false;
            isCdnChanged = false;

        }

        private void OnDisable()
        {
            // LaunchSettings.Instance.ResetSettings();
        }

        /// <summary>
        /// 保存清理的 Action 列表
        /// </summary>
        private static List<Action> clearActions = new List<Action>();

        private void OnGUI()
        {
            GUIStyle textMiddleCenter = new GUIStyle(GUI.skin.label);
            textMiddleCenter.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label("--- 请选择小游戏AppId（正式AppId仅支持正式服）---", textMiddleCenter);
            CreateAppId();
            EditorGUILayout.Space();

            GUILayout.Label("--- 请选择运行环境（目前仅支持修改CDN地址） ---", textMiddleCenter);
            CreateEnvironment();
            EditorGUILayout.Space();

            GUILayout.Label("--- 版本信息 ---", textMiddleCenter);
            CreateVersion();
            EditorGUILayout.Space();

            GUILayout.Label("--- 调试选项（仅针对微信小游戏） ---", textMiddleCenter);
            CreateProfile();
            EditorGUILayout.Space();

            GUILayout.Label("--- 构建目标平台 ---", textMiddleCenter);
            CreateBuildPlatform();
        }

        static void InitSetting()
        {
            _appIdType = AppIdType.WebGL;
            if (_windowSettings != null)
            {
                if (Enum.IsDefined(typeof(AppIdType), _windowSettings.appIdType))
                {
                    _appIdType = (AppIdType)_windowSettings.appIdType;
                }

                _isCndOverride = _windowSettings.isOverrideCdn;
             
                if (_appIdType == AppIdType.WeChatBeta || _appIdType == AppIdType.WeChatProduction)
                {
                    _isDevelopmentBuild = _windowSettings.developmentBuild;
                    _isAutoProfile = _windowSettings.autoConnectProfiler;
                    _isProfilingFuncs = _windowSettings.profilingFuncs;
                }
                else
                {
                    _isDevelopmentBuild = false;
                    _isAutoProfile = false;
                    _isProfilingFuncs = false;
                    
                }
                _isFullWithStacktrace = _windowSettings.fullWithStacktrace;
            }

            _appId = _appIdDict[_appIdType];

            if (_appIdType.ToString().Contains("Production"))
            {
                _environmentType = LaunchSettings.EnvironmentType.Production;
            }
            else
            {
                _environmentType = (LaunchSettings.EnvironmentType)GameVersion.GameEnv;
            }

            if (_windowSettings != null)
            {
                var setting = LaunchSettings.GetEnvironmentSettings(_environmentType);
                setting.cdnDomain = _windowSettings.environmentCdn;
            }
            curEnvironmentSettingsArray = GetEnvironmentTypeArray(_appIdType);
            curSelectEnvironmentIndex = GetIndexFromArray(curEnvironmentSettingsArray, _environmentType.ToString());
            var launchSettings = LaunchSettings.GetEnvironmentSettings(_environmentType);

            if (_windowSettings == null || string.IsNullOrEmpty(_windowSettings.appVersion))
            {
                _isCndOverride = !launchSettings.cdnDomain.Equals(GetDefaultUrl(_environmentType));
                var version =  Application.version.Split(".");
                if (version.Length < 3)
                {
                    version = new[] { "0", "0", "1" };
                }

                _appVersion = version[0] + "." + version[1];
                _resVersion = GameVersion.GameVer; 
            }
            else
            {
                _appVersion = _windowSettings.appVersion;
                _resVersion = _windowSettings.resVersion;
            }

            _cdnUrl = launchSettings.cdnDomain;
        }


        static string[] GetEnvironmentTypeArray(AppIdType appIdType)
        {
            switch (appIdType)
            {
                case AppIdType.WebGL:
                case AppIdType.WeChatBeta:
                case AppIdType.ByteGameBeta:
                default:
                    return new string[]
                           {
                               LaunchSettings.EnvironmentType.Development.ToString(),
                               LaunchSettings.EnvironmentType.Testing.ToString(),
                               LaunchSettings.EnvironmentType.Beta.ToString()
                           };

                case AppIdType.WeChatProduction:
                case AppIdType.ByteGameProduction:
                    return new string[]
                           {
                               LaunchSettings.EnvironmentType.Production.ToString(),
                           };
            }
        }

        static int GetIndexFromArray(string[] types, string type)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].Equals(type))
                    return i;
            }

            return 0;
        }

        private static void CreateAppId()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AppId: ", GUILayout.Width(50));
            _appIdType = (AppIdType)EditorGUILayout.EnumPopup(_appIdType);
           
            if (_appId != _appIdDict[_appIdType])
            {
                _appId = _appIdDict[_appIdType];
                curEnvironmentSettingsArray = GetEnvironmentTypeArray(_appIdType);
                curSelectEnvironmentIndex = GetIndexFromArray(curEnvironmentSettingsArray, _environmentType.ToString());
            }
            if (_windowSettings != null)
                _windowSettings.appIdType = (int)_appIdType;
            GUILayout.EndHorizontal();
        }

       static string GetDefaultUrl(LaunchSettings.EnvironmentType type)
        {
            switch (type)
            {
                case LaunchSettings.EnvironmentType.Development:
                case LaunchSettings.EnvironmentType.Testing:
                    return LaunchSettings.localCdnDomain;
                    
                case LaunchSettings.EnvironmentType.Beta:
                case LaunchSettings.EnvironmentType.Production:
                   return LaunchSettings.RemoteCdnDomain;
                
            }

            return string.Empty;
        }
        private static void CreateEnvironment()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("运行环境: ", GUILayout.Width(60));
           

            curSelectEnvironmentIndex = EditorGUILayout.Popup("", curSelectEnvironmentIndex, curEnvironmentSettingsArray);
            _environmentType = Enum.Parse<LaunchSettings.EnvironmentType>(curEnvironmentSettingsArray[curSelectEnvironmentIndex]);
            if (_windowSettings != null)
                _windowSettings.environment = (int)_environmentType;
            
            string environmentName = "";
            switch (_environmentType)
            {
                case LaunchSettings.EnvironmentType.Development:
                    environmentName = "局域网";
                    break;
                case LaunchSettings.EnvironmentType.Testing:
                    environmentName = "内网";
                    break;
                case LaunchSettings.EnvironmentType.Beta:
                    environmentName = "测试服";
                    break;
                case LaunchSettings.EnvironmentType.Production:
                    environmentName = "正式服（仅支持正式AppId）";
                    break;
            }

            EditorGUILayout.LabelField(environmentName, GUILayout.Width(160));
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            var environmentSettings = LaunchSettings.GetEnvironmentSettings(_environmentType);

            GUILayout.BeginHorizontal();
            _isCndOverride = EditorGUILayout.Toggle(_isCndOverride, GUILayout.Width(20));
            if (_windowSettings != null)
                _windowSettings.isOverrideCdn = _isCndOverride;
            if (!_isCndOverride)
            {
                switch (_environmentType)
                {
                    case LaunchSettings.EnvironmentType.Development:
                    case LaunchSettings.EnvironmentType.Testing:
                        _cdnUrl = LaunchSettings.localCdnDomain;
                        break;
                    case LaunchSettings.EnvironmentType.Beta:
                    case LaunchSettings.EnvironmentType.Production:
                        _cdnUrl = LaunchSettings.RemoteCdnDomain;
                        break;
                }
            }

            EditorGUILayout.LabelField("CDN: ", GUILayout.Width(40));
            EditorGUI.BeginDisabledGroup(!_isCndOverride);
            _cdnUrl = EditorGUILayout.TextField(_cdnUrl);
            if (_windowSettings != null)
                _windowSettings.environmentCdn = _cdnUrl;
            if (GUILayout.Button("本地IP"))
            {
                _cdnUrl = "http://" + GetLocalIPAddress() + ":8082/";
            }
            if (GUILayout.Button("LocalHost"))
            {
                _cdnUrl = "http://localhost:8082/";
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("登录服: ", GUILayout.Width(50));
            EditorGUILayout.TextField(environmentSettings.loginDomain);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("游戏服: ", GUILayout.Width(50));
            EditorGUILayout.TextField(environmentSettings.gameDomain);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }

        private static void CreateVersion()
        {
            _appVersion = EditorGUILayout.TextField("程序版本号", _appVersion);
            _resVersion = EditorGUILayout.TextField("资源版本号", _resVersion);
            if (_windowSettings != null)
            {
                _windowSettings.appVersion = _appVersion;
                _windowSettings.resVersion = _resVersion;
            }
        }

        private static void CreateProfile()
        {
            GUILayout.BeginHorizontal();
            _isDevelopmentBuild = EditorGUILayout.Toggle(_isDevelopmentBuild, GUILayout.Width(20));
            EditorGUILayout.LabelField("Development Build", GUILayout.Width(120));
            _isAutoProfile = EditorGUILayout.Toggle(_isAutoProfile, GUILayout.Width(20));
            EditorGUILayout.LabelField("Auto connect Profiler", GUILayout.Width(140));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _isProfilingFuncs = EditorGUILayout.Toggle(_isProfilingFuncs, GUILayout.Width(20));
            EditorGUILayout.LabelField("Profiling Funcs", GUILayout.Width(120));
            _isFullWithStacktrace = EditorGUILayout.Toggle(_isFullWithStacktrace, GUILayout.Width(20));
            EditorGUILayout.LabelField("Full With Stacktrace", GUILayout.Width(120));
            GUILayout.EndHorizontal();
            if (_windowSettings != null)
            {
                _windowSettings.developmentBuild = _isDevelopmentBuild;
                _windowSettings.autoConnectProfiler = _isAutoProfile;
                _windowSettings.profilingFuncs = _isProfilingFuncs;
                _windowSettings.fullWithStacktrace = _isFullWithStacktrace;
            }
        }

        static void SaveSetting()
        {
            if (_windowSettings != null)
            {
                EditorPrefs.SetString(CustomBuildWindowSettingKey, JsonMapper.ToJson(_windowSettings));
            }
        }

        private static void CreateBuildPlatform()
        {
            switch (_appIdType)
            {
                case AppIdType.WebGL:
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("构建WebGL"))
                    {
                        if(!string.IsNullOrEmpty(_cdnUrl) && !_cdnUrl.Equals(LaunchSettings.GetEnvironmentSettings(_environmentType).cdnDomain))
                            ModifyLaunchSettingScript(_environmentType,_cdnUrl);
                        SaveSetting();
                        bool switchSuccess = PlatformSwitcher.SwitchEditor();
                        if (!switchSuccess)
                        {
                            EditorUtility.DisplayDialog("warn", "平台切换失败，没有安装WebGL模块", "ok");
                            return;
                        }
                        StartBuild(PlatformType.WebGL, "");
                    }

                    GUILayout.EndHorizontal();
                    break;
                case AppIdType.WeChatBeta:
                case AppIdType.WeChatProduction:
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("构建微信小游戏"))
                    {
                       if(!string.IsNullOrEmpty(_cdnUrl) && !_cdnUrl.Equals(LaunchSettings.GetEnvironmentSettings(_environmentType).cdnDomain))
                           ModifyLaunchSettingScript(_environmentType,_cdnUrl);
                       SaveSetting();
                       bool switchSuccess = PlatformSwitcher.SwitchWeChat();
                       if (!switchSuccess)
                       {
                           EditorUtility.DisplayDialog("warn", "平台切换失败，没有安装WebGL模块", "ok");
                           return;
                       }
                       StartBuild(PlatformType.WeChat, "");
                      
                    }

                    GUILayout.EndHorizontal();
                    break;
                case AppIdType.ByteGameBeta:
                case AppIdType.ByteGameProduction:
                    // GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    if (GUILayout.Button("构建抖音小游戏"))
                    {
                        if(!string.IsNullOrEmpty(_cdnUrl) && !_cdnUrl.Equals(LaunchSettings.GetEnvironmentSettings(_environmentType).cdnDomain))
                            ModifyLaunchSettingScript(_environmentType,_cdnUrl);
                        SaveSetting();
                        bool switchSuccess = PlatformSwitcher.SwitchByteGame();
                        if (!switchSuccess)
                        {
                            EditorUtility.DisplayDialog("warn", "平台切换失败，没有安装WebGL模块", "ok");
                            return;
                        }
                        StartBuild(PlatformType.ByteGame, "2443534060226444");
                      
                    }
                    if (GUILayout.Button("构建抖音Native+WebGL"))
                    {
                       
                        if(!string.IsNullOrEmpty(_cdnUrl) && !_cdnUrl.Equals(LaunchSettings.GetEnvironmentSettings(_environmentType).cdnDomain))
                            ModifyLaunchSettingScript(_environmentType,_cdnUrl);
                        SaveSetting();
                        bool switchSuccess = PlatformSwitcher.SwitchByteGame();
                        if (!switchSuccess)
                        {
                            EditorUtility.DisplayDialog("warn", "平台切换失败，没有安装WebGL模块", "ok");
                            return;
                        }
                        StartBuild(PlatformType.ByteGame, "2443534060226444", true);
                   
                    }
                    GUILayout.EndVertical();
                    // GUILayout.EndHorizontal();
                    break;
                default:

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("需要自定义实现"))
                    {
                        if(!string.IsNullOrEmpty(_cdnUrl) && !_cdnUrl.Equals(LaunchSettings.GetEnvironmentSettings(_environmentType).cdnDomain))
                            ModifyLaunchSettingScript(_environmentType,_cdnUrl);
                        SaveSetting();
                    }

                    GUILayout.EndHorizontal();
                    break;
            }
            // GUILayout.BeginHorizontal();
            // if (GUILayout.Button("构建微信小游戏"))
            // {
            //     PlatformSwitcher.SwitchWeChat();
            //     StartBuild(PlatformType.WeChat,"");
            // }
            //
            // if (GUILayout.Button("构建抖音小游戏"))
            // {
            //     PlatformSwitcher.SwitchByteGame();
            //     StartBuild(PlatformType.ByteGame,"2286635002892040");
            // }
            //
            // GUILayout.EndHorizontal();
            //
            // GUILayout.BeginHorizontal();
            // if (GUILayout.Button("构建Android"))
            // {
            // }
            //
            // if (GUILayout.Button("构建WebGL"))
            // {
            //     PlatformSwitcher.SwitchEditor();
            //     StartBuild(PlatformType.WebGL,"");
            // }
            //
            // GUILayout.EndHorizontal();
        }

        private static string GetLocalIPAddress()
        {
            string ip = "No IP Address";

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface variable in interfaces)
            {
                if (variable.NetworkInterfaceType == NetworkInterfaceType.Ethernet || variable.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    foreach (UnicastIPAddressInformation ipInfo in variable.GetIPProperties().UnicastAddresses)
                    {
                        if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            string currentIP = ipInfo.Address.ToString();
                            if (currentIP.StartsWith("192."))
                            {
                                ip = currentIP;
                                break;
                            }
                        }
                    }
                }

                if (!ip.Equals("No IP Address"))
                {
                    break;
                }
            }

            return ip;
        }

        private static void StartBuild(PlatformType platformType, string uId, bool isNative = false)
        {
            try
            {
                CustomBuildOptions customBuildOptions = new CustomBuildOptions();
                customBuildOptions.platformType = platformType;
                customBuildOptions.appId = _appId;
                customBuildOptions.appVersion = _appVersion;
                customBuildOptions.resVersion = _resVersion;
                customBuildOptions.uId = uId;
                customBuildOptions.isNative = isNative;
                Debug.Log(($"### 当前打包版本号: {_resVersion} 环境： {_environmentType}"));
                //设置版本号到本地脚本里
                GenerateGameVersionTool.GenerateGameVersionCodeFile(_resVersion, _environmentType);

                //设置备份参数
                var backupOptions = new CustomBuildBackUpOptions()
                                    {
                                        platformType = platformType,
                                        resVersion = _resVersion
                                    };
                Environment.SetEnvironmentVariable(CustomBuildBackOptionsKey, JsonMapper.ToJson(backupOptions));

                Debug.Log($"### 1 Start 开始构建{JsonMapper.ToJson(customBuildOptions)}");
                Environment.SetEnvironmentVariable(CustomBuildOptionsKey, JsonMapper.ToJson(customBuildOptions));
                CompilationPipeline.RequestScriptCompilation();
                // 监听编译完成事件  EditorApplication.isCompiling 在batchMode 下的值有问题，不能用于判断编译是否完成
                CompilationPipeline.compilationFinished += OnCompilationFinished;
                CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            }
            catch (Exception ex)
            {
                Debug.LogError($"构建过程中发生异常: {ex.Message}");
                Quit(-1);
            }
           
            // CompilationPipeline.compilationStarted += o =>
            // {
            //     Debug.Log("### 开始编译");
            // };
            // CompilationPipeline.compilationStarted += o =>
            // {
            //     Debug.Log("### 开始编译");
            // };
        }
        
        /// <summary>
        /// 编译完成后的回调，早于[UnityEditor.Callbacks.DidReloadScripts]
        /// 如果是切换平台后发生的编译报错，不会走[UnityEditor.Callbacks.DidReloadScripts]回调，所以不会中断TeamCity 报错，所以加了这个回调
        /// </summary>
        private static void OnCompilationFinished(object context)
        {
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;   
        }
        /// <summary>
        /// 每个程序集编译后的回调
        /// </summary>
        /// <param name="path"></param>
        /// <param name="compilerMessages"></param>
        static void OnAssemblyCompilationFinished(string path, CompilerMessage[] compilerMessages)
        {
            for (int i = 0; i < compilerMessages.Length; i++)
            {
                var compilerMessage = compilerMessages[i];
                if (compilerMessage.type == CompilerMessageType.Error)
                {
                    Debug.LogError($"### 脚本编译失败！error:{compilerMessage.message} in {compilerMessage.file} line:{compilerMessage.line}");
                    Quit(-1);
                    break;
                }
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnDidReloadScripts()
        {
            var json = Environment.GetEnvironmentVariable(CustomBuildOptionsKey);
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            string content = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL);
            Debug.Log($"### DefineSymbols {content}");

            EditorApplication.update += CompilationFinished;
        }

        private static void CompilationFinished()
        {
            EditorApplication.update -= CompilationFinished;
            var json = Environment.GetEnvironmentVariable(CustomBuildOptionsKey);
            if (string.IsNullOrEmpty(json))
            {
                return;
            }


            var customBuildOptions = JsonMapper.ToObject<CustomBuildOptions>(json);
            switch (customBuildOptions.platformType)
            {
                case PlatformType.WeChat:
                    BuildWeChatMiniGame(customBuildOptions.appId, customBuildOptions.appVersion, customBuildOptions.resVersion);
                    Environment.SetEnvironmentVariable(CustomBuildOptionsKey, "");
                    break;
                case PlatformType.ByteGame:
                    Debug.Log($"### ByteGame Compilation Finished. {JsonMapper.ToJson(customBuildOptions)}");
                    // 纯webgl构建，走之前的逻辑
                    if (!customBuildOptions.isNative)
                    {
                        Debug.Log("### 1-1 Start 开始构建抖音小游戏纯WebGL");
                        BuildDouYinMiniGame(customBuildOptions.appId, customBuildOptions.appVersion, customBuildOptions.resVersion, customBuildOptions.uId, customBuildOptions.isNative);
                        Environment.SetEnvironmentVariable(CustomBuildOptionsKey, "");
                    }
                    // Native+WebGL Step1: 处理基础参数，切换安卓平台，等待脚本编译完成
                    else if (!customBuildOptions.waitNativeCompile)
                    {
                        Debug.Log("### 1-1 Start 开始构建抖音小游戏Native + WebGL");
                        customBuildOptions.waitNativeCompile = true;
                        Environment.SetEnvironmentVariable(CustomBuildOptionsKey, JsonMapper.ToJson(customBuildOptions));
                        BuildDouYinMiniGame(customBuildOptions.appId, customBuildOptions.appVersion, customBuildOptions.resVersion, customBuildOptions.uId, customBuildOptions.isNative);
                    }
                    // Native+WebGL Step2: 打包Native，切换WebGL平台，等待脚本编译完成
                    else if (string.IsNullOrEmpty(customBuildOptions.apkPath))
                    {
                        Debug.Log("### 1-3 Start 开始打包抖音Native");
                        BuildDouYinNative(customBuildOptions);
                    }
                    // Native+WebGL Step3: 打包WebGL，平台发布
                    else if (string.IsNullOrEmpty(customBuildOptions.webGlPath))
                    {
                        Debug.Log("### 1-4 Start 开始打包抖音WebGL");
                        BuildDouYinWebGLAndPublish(customBuildOptions);
                    }
                    break;
                case PlatformType.WebGL:
                    BuildWebGl(customBuildOptions.appVersion, customBuildOptions.resVersion);
                  
                    break;
            }
        }
        
        /// <summary>
        /// 打包完成退出
        /// </summary>
        /// <param name="quitValue">值是0，正常退出</param>
        private static void Quit(int quitValue = 0)
        {
            if (quitValue == 0)
            {
                //获取备份所需数据
                var json = Environment.GetEnvironmentVariable(CustomBuildBackOptionsKey);
                if (!string.IsNullOrEmpty(json))
                {
                    //备份serverData 和 Build 文件夹
                    var backUpOptions = JsonMapper.ToObject<CustomBuildBackUpOptions>(json);
                    BackUpCreateZip(backUpOptions.resVersion,backUpOptions.platformType);
                }
                Environment.SetEnvironmentVariable(CustomBuildBackOptionsKey, "");
                Debug.Log("### BuildSuccess");
            }
            else
            {
                Debug.Log("### BuildFailed");
            }
            if (Application.isBatchMode)
            {
                Debug.Log("### 开始退出");
                EditorApplication.Exit(quitValue);
            }
        }

        /// Python调用构建
        public static void BuildMiniGame()
        {
            List<string> commandLineArgs = Environment.GetCommandLineArgs().ToList();
            Debug.Log("### commandLineArgs " + JsonMapper.ToJson(commandLineArgs));
            int index = commandLineArgs.IndexOf("-platform");
            string platform = commandLineArgs[index + 1];
            index = commandLineArgs.IndexOf("-appId");
            _appId = commandLineArgs[index + 1];
            index = commandLineArgs.IndexOf("-appVersion");
            _appVersion = commandLineArgs[index + 1];
            index = commandLineArgs.IndexOf("-resVersion");
            _resVersion = commandLineArgs[index + 1];
            //PlayerSettings.bundleVersion = _appVersion + "." + _resVersion; //不再设置PlayerSetting的版本号，目前没有用到，直接读取GameVersion.GameVer
            index = commandLineArgs.IndexOf("-env");
            string env = commandLineArgs[index + 1];
           
            env = env.Trim();
            if (Enum.TryParse(env, true, out LaunchSettings.EnvironmentType envType))
            {
                Debug.Log($"解析 environmentType: {envType}");
            }
            else
            {
                Debug.Log($"解析 environmentType 失败: {env}");
                Quit(-1);
                return;
            }

            index = commandLineArgs.IndexOf("-uid");
            string uid = commandLineArgs[index + 1];
            _environmentType = envType;
            
            // LaunchSettings.Instance.ResetSettings();
            // LaunchSettings.Instance.environmentType = envType;
            // EditorUtility.SetDirty(LaunchSettings.Instance);
            // AssetDatabase.SaveAssets();
            Debug.Log($"environmentType: {_environmentType}");
        
            switch (platform)
            {
                case "wx":
                    PlatformSwitcher.SwitchWeChat();
                    StartBuild(PlatformType.WeChat, uid);
                    break;
                case "tt":
                    PlatformSwitcher.SwitchByteGame();
                    StartBuild(PlatformType.ByteGame, uid);
                    break;
                case "ttnw":
                    PlatformSwitcher.SwitchByteGame();
                    StartBuild(PlatformType.ByteGame, uid, true);
                    break;
                default:
                    Debug.LogError($"### TeamCity配置的参数platform：【{platform}】 没有处理");
                    break;
            }
        }

        /// <summary>
        /// 构建微信小游戏, 会自动清理其它库
        /// </summary>
        /// <param name="appVersion">程序的版本号</param>
        /// <param name="resVersion">资源的版本号</param>
        public static void BuildDouYinMiniGame(string appId, string appVersion, string resVersion, string uid, bool isNative = false)
        {
            DoBuild(EHostPlatform.Tt, appVersion, resVersion, BuildAction, "PLATFORM_DOUYIN").Forget();
            return;

#pragma warning disable 1998
            async UniTask BuildAction()
            {
                Debug.Log("###1-2 Start 配置抖音参数");
                // 打包机第一次拉取代码打包时，如果识别自定义宏，用编辑器打开一次，就可以了
#if PLATFORM_DOUYIN
              
                var settings = StarkSDKTool.StarkBuilderSettings.Instance;
                settings.appId = appId;
                settings.needCompress = true;
                // WebGL
                settings.webGLOutputDir = Path.Combine(Environment.CurrentDirectory, "Build");
                // Native
                settings.apkOutputDir = Path.Combine(Environment.CurrentDirectory, "Build");
                // 横屏游戏
                settings.orientation = StarkBuilderSettings.Orientation.Landscape;
                Debug.Log($"### settings.version 0 {settings.version}");
                settings.urlCacheList = new[]
                {
                    "https://incubator-static.easygame2021.com"
                };
                Debug.Log($"### settings.version 1 {settings.version}");
                var version = appVersion + "." + resVersion;
                Debug.Log($"ByteGame version " + version);
                Debug.Log("###1-2 End 配置抖音参数");
                if (isNative) // 判断参数
                {
                    // 删除一下ServerData下的两个目录
                    FileUtil.DeleteFileOrDirectory(Path.Combine(Environment.CurrentDirectory, "ServerData", "Android"));
                    FileUtil.DeleteFileOrDirectory(Path.Combine(Environment.CurrentDirectory, "ServerData", "WebGL"));
                    // switch android and compile script
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                    CompilationPipeline.RequestScriptCompilation();
                }
                else
                {
                    // webglZipPath 存储的是打包后的文件路径
                    var webglZipPath = await StarkSDKTool.API.BuildManager.Build(Framework.Wasm);
                    bool publishResult = await StarkSDKTool.API.PublishManager.PublishAndroidWebGLWithIOS(uid, appId, version, webglZipPath);
                    Debug.Log(publishResult ? $"### 发布成功 {webglZipPath}" : $"### 发布失败 {webglZipPath}");
                    Debug.Log("###1-1 End 结束构建抖音小游戏纯WebGL");
                    Debug.Log("###1 End 构建结束");
                    Quit(publishResult? 0 : -1);
                }
               
#else
                Debug.Log("###1 End 构建结束");
                Debug.LogError($"### 错误的打包方法调用[BuildDouYinMiniGame]，清空打包配置缓存 {isNative}");
                Environment.SetEnvironmentVariable(CustomBuildOptionsKey, "");
                Quit(-1);
#endif
            }
        }

        /// <summary>
        /// 构建抖音的Native包
        /// </summary>
        /// <param name="options"></param>
        private static async void BuildDouYinNative(CustomBuildOptions options)
        {
            
#if PLATFORM_DOUYIN
            string appVersion = options.appVersion;
            string resVersion = options.resVersion;
            BuildScript.buildCompleted += TempBuildCompleted;
            string apkPath = await StarkSDKTool.API.BuildManager.Build(Framework.Native);
            BuildScript.buildCompleted -= TempBuildCompleted;
            if (string.IsNullOrEmpty(apkPath))
            {
                Debug.LogError($"### 1-3 End 抖音Native打包失败");
                Environment.SetEnvironmentVariable(CustomBuildOptionsKey, "");
                Quit(-1);
                return;
            }
            options.apkPath = apkPath;
            Debug.Log($"### 1-3 End 抖音Native打包成功 {apkPath}");
            Environment.SetEnvironmentVariable(CustomBuildOptionsKey, JsonMapper.ToJson(options));
            // switch webgl and compile script
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            CompilationPipeline.RequestScriptCompilation();

            void TempBuildCompleted(AddressableAssetBuildResult result)
            {
                Debug.Log("### BuildScript.buildCompleted from BuildDouYinNative");
                OnBuildCompleted(result, appVersion, resVersion);
            }
#else
            Environment.SetEnvironmentVariable(CustomBuildOptionsKey, "");
            Debug.Log("###1 End 构建结束");
            Debug.LogError($"### 错误的打包方法调用[BuildDouYinNative]，清空打包配置缓存 {JsonMapper.ToJson(options)}");
            Quit(-1);
#endif
        }

        /// <summary>
        /// 构建抖音的WebGL包，然后发布
        /// </summary>
        /// <param name="options"></param>
        private static async void BuildDouYinWebGLAndPublish(CustomBuildOptions options)
        {
            bool publishResult = false;
#if PLATFORM_DOUYIN
            string appVersion = options.appVersion;
            string resVersion = options.resVersion;
            BuildScript.buildCompleted += TempBuildCompleted;
            string webglZipPath = await StarkSDKTool.API.BuildManager.Build(Framework.Wasm);
            BuildScript.buildCompleted -= TempBuildCompleted;
            if (string.IsNullOrEmpty(webglZipPath))
            {
                Debug.LogError($"###1-4 End 抖音WebGL打包失败");
                Environment.SetEnvironmentVariable(CustomBuildOptionsKey, "");
                Quit(-1);
                return;
            }
            options.webGlPath = webglZipPath;
            
            Debug.Log($"###1-4 End 抖音WebGL打包成功  {webglZipPath}");
            // publish
            string version = options.appVersion + "." + options.resVersion;
             publishResult = await StarkSDKTool.API.PublishManager.PublishAndroidNativeWithIOS(options.uId, options.appId, version, options.apkPath, options.webGlPath);
            if (publishResult)
            {
                Debug.Log($"### 抖音Native+WebGL发布成功 {options.apkPath} {options.webGlPath}");
            }
            else
            {
                Debug.LogError($"### 抖音Native+WebGL发布失败 {options.apkPath} {options.webGlPath}");
            }
            void TempBuildCompleted(AddressableAssetBuildResult result)
            {
                Debug.Log("### BuildScript.buildCompleted from BuildDouYinWebGL");
                OnBuildCompleted(result, appVersion, resVersion);
            }
#else
            Debug.LogError($"### 错误的打包方法调用[BuildDouYinWebGLAndPublish]，清空打包配置缓存 {JsonMapper.ToJson(options)}");
#endif
            Debug.Log("### 1-1 End 构建抖音小游戏Native + WebGL 结束");
            Debug.Log("### 1 End 构建结束");
            Environment.SetEnvironmentVariable(CustomBuildOptionsKey, "");
            Quit(publishResult? 0 : -1);
        }

        /// <summary>
        /// 构建微信小游戏包, 会自动清理其它库
        /// </summary>
        public static void BuildWeChatMiniGame(string appId, string appVersion, string resVersion)
        {
            Debug.Log("### 1-1 Start 开始构建微信小游戏");
            DoBuild(EHostPlatform.Wx, appVersion, resVersion, BuildAction, "PLATFORM_WEIXIN").Forget();
            return;

            async UniTask BuildAction()
            {
                bool result = false;
          
#if PLATFORM_WEIXIN
                WeChatWASM.WXConvertCore.config.ProjectConf.Appid = appId;
                WeChatWASM.WXConvertCore.config.ProjectConf.DST = Path.Combine(Environment.CurrentDirectory, "Build");
                var curSettings = LaunchSettings.GetEnvironmentSettings(GameVersion.GameEnv);
                var cdnDomain = curSettings.cdnDomain;
                WeChatWASM.WXConvertCore.config.ProjectConf.CDN = cdnDomain.StartsWith("https") ? cdnDomain : "";
                WeChatWASM.WXConvertCore.config.ProjectConf.bundlePathIdentifier = cdnDomain.StartsWith("https") ? "WebGL;" : "";
                WeChatWASM.WXConvertCore.config.CompileOptions.DevelopBuild = _isDevelopmentBuild;
                WeChatWASM.WXConvertCore.config.CompileOptions.AutoProfile = _isAutoProfile;
                WeChatWASM.WXConvertCore.config.CompileOptions.profilingFuncs = _isProfilingFuncs;
                WeChatWASM.WXConvertCore.DoExport();
                result = true;
                Debug.Log("###1-1 End 微信小游戏构建结束");
#endif
                Debug.Log("###1 End 结束构建");
                Quit(result?0:-1);
            }
        }

        /// <summary>
        /// 构建 WebGL 包
        /// </summary>
        private static void BuildWebGl(string appVersion, string resVersion)
        {
            Debug.Log("### 1-1 Start 开始构建WebGL");
            DoBuild(EHostPlatform.Web, appVersion, resVersion, BuildAction, "PLATFORM_WEB").Forget();
            return;

            async UniTask BuildAction()
            {
                var locationPathName = Path.Combine(Environment.CurrentDirectory, "Build", "WebGL");
                var buildPlayerOptions = new BuildPlayerOptions()
                                         {
                                             scenes = new[] { "Assets/Bootstrap.unity" },
                                             locationPathName = locationPathName,
                                             target = BuildTarget.WebGL,
                                             options = BuildOptions.None
                                         };
                var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                bool result = report.summary.result == BuildResult.Succeeded;
                if (result)
                {
                    Debug.Log($"### 1-1 End  WebGL构建成功. 目标目录: {locationPathName}");
                }
                else
                {
                    Debug.Log($"### 1-1 End WebGL构建失败. {report.summary.result}");
                }
                Debug.Log($"### 1 End 构建结束. {report.summary.result}");
                Environment.SetEnvironmentVariable(CustomBuildOptionsKey, "");
                Quit(result?0:-1);
            }
        }

        /// <summary>
        /// 开始构建指定平台
        /// </summary>
        /// <param name="platform">指定要构建的平台</param>
        /// <param name="appVersion">程序版本号</param>
        /// <param name="resVersion">资源版本号</param>
        /// <param name="buildAction">真正的构建函数</param>
        /// <param name="symbolsToAdd">指定要添加的宏定义</param>
        private static async UniTask DoBuild(EHostPlatform platform, string appVersion, string resVersion, Func<UniTask> buildAction, string symbolsToAdd)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // 提示用户是否保存场景
                return;
            }

            Debug.Log("### 开始清空Build目录");
            FileUtil.DeleteFileOrDirectory(Path.Combine(Environment.CurrentDirectory, "Build"));
            
            Debug.Log("### 开始清空ServerData目录");
            FileUtil.DeleteFileOrDirectory(Path.Combine(Environment.CurrentDirectory, "ServerData"));
            Debug.Log($"开始构建 {platform}, 程序版本号: {appVersion}, 资源版本号: {resVersion}");

            // 清理掉清理动作
            clearActions.Clear();
            EnsureBuildProfile();

            // 清理当前的构建目录
            if (Directory.Exists(RemoteBuildPath))
                Directory.Delete(RemoteBuildPath, true);

            EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;
            PlayerSettings.WebGL.exceptionSupport = _isFullWithStacktrace ? WebGLExceptionSupport.FullWithStacktrace : WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;

            // 设置catlog版本
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            settings.BuildRemoteCatalog = true;
            settings.OverridePlayerVersion = resVersion;//Application.version;
            
            BuildScript.buildCompleted += TempBuildCompleted;
            clearActions.Add(() => BuildScript.buildCompleted -= TempBuildCompleted);
            try
            {
                {
                    // 自动收集及构建 AA 的资源 Entry
                    Debug.Log("### 开始收集及分析打包资源");
                    await RefreshAddressableGroupEntries(resVersion);
                }

                Debug.Log("### 开始构建");
                await buildAction();
                return;
            }
            finally
            {
                foreach (var clearAction in clearActions)
                {
                    clearAction();
                }
            }

            void TempBuildCompleted(AddressableAssetBuildResult result)
            {
                Debug.Log("### BuildScript.buildCompleted");
                OnBuildCompleted(result, appVersion, resVersion);
            }
        }

        /// <summary>
        /// 确保构建配置
        /// </summary>
        private static void EnsureBuildProfile()
        {
            // 确保当前的 Profile 为 Production
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var profileId = settings.profileSettings.GetProfileId(PlatformUtils.ProductionProfileName);
            if (string.IsNullOrEmpty(profileId))
            {
                throw new Exception($"没有找到 Addressable 中名称为 {PlatformUtils.ProductionProfileName} 的 Profile, 请配置专门发布用的 Profile");
            }

            var oldActiveProfileId = settings.activeProfileId;
            settings.activeProfileId = profileId;

            // 添加恢复的操作
            clearActions.Add(() => { settings.activeProfileId = oldActiveProfileId; });

            // 设置 Play Mode 为 Use Existing Build
            var oldPlayModeDataBuilderIndex = settings.ActivePlayModeDataBuilderIndex;
            var useExistingBuildModeIndex = settings.DataBuilders.IndexOf(settings.DataBuilders.Find(s => s is BuildScriptPackedPlayMode));
            if (oldPlayModeDataBuilderIndex != useExistingBuildModeIndex)
            {
                settings.ActivePlayModeDataBuilderIndex = useExistingBuildModeIndex;
                // 添加恢复的操作
                clearActions.Add(() => { settings.ActivePlayModeDataBuilderIndex = oldPlayModeDataBuilderIndex; });
            }
        }

        public static string RemoteBuildPath
        {
            get
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                var baseRemoteBuildPathValue = settings.profileSettings.GetValueByName(settings.activeProfileId, AddressableAssetSettings.kRemoteBuildPath);
                var remoteBuildPath = settings.profileSettings.EvaluateString(settings.activeProfileId, baseRemoteBuildPathValue);
                return remoteBuildPath;
            }
        }

        /// <summary>
        /// Addressable Asset 构建后的处理, 比如复制 settings.json 等文件到 ServerData 目录
        /// </summary>
        private static void OnBuildCompleted(AddressableAssetBuildResult result, string appVersion, string resVersion)
        {
            // 复制 settings.json, catalog.json 等文件到 ServerData/[BuildTarget] 目录, 以实现 settings.json, catalog.json 文件的热更新 
            var srcPath = result.OutputPath;

            // var settings = AddressableAssetSettingsDefaultObject.Settings;
            var remoteBuildPath = RemoteBuildPath;
            var targetPath = Path.Combine(remoteBuildPath, $"settings_{resVersion}.json");
            Debug.Log("### settings targetPath " + targetPath);
            File.Copy(srcPath, targetPath,true);
          
        }
        
        //LaunchSettings.cs脚本所在位置
        private static readonly string LaunchSettingFilePath = "Assets/Sdk/Runtime/LaunchSettings.cs";

        //数组的值和LaunchSettings.cs脚本中定义的各个环境的变量名是对应的
        private static string[] EnvTypePropertyNames = new string[]
                                                       {
                                                           "developmentSettings",
                                                           "testingSettings",
                                                           "betaSettings",
                                                           "productionSettings",
                                                       };
        
        static void ModifyLaunchSettingScript(LaunchSettings.EnvironmentType environmentType, string newDomain)
        {
            if (!File.Exists(LaunchSettingFilePath))
            {
                Debug.LogError("File not found: " + LaunchSettingFilePath);
                return;
            }

            string[] lines = File.ReadAllLines(LaunchSettingFilePath);
            bool insideTargetSection = false;
            string pattern = $@"({EnvTypePropertyNames[(int)environmentType]}\.cdnDomain\s*=\s*"")[^""]*(""[;])";   // 动态生成正则表达式模式
            string cdnPattern =$@"({EnvTypePropertyNames[(int)environmentType]}\.cdnSuffix\s*=\s*"")[^""]*(""[;])"; // 动态生成正则表达式模式

            string modifyStr = string.Empty;
            string oldStr = string.Empty;
            string newSuffix = string.Empty;
            if (LaunchSettings.IsLocalCdn(newDomain))
            {
                newSuffix = string.Empty;
            }
            else
            {
                switch (environmentType)
                {
                    case LaunchSettings.EnvironmentType.Development:
                    case LaunchSettings.EnvironmentType.Testing:
                        
                        break;
                    case LaunchSettings.EnvironmentType.Beta:
                        break;
                    case LaunchSettings.EnvironmentType.Production:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(environmentType), environmentType, null);
                }
            }
            
            using (StreamWriter writer = new StreamWriter(LaunchSettingFilePath))
            {
                foreach (string line in lines)
                {
                    if (line.Contains("#Start"))
                    {
                        insideTargetSection = true;
                        writer.WriteLine(line);
                        continue;
                    }

                    if (line.Contains("#End"))
                    {
                        insideTargetSection = false;
                    }

                    if (insideTargetSection && Regex.IsMatch(line, pattern))
                    {
                        oldStr = line;
                     
                        string modifiedLine = Regex.Replace(line, pattern, $"$1{newDomain}$2");
                        modifyStr = modifiedLine;
                        writer.WriteLine(modifiedLine);
                    }
                    else if(insideTargetSection && Regex.IsMatch(line,cdnPattern))
                    {
                        oldStr += line;
                     
                        string modifiedLine = Regex.Replace(line, pattern, $"$1{newDomain}$2");
                        modifyStr += modifiedLine;
                        writer.WriteLine(modifiedLine);
                    }
                    else
                    {
                        writer.WriteLine(line);
                    }
                }
            }
            Debug.Log($"Script modified successfully.\n oldLine:{oldStr} \n newLine:{modifyStr}");
        }

        
        #region 备份bundle
        
        public static void BackUpCreateZip(string resVersion,PlatformType platform)
        {
            string assetTopPath = Application.dataPath.Replace("Assets", "");
            
            var remoteBuildPath = RemoteBuildPath;
            string wasmBuildPath = string.Empty;
            string targetDirName = string.Empty;
            switch (platform)
            {
                case PlatformType.WebGL:
                    targetDirName = "Build/WebGL";
                    break;
                case PlatformType.WeChat:
                    targetDirName = "Build/minigame";
                    break;
                case PlatformType.ByteGame:
                    targetDirName = "Build/webgl";
                    break;
                default:
                    Debug.LogError($"检查下{platform.ToString()}是否需要处理打包备份");
                    return;
            }

            wasmBuildPath = assetTopPath + targetDirName;
            
            string targetPath = assetTopPath + "BuildBackUp/";
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);
            

            //设置备份zip文件的路径
            string currentTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string zipOutputPath = Path.Combine(targetPath, $"bundle_{platform}_{resVersion}_{currentTime}.zip");
            // 确保两个文件夹都存在
            if (Directory.Exists(remoteBuildPath) && Directory.Exists(wasmBuildPath))
            {
                // 如果目标 zip 文件已存在，则先删除
                if (File.Exists(zipOutputPath))
                    File.Delete(zipOutputPath);

                // 创建临时文件夹来存储合并后的内容
                string tempFolderPath = Path.Combine(targetPath, $"tempFolder");
                if (Directory.Exists(tempFolderPath))
                    Directory.Delete(tempFolderPath, true);

                Directory.CreateDirectory(tempFolderPath);

                string firstDirName = Path.Combine(tempFolderPath, EditorUserBuildSettings.activeBuildTarget.ToString());
                if (!Directory.Exists(firstDirName))
                    Directory.CreateDirectory(firstDirName);
                // 将第一个文件夹的内容复制到临时文件夹
                CopyDirectory(remoteBuildPath, firstDirName);

                string secondDirName = Path.Combine(tempFolderPath,targetDirName);
                if (!Directory.Exists(secondDirName))
                    Directory.CreateDirectory(secondDirName);
                // 将第二个文件夹的内容复制到临时文件夹（如果有相同文件名，会覆盖）
                CopyDirectory(wasmBuildPath, secondDirName);

                // 打包临时文件夹
                ZipFile.CreateFromDirectory(tempFolderPath, zipOutputPath);
                Debug.Log("文件夹已成功打包为 ZIP 文件: " + zipOutputPath);

                // 删除临时文件夹
                Directory.Delete(tempFolderPath, true);
                //将zip拷贝到teamcity 备份文件夹,便于teamcity 备份指定文件夹
                string teamcityBackUpFilePath = assetTopPath + "TeamCityBackUp/";
                if (Directory.Exists(teamcityBackUpFilePath))
                    Directory.Delete(teamcityBackUpFilePath, true);

                Directory.CreateDirectory(teamcityBackUpFilePath);
                if(File.Exists(zipOutputPath))
                    File.Copy(zipOutputPath, teamcityBackUpFilePath, overwrite: true);
            }
            else
            {
                Debug.LogError("要备份的文件夹不存在: " + remoteBuildPath + " 或 " + wasmBuildPath);
            }

            // 调用删除3个月之前的备份文件方法
            DeleteOldBackups(targetPath, TimeSpan.FromDays(90));
        }

        // 递归复制文件夹内容
        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException("源目录不存在: " + sourceDir);

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);

            // 复制所有文件
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            // 递归复制子目录
            foreach (DirectoryInfo subdir in dirs)
            {
                string targetSubDir = Path.Combine(destinationDir, subdir.Name);
                CopyDirectory(subdir.FullName, targetSubDir);
            }
        }
        
        //删除老的备份
        public static void DeleteOldBackups(string targetPath, TimeSpan expirationTime)
        {
            // 获取目标目录中的所有备份文件
            var backupFiles = Directory.GetFiles(targetPath, "*.zip");

            foreach (var file in backupFiles)
            {
                if (!Path.GetFileName(file).StartsWith("bundle"))
                    continue;
                // 获取文件的创建时间
                DateTime creationTime = File.GetCreationTime(file);
            
                // 如果文件的创建时间超过了设定的过期时间，则删除
                if (DateTime.Now - creationTime > expirationTime)
                {
                    try
                    {
                        File.Delete(file);
                        Console.WriteLine($"删除过期备份: {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"删除文件失败: {file} - 错误: {ex.Message}");
                    }
                }
            }
        }

        #endregion
        
    }
}
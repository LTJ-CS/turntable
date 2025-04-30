using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Cysharp.Threading.Tasks;
using LitJson;
using Newtonsoft.Json;
using Sdk.Runtime.Base;
using Sdk.Runtime.Platforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
#endif

namespace Sdk.Runtime.FirstScreen
{
    /// <summary> 
    /// 首屏的组件. 在这里一般做远程加载主场景等.
    /// </summary>
    /// <remarks>尽量少在这里直接绑定资源, 以减少首包大小, 一些后续的初始化逻辑可以在 Main 场景去做</remarks>
    /// <remarks>首屏场景不应该被重复加载使用</remarks>
    public class FirstScreenComponent : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("等待的提示")]
        private GameObject waitingTint;

        [SerializeField]
        [Tooltip("加载提示的文本, 注意引用字体的大小, 越小越好, 提示中只需要包含英文字母与数字即可")]
        private Text waitingText;

        [SerializeField]
        [Tooltip("加载提示的文本, 注意引用字体的大小, 越小越好, 提示中只需要包含英文字母与数字即可")]
        private Text errorText;

        [SerializeField]
        [Tooltip("要加载的入口场景")]
        private AssetReference entryScene;

#if MY_USE_DYNAMIC_AA && !UNITY_EDITOR
        [SerializeField]
        [Tooltip("生成环境资源版本服务器的 Url")]
        private string resourceServerUrl;
#else
        [SerializeField]
        [Tooltip("生产环境的CDN Url")]
        private string productionCdnUrl;
#endif

        [Header("初始化的任务注册列表，需要实现IInitializable接口")]
        [SerializeField]
        [Tooltip("注册初始化执行的, 会先预加载资源，不需要等待预加载完成，初始化的动作函数需要支持重复执行, 且按顺序执行!!!")]
        public List<MonoBehaviour> registerInitializables;


        /// <summary>
        /// 保存所有的要初始化动作
        /// </summary>
        private List<Func<UniTask>> initActions;

        /// <summary>
        /// 保存入口场景实例
        /// </summary>
        private SceneInstance entrySceneInstance;


        /// <summary>
        /// 预加载的路径
        /// </summary>
        private List<string> preLoadPathList = new List<string>();

        /// 预加载的handler
        /// </summary>
        private List<AsyncOperationHandle<object>> handlerList = new List<AsyncOperationHandle<object>>();


        /// <summary>
        /// 设置所有的初始化动作
        /// </summary>
        private void SetInitActions()
        {
            preLoadPathList.Clear();
            SetTintText("Initialize actions.");

            // 初始化所有初始化动作, 这些动作必须是异步的且必须支持 **重新执行**
            initActions = new List<Func<UniTask>>()
                          {
                              InitAddressable
                          };
            for (int i = 0; i < registerInitializables.Count; i++)
            {
                var initializableGo = registerInitializables[i];
                if (initializableGo is IInitializable initializable)
                {
                    //不能重试的初始化操作，放在Register方法里
                    initializable.Register();
                    //获取需要预加载的资源地址
                    initializable.GetPreloadPath(ref preLoadPathList);
                    // 注册所有的初始化动作. 注: 注册的动作如果有可以重试错误, 需要抛出异常, 以便重试.
                    initActions.Add(initializable.InitializeAsync);
                }
            }

            // 确保入口场景加载成功
            async UniTask EnsureLoadEntryScene()
            {
                while (true)
                {
                    SetTintText("Load entry scene.");
                    // 加载入口场景的资源.
                    var entrySceneHandle = entryScene.LoadSceneAsync();

                    while (!entrySceneHandle.IsDone)
                        await UniTask.Yield();

                    if (entrySceneHandle.Status == AsyncOperationStatus.Succeeded)
                        break;
                    await entryScene.UnLoadScene();
                    SetTintText("Load entry scene failed, retry later.");
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));
                }
            }

            // 最后添加加载主场景的逻辑
            initActions.Add(EnsureLoadEntryScene);
        }

        // Start is called before the first frame update
        private async void Start()
        {
            try
            {
                TdReport.Instance.Init();
            }
            catch (Exception e)
            {
                Debug.LogError($"初始化TD失败{e.Message}");
                Debug.LogError(e);
            }

         
#if !UNITY_EDITOR
            // 禁用了 URP 的调试功能, 它会每帧 GC
            UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
#endif
            Application.targetFrameRate = 60;
            //初始化addressable 的url,必须在执行InitAddressable前
            InitAddressableUrl();

            SetInitActions();


            // 执行所有的动作
            for (var i = 0; i < initActions.Count;)
            {
                //等待初始化addressableAction 完成后，再去预加载资源
                if (i == 1)
                {
                    DoInitializablePreLoad();
                }

                var action = initActions[i];
                string actionName = $"{action.Method.DeclaringType?.Name}.{action.Method.Name}";
                try
                {
                    SetTintText($"Initialize action: {actionName}");
                    ReportAction("first_init_start", actionName);
                    await action();
                    ReportAction("first_init_end", actionName);
                    i++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"初始化第 {i} 个动作({actionName})失败: {e}");
                    SetTintText($"Initialize action({actionName}) failed, retry again.");
                    ReportAction("first_init_err", actionName, e.Message);

                    // 等待 0.5 秒再重试
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));
                }
            }

            // 隐藏等待提示
            if (waitingTint != null)
                waitingTint.SetActive(false);
        }

        static void ReportAction(string eventName, string actionName, string errMsg = null)
        {
            var dict = new Dictionary<string, object>
                       {
                           {
                               "actionName", actionName
                           }
                       };
            if (!string.IsNullOrEmpty(errMsg)) dict.Add("errMsg", errMsg);
            TdReport.Report(eventName, dict);
        }

        static void ReportGameSetting(string version, string settingAddress)
        {
            var dict = new Dictionary<string, object>
                       {
                           {
                               "gameversion", version
                           },
                           {
                               "address", settingAddress
                           }
                       };
            TdReport.Report("game_setting", dict);
        }

        void DoInitializablePreLoad()
        {
            ClearPreHandlerList();
            for (int i = 0; i < preLoadPathList.Count; i++)
            {
                var path = preLoadPathList[i];
                try
                {
                    if (string.IsNullOrEmpty(path))
                        continue;
                    
                    var handler = Addressables.LoadAssetAsync<object>(path);
                    handlerList.Add(handler);
                    
                }
                catch (Exception e)
                {
                    Debug.LogError($"preload {path} throw exception {e.Message}");
                }
            }
        }

        void ClearPreHandlerList()
        {
            if (handlerList == null)
                return;
            for (int i = 0; i < handlerList.Count; i++)
            {
                var handler = handlerList[i];
                if (handler.IsValid())
                {
                    Addressables.Release(handler);
                }
            }

            handlerList.Clear();
        }

        /// <summary>
        ///  设置等待提示, 只支持英文字符, 数字, 标点符号等, 中文显示不出来
        /// </summary>
        /// <param name="tint">指定要设置的提示</param>
        private void SetTintText(string tint)
        {
            if (waitingText == null)
                return;
            waitingText.text = tint;
        }

        // 定义获取资源版本号的参数
        public struct GetResourceVerParams
        {
            [JsonProperty("appVer")]
            public int AppVer;

            [JsonProperty("gameId")]
            public string GameId;

            [JsonProperty("uid")]
            public string Uid;

            [JsonProperty("platform")]
            public int Platform;
        }

        /// <summary>
        /// 定义获取资源版本号的返回值
        /// </summary>
        public struct GetResourceVerResult
        {
            /// <summary>
            /// 返回的资源版本号
            /// </summary>
            [JsonProperty("resVer")]
            public int ResVer;

            /// <summary>
            /// 返回 Addressable Asset CDN 的地址
            /// </summary>
            [JsonProperty("aaUrl")]
            public string AAUrl;
        }


        private string settingsPath = string.Empty;

        /// <summary>
        /// 初始化addressable 的地址
        /// </summary>
        void InitAddressableUrl()
        {
            // 本机cdn："http://192.168.31.116:8082"
            var environmentSettings = LaunchSettings.GetCurEnvironmentSettings();
            ResCfg.AAUrl = Path.Combine(environmentSettings.cdnDomain, LaunchSettings.GetCdnSuffix());
            
            ResCfg.AAUrl = Path.Combine(ResCfg.AAUrl, PlatformUtils.BuildTargetName);

            // 来自于 AddressablesImpl.InitializeAsync() 函数的实现
#if UNITY_EDITOR
            settingsPath = PlayerPrefs.GetString(Addressables.kAddressablesRuntimeDataPath, Addressables.RuntimePath + "/settings.json");
            // 这段代码来自于 UnityEngine.AddressableAssets.AddressablesImpl.ResolveInternalId() 函数
            settingsPath = AddressablesRuntimeProperties.EvaluateString(settingsPath);
#else
            settingsPath = Path.Combine(ResCfg.AAUrl, $"settings_{GameVersion.GameVer}.json");
#endif
            //errorText.text = $"[FirstScreen] {GameVersion.GameVer} Addressable 加载地址: {settingsPath}, CDN 地址: {ResCfg.AAUrl}";
            Debug.Log($"[FirstScreen] Addressable 加载地址: {settingsPath}, CDN 地址: {ResCfg.AAUrl}");
            string address = $"settingPath:{settingsPath} cdn:{ResCfg.AAUrl}";
            ReportGameSetting(GameVersion.GameVer, address);
        }

        /// <summary>
        /// 初始化 Addressable
        /// </summary>
        private async UniTask InitAddressable()
        {
            // 这个函数加载失败后会自带清理功能, 可以重入, 否则 Addressables 第一次加载失败后会一直返回加载中
            await InitializeAsync(settingsPath);
        }

        private void OnDestroy()
        {
            ClearPreHandlerList();
        }

        /// <summary>
        /// 封装的通过反射来调用 Addressables.InitializeAsync() 方法, 因为它没有暴露出来我们想用的函数
        /// </summary>
        /// <param name="settingPath">指定 settings.json 的路径</param>
        /// <returns></returns>
        private async UniTask InitializeAsync(string settingPath)
        {
#if UNITY_EDITOR
            Addressables.Log("强制先创建 addressablesImpl 的实例, 只在编辑器中运行");
#endif
            Debug.Log("### settingsPath " + settingPath);

            // 通过反射获取对 AddressablesImpl 的访问
            var instanceField = typeof(Addressables).GetField("m_AddressablesInstance", BindingFlags.Static | BindingFlags.NonPublic)!;
            var addressablesInstance = instanceField.GetValue(null);
            var addressablesImplType = addressablesInstance.GetType();

            // 参数类型
            Type[] parameterTypes =
            {
                typeof(string), typeof(string), typeof(bool)
            };
            // 获取重载版本
            var initializeAsyncMethod = addressablesImplType.GetMethod("InitializeAsync", BindingFlags.Instance | BindingFlags.Public, null, parameterTypes, null)!;

            // 准备调用 InitializeAsync() 方法的参数
            object[] parameters =
            {
                settingPath, null, true
            };

            try
            {
                var result = new UniTaskCompletionSource();
                // 调用 InitializeAsync() 方法
                var handler = (AsyncOperationHandle<IResourceLocator>)initializeAsyncMethod.Invoke(addressablesInstance, parameters);
                // 直接await或者while IsDone 都会报错：handler是无效句柄
                handler.Completed += handle =>
                {
                    if (handler.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.Log($"### aa 初始化失败 {handler.OperationException}");

                        if (handle.OperationException != null)
                        {
                            var innerException1 = handle.OperationException.InnerException;
                            if (innerException1 != null && !string.IsNullOrEmpty(innerException1.Message))
                            {
                                string error = innerException1.Message;
                                if (innerException1.InnerException != null && !string.IsNullOrEmpty(innerException1.InnerException.Message))
                                {
                                    error += innerException1.InnerException.Message;
                                }

                                ReportAction("first_init_err", "FirstScreenComponent.InitAddressable", error);
                            }
                        }

                        result.TrySetException(handle.OperationException);
                    }

                    result.TrySetResult();
                };
                await result.Task;
            }
            catch (Exception e)
            {
                // 初始化失败, 我们需要重置一下 Addressables, 否则下次就算网络好了, 它也不会再次初始化
                var constructor = addressablesImplType.GetConstructor(new[]
                                                                      {
                                                                          typeof(IAllocationStrategy)
                                                                      })!;

                var newAddressablesImpl = constructor.Invoke(new object[]
                                                             {
                                                                 new LRUCacheAllocationStrategy(1000, 1000, 100, 10)
                                                             });

                instanceField.SetValue(null, newAddressablesImpl);
                throw;
            }
        }
    }
}
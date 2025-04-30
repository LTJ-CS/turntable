using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UIFramework.Base;
using UIFramework.UIScreen;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace UIFramework
{
    public struct OpenCallbackStruct
    {
        public Action<OpenPresenterFailResult> fail;
    }
    
    public enum OpenPresenterFailResult
    {
        ScreenLoaderNotExist = 0,
        ScreenReopenError = 1,
        ScreenLoadFailed = 2,
    }
    /// <summary>
    /// UI 管理器, 管理 Screen 等各个窗口的加载, 打开, 关闭等逻辑
    /// </summary>
    /// <remarks>
    /// 按目前的设计, 每个 Layer 只能打开一个 Screen, 即打开一个新的 Screen 时必然会导致关闭这个 Layer 当前的 Screen
    /// </remarks>
    public partial class UIManager
    {
        /// <summary>
        /// 获取 UI 管理器的唯一实例 
        /// </summary>
        private static UIManager Instance { get; set; }

        /// <summary>
        /// UI 的根节点, 永远不会被销毁
        /// </summary>
        private UIRootComponent _uiRoot;

        /// <summary>
        /// 返回 UIManager 是否有效, 只在编辑器中使用
        /// </summary>
        public static bool IsValid =>
#if UNITY_EDITOR
            Instance != null;
#else
            true;
#endif

        /// <summary>
        /// 保存每层的 UI 的管理器
        /// </summary>
        private readonly UILayer[] _uiLayers = Enumerable.Range(0, (int)EUILayer.Count)
                                                         .Select(i => new UILayer())
                                                         .ToArray();

        /// <summary>
        /// 存储正在使用中(已经加载或加载中)的 ScreenLoader
        /// </summary>
        private readonly Dictionary<Type, UIScreenLoader> _usingScreenLoaders = new();

        /// <summary>
        /// 封装 UI 操作, 用于延迟进行 UI 的操作, 以解决递归访问问题, 即在 OnOpen() 或 OnClose() 中再次调用 OpenScreen() 或 CloseScreen() 函数
        /// </summary>
        private class UIActionInfo
        {
            public Type   PresenterType;
            public Action Operation;
            public int    OperateCount;
        }

        /// <summary>
        /// 缓冲的 UI 操作的列表, 交的列表
        /// </summary>
        private readonly List<UIActionInfo>[] _uiActionInfoSwapLists =
        {
            new(),
            new(),
        };

        /// <summary>
        /// 当前在使用的 UI 操作的缓冲列表索引
        /// </summary>
        private int _uiActionInfosCurrentIndex;

        /// <summary>
        /// 获取当前索引处的UI操作信息列表。
        /// </summary>
        private List<UIActionInfo> UIActionInfos => _uiActionInfoSwapLists[_uiActionInfosCurrentIndex];

        /// <summary>
        /// UI 操作的协程, 用于识别
        /// </summary>
        private Coroutine _uiActionCoroutine;

        private UIManager()
        {
            // 初始化每层的子层, 目前手动添加, 更加准确
            _uiLayers[(int)EUILayer.Screen].AddChildLayer(EUILayer.Screen_1);
            _uiLayers[(int)EUILayer.Screen].AddChildLayer(EUILayer.Screen_2);
            _uiLayers[(int)EUILayer.Screen].AddChildLayer(EUILayer.Screen_3);
        }

        /// <summary>
        /// 初始化 UIManager, 执行完成后才能访问 UIManager.Instance, UIManager.OpenScreen() 等后续逻辑, 切记切记!!!
        /// </summary>
        /// <typeparam name="UIFrameworkSettingsType">指定使用的 UIFrameworkSettings 的类型</typeparam>
        /// <remarks>在调用本函数前, 必须保证 Addressables 已经初始化完毕</remarks>
        public static async UniTask Init<UIFrameworkSettingsType>()
            where UIFrameworkSettingsType : UIFrameworkSettings, new()
        {
#if UNITY_EDITOR
            if (EditorSettings.enterPlayModeOptionsEnabled && _reinitializeUIManager)
            {
                // 支持编辑器的 enterPlayMode 模式, 即快速进入模式
                _reinitializeUIManager = false;
                Instance = null;
            }
#endif

            Debug.Assert(Instance == null, "UI 管理器实例已经存在, 不支持重复初始化");
            _ = new UIFrameworkSettingsType();
            Instance = new();
            await Instance.InitUIRoot();
        }

        #region 打开 UI 的函数

        /// <summary>
        /// 打开指定类型的 Screen
        /// </summary>
        /// <typeparam name="TScreenPresenter">指定要打开的 Screen Presenter 的类型</typeparam>
        /// <remarks>
        /// <li>注意打开过程是异步的, 最好是在对应的 ScreenPresenter 的 OnOpened 重载函数中做打开 Screen 后的逻辑处理</li>
        /// <li>注意, 如果 Screen 已经处于打开状态, 目前逻辑上会先关闭当前 Screen, 再打开 Screen, 但 ScreenView 对应的 GameObject 并不会被销毁或重置, 复用, 理论上重新打开只需要刷新数据</li> 
        /// <li>理论上并不能保证 Screen 一定会被正常打开, 比如网络一直是断开的, ScreenView 需要的资源下载不下来, 这时会出现重试按钮</li>
        /// </remarks>
        public static void OpenScreen<TScreenPresenter>()
            where TScreenPresenter : UIScreenPresenterBase, new()
        {
            Instance.OpenScreenInternal<TScreenPresenter>(presenter => presenter.OnOpen());
        }
        
        /// <summary>
        ///  打开指定类型的 Screen, 带有一个参数
        /// </summary>
        /// <inheritdoc cref="OpenScreen{TScreenPresenter}"/>
        public static void OpenScreen<TScreenPresenter, P1>(P1 p1)
            where TScreenPresenter : UIScreenPresenterBase, IScreenOpen<P1>, new()
        {
            Instance.OpenScreenInternal<TScreenPresenter>(presenter => presenter.OnOpen(p1));
        }

        /// <summary>
        ///  打开指定类型的 Screen, 带有两个参数
        /// </summary>
        /// <inheritdoc cref="OpenScreen{TScreenPresenter}"/>
        public static void OpenScreen<TScreenPresenter, P1, P2>(P1 p1, P2 p2, OpenCallbackStruct callbackStruct = default)
            where TScreenPresenter : UIScreenPresenterBase, IScreenOpen<P1, P2>, new()
        {
            Instance.OpenScreenInternal<TScreenPresenter>(presenter => presenter.OnOpen(p1, p2), callbackStruct);
        }

        /// <summary>
        ///  打开指定类型的 Screen, 带有三个参数
        /// </summary>
        /// <inheritdoc cref="OpenScreen{TScreenPresenter}"/>
        public static void OpenScreen<TScreenPresenter, P1, P2, P3>(P1 p1, P2 p2, P3 p3, OpenCallbackStruct callbackStruct = default)
            where TScreenPresenter : UIScreenPresenterBase, IScreenOpen<P1, P2, P3>, new()
        {
            Instance.OpenScreenInternal<TScreenPresenter>(presenter => presenter.OnOpen(p1, p2, p3), callbackStruct);
        }

        /// <summary>
        ///  打开指定类型的 Screen, 带有四个参数
        /// </summary>
        /// <inheritdoc cref="OpenScreen{TScreenPresenter}"/>
        public static void OpenScreen<TScreenPresenter, P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4, OpenCallbackStruct callbackStruct = default)
            where TScreenPresenter : UIScreenPresenterBase, IScreenOpen<P1, P2, P3, P4>, new()
        {
            Instance.OpenScreenInternal<TScreenPresenter>(presenter => presenter.OnOpen(p1, p2, p3, p4), callbackStruct);
        }

        /// <summary>
        ///  打开指定类型的 Screen, 带有五个参数
        /// </summary>
        /// <inheritdoc cref="OpenScreen{TScreenPresenter}"/>
        public static void OpenScreen<TScreenPresenter, P1, P2, P3, P4, P5>(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, OpenCallbackStruct callbackStruct = default)
            where TScreenPresenter : UIScreenPresenterBase, IScreenOpen<P1, P2, P3, P4, P5>, new()
        {
            Instance.OpenScreenInternal<TScreenPresenter>(presenter => presenter.OnOpen(p1, p2, p3, p4, p5), callbackStruct);
        }
        
        /// <summary>
        ///  打开指定类型的 Screen, 返回加载回调
        /// </summary>
        /// <param name="openCallbackStruct">加载界面回调的结构，目前只有加载失败回调，使用需要慎重，是异步返回</param>
        /// <typeparam name="TScreenPresenter"></typeparam>
        public static void OpenScreen<TScreenPresenter>(OpenCallbackStruct openCallbackStruct)
            where TScreenPresenter : UIScreenPresenterBase, new()
        {
            Instance.OpenScreenInternal<TScreenPresenter>(presenter => presenter.OnOpen(),openCallbackStruct);
        }
        /// <summary>
        ///  打开指定类型的 Screen, 带有一个参数,且返回加载回调
        /// </summary>
        /// <param name="openCallbackStruct">加载界面回调的结构，目前只有加载失败回调，使用需要慎重，是异步返回</param>
        /// <inheritdoc cref="OpenScreen{TScreenPresenter}"/>
        public static void OpenScreen<TScreenPresenter, P1>(P1 p1,OpenCallbackStruct openCallbackStruct)
            where TScreenPresenter : UIScreenPresenterBase, IScreenOpen<P1>, new()
        {
            Instance.OpenScreenInternal<TScreenPresenter>(presenter => presenter.OnOpen(p1), openCallbackStruct);
        }

        private void OpenScreenInternal<TScreenPresenter>(Action<TScreenPresenter> onOpen,OpenCallbackStruct callbackStruct = default)
            where TScreenPresenter : UIScreenPresenterBase, new()
        {
            if (IsPlayingModeExit)
                return;

            void DelayLoad()
            {
                var screenLoader = GetOrCreateScreenLoader(onOpen,callbackStruct);
                if (screenLoader == null)
                {
                    Debug.LogError($"无法打开 Screen: {typeof(TScreenPresenter).Name}, 因为 ScreenLoader 没有被创建");
                }
            }

            // 把这个操作压入到本帧的操作列表中, 记录操作, 延迟执行, 固定在特定的地方, 以解决在 OnOpen, OnClose() 等函数再次调用 Open与Close()的问题
            PushToActionList(typeof(TScreenPresenter), DelayLoad, 1);
        }

        #endregion

        #region 关闭 Screen

        /// <summary>
        /// 关闭指定类型的 Screen
        /// </summary>
        /// <typeparam name="TScreenPresenter">指定要关闭的 ScreenPresenter 类型</typeparam>
        /// <returns>如果窗口有关闭动画, 表示关闭动画结束时调用</returns>
        public static void CloseScreen<TScreenPresenter>()
            where TScreenPresenter : UIScreenPresenterBase, new()
        {
            Instance.CloseScreenInternal<TScreenPresenter>();
        }

        private void CloseScreenInternal<TScreenPresenter>()
            where TScreenPresenter : UIScreenPresenterBase, new()
        {
            Instance.CloseScreenInternal(typeof(TScreenPresenter));
        }

        /// <summary>
        /// 直接关闭指定实例的 Screen, 只用于内部调用
        /// </summary>
        /// <param name="screenPresenter">指定要关闭的 ScreenPresenter</param>
        internal static void CloseScreen(UIScreenPresenterBase screenPresenter)
        {
            Instance.CloseScreenInternal(screenPresenter);
        }

        private void CloseScreenInternal(UIScreenPresenterBase screenPresenter)
        {
            CloseScreenInternal(screenPresenter.GetType());
        }

        /// <summary>
        /// 关闭指定类型的 Screen
        /// </summary>
        /// <param name="presenterType">指定要关闭的 ScreenPresenter 类型</param>
        private void CloseScreenInternal(Type presenterType)
        {
            if (IsPlayingModeExit)
            {
                return;
            }

            void DelayClose()
            {
                var screenLoader = GetScreenLoader(presenterType);
                if (screenLoader == null)
                    return;

                if (screenLoader.IsClosed)
                    return; // 窗口已经被关闭了，忽略此次关闭处理
                UIScreenLoader bePopLoadingLoader = null;
                // 检测数据是否符合设计
                {
                    var uiLayer = _uiLayers[(int)screenLoader.Layer];
                    var currentScreen = uiLayer.TopScreen();
                    if (currentScreen != screenLoader)
                    {
                        if (!currentScreen.IsLoading)
                        {
                            Debug.LogError($"关闭 Screen({screenLoader.gameObject.name}) 时, 当前 Screen({currentScreen.gameObject.name}) 不是 ScreenLoader 所对应的 Screen, 严重 Bug!!!");
                            Debug.DebugBreak();
                            return;
                        }
                        else
                        {
                            // 新的窗口在加载中
                            uiLayer.PopScreen();
                            bePopLoadingLoader = currentScreen;
                        }
                    }
                }

                // 关闭 UI Screen. TODO: 考虑是否需要延迟卸载
                CloseLayerCurrentScreen(screenLoader.Layer);

                if (bePopLoadingLoader != null)
                {
                    // 添加 Screen Loader 到 UI 层级
                    _uiLayers[(int)bePopLoadingLoader.Layer].PushScreen(bePopLoadingLoader);
                }
            }

            // TODO: 也许应该先禁止这个要关闭的 Screen 的交互, 因为它将被关闭了???

            PushToActionList(presenterType, DelayClose, -1);
        }

        /// <summary>
        /// 关闭指定层的当前的 Screen
        /// </summary>
        /// <param name="screenPresenterLayer"></param>
        private void CloseLayerCurrentScreen(EUILayer screenPresenterLayer)
        {
            // 获取指定层的当前 Screen
            var uiLayer = _uiLayers[(int)screenPresenterLayer];
            var currentScreen = uiLayer.PopScreen(); // TODO: 支持基于栈的 Screen 关闭
            if (currentScreen == null)
                return;

            // 关闭 Screen
            try
            {
                currentScreen.Close().Forget();
            }
            catch (Exception ex)
            {
                Debug.LogError($"关闭 Screen ({currentScreen.gameObject.name} : {currentScreen.PresenterType}) 时出现错误, 异常: {ex}");
            }

            // 当前直接卸载了, TODO: 也许以后会实现支持延迟卸载的 Screen, 以优化性能
            _uiRoot.ReleaseScreenLoader(currentScreen);

            // 自动关闭它的子层
            foreach (var childLayer in uiLayer.ChildLayers)
            {
                CloseLayerCurrentScreen(childLayer);
            }
        }

        #endregion

        /// <summary>
        /// 是否退出了 Playing 模式
        /// </summary>
        /// <remarks>// 在编辑器模式下, 如果在加载一个场景的过程中退出 Playing 模式会导致 加载的 Scene 初始化, 触发场景的一些逻辑, 但这时 UISystem 已经被销毁了 </remarks>
        private bool IsPlayingModeExit => _uiRoot == null;

        /// <summary>
        /// 把给定的 UI 动作压入到延后操作列表中, 记录操作, 延迟执行, 以解决在 OnOpen, OnClose() 等函数再次调用 Open与Close()的问题
        /// </summary>
        /// <param name="screenPresenterType">要打开或关闭的 ScreenPresenter 的类型</param>
        /// <param name="delayLoad">延迟执行的操作</param>
        /// <param name="operateInc">操作计数, 如果为正数, 则表示打开, 如果为负表示关闭, 为0表示无操作</param>
        private void PushToActionList(Type screenPresenterType, Action delayLoad, int operateInc)
        {
            StartUIActionCoroutine();

            // 把这个操作压入到本帧的操作列表中, 记录操作, 延迟执行, 固定在特定的地方, 以解决在 OnOpen, OnClose() 等函数再次调用 Open与Close()的问题
            UIActionInfo uiActionInfo = null;
            for (var i = 0; i < UIActionInfos.Count; i++)
            {
                var t = UIActionInfos[i];
                if (t.PresenterType == screenPresenterType)
                {
                    uiActionInfo = t;
                    uiActionInfo.OperateCount += operateInc;
                    uiActionInfo.OperateCount = Math.Clamp(uiActionInfo.OperateCount, -1, 1); // 确保不会出现两次以上的打开或关闭
                    uiActionInfo.Operation = delayLoad;

                    if (i < UIActionInfos.Count - 1)
                    {
                        // 把本操作放置到最后面, 因为它是新的操作
                        UIActionInfos.RemoveAt(i);
                        UIActionInfos.Add(uiActionInfo);
                    }

                    return;
                }
            }

            uiActionInfo = new UIActionInfo
                           {
                               PresenterType = screenPresenterType,
                               OperateCount = operateInc,
                               Operation = delayLoad
                           };
            UIActionInfos.Add(uiActionInfo);
        }

        private void StartUIActionCoroutine()
        {
            if (_uiActionCoroutine != null)
                return;

            _uiActionCoroutine = _uiRoot.StartCoroutine(UIActionCoroutine());
        }

        /// <summary>
        /// 减少 GC
        /// </summary>
        private readonly WaitForEndOfFrame _waitForEndOfFrame = new();

        /// <summary>
        /// 真正的执行 UI 打开与关闭操作的函数
        /// </summary>
        /// <returns></returns>
        private IEnumerator UIActionCoroutine()
        {
            // 等待到帧结束再执行
            yield return _waitForEndOfFrame;
            var tempActionInfos = _uiActionInfoSwapLists[_uiActionInfosCurrentIndex];
            // 切换到下一个列表
            _uiActionInfosCurrentIndex = (_uiActionInfosCurrentIndex + 1) % _uiActionInfoSwapLists.Length;

            foreach (var uiActionInfo in tempActionInfos)
            {
                if (uiActionInfo.OperateCount != 0)
                    uiActionInfo.Operation();
            }

            UIActionInfos.Clear();
            _uiActionCoroutine = null;
        }

        /// <summary>
        /// 获取指定类型的 UI Presenter 对应的加载器
        /// </summary>
        /// <param name="onOpen">当加载 View 成功后调用</param>
        /// <param name="callbackStruct">加载界面回调的结构，目前只有加载失败回调，使用需要慎重，是异步返回</param>
        /// <typeparam name="TScreenPresenter">指定获取的 Screen Presenter 的类型</typeparam>
        /// <returns></returns>
        private UIScreenLoader GetOrCreateScreenLoader<TScreenPresenter>(Action<TScreenPresenter> onOpen,OpenCallbackStruct callbackStruct)
            where TScreenPresenter : UIScreenPresenterBase, new()
        {
            // 加载完 Screen 后, 在打开 Screen 前我们需要做些处理
            Action<UIScreenLoader, TScreenPresenter> onPreOpenScreen = (loader, screenPresenter) =>
            {
                // 这里要先移除本 Screen, 关闭下面的 Screen 后再添加回去
                var topScreenLoader = _uiLayers[(int)screenPresenter.Layer].PopScreen();
                if (topScreenLoader != loader)
                {
                    // 顶层竟然不是当前要打开的 Screen???
                    Debug.LogError($"加载完成要真正显示 Screen 时, 发现顶层 ScreenLoader 不是当前要打开的 ScreenLoader, 这是一个严重错误!!!");
                    return;
                }

                // 先关闭本层当前的 Screen
                CloseLayerCurrentScreen(screenPresenter.Layer);

                // 添加 Screen Loader 到 UI 层级
                _uiLayers[(int)screenPresenter.Layer].PushScreen(loader);

                // 调用 ScreenPresenter 对应的 OnOpen() 函数
                onOpen!(screenPresenter);
            };

            // 如果当前顶部的UI还在加载中，则直接移除它
            void TryRemoveLoadingScreenInLayer(EUILayer toCheckLayer)
            {
                var topScreenLoader = _uiLayers[(int)toCheckLayer].TopScreen();
                if (topScreenLoader == null || topScreenLoader.State != UIScreenLoader.EState.Loading)
                    return;
                _uiLayers[(int)toCheckLayer].PopScreen();
                _uiRoot.ReleaseScreenLoader(topScreenLoader);
            }

            // 查看 UIScreenLoader 是否存在
            var screenLoader = GetScreenLoader<TScreenPresenter>();
            if (screenLoader != null)
            {
                var topScreenLoader = _uiLayers[(int)screenLoader.Layer].TopScreen();
                if (topScreenLoader != null && screenLoader != topScreenLoader)
                {
                    Debug.LogError($"[UIManager] 打开一个已经被打开的Screen({typeof(TScreenPresenter).Name})时，栈顶的 Screen ({topScreenLoader.PresenterType.Name}) 不正确");
                }
                
                // 有可能在加载中也可能已经加载完成, 需要 screenLoader 来做更详细的判定
                if (screenLoader.IsLoading)
                {
                    // 还在加载中, 替换旧的加载完成函数, 直接忽略上次加载的 onLoaded
                    screenLoader.SetLoadCompleteCallback(onPreOpenScreen);
                }
                else if (screenLoader.State == UIScreenLoader.EState.Opened)
                {
                    // 先关闭当前的 Screen, 再打开, 因为这次可能使用不同的参数来打开的
                    var presenter = screenLoader.GetPresenter<TScreenPresenter>();
                    try
                    {
                        presenter.OnClose();
                        onOpen(presenter);
                    }
                    catch (Exception e)
                    {
                        ExecuteOpenFailCallback(callbackStruct, OpenPresenterFailResult.ScreenReopenError);
                        Debug.LogError($"[UIManager] 关闭再打开 Screen({typeof(TScreenPresenter).Name})时出错: {e.Message}");
                    }
                }
                else
                {
                    throw new NotImplementedException($"不支持的 ScreenLoader 状态: {screenLoader.State}");
                }

                return screenLoader;
            }

            // 优先在缓冲中查找 ScreenLoader
            screenLoader = _uiRoot.QueryCachedScreenLoader<TScreenPresenter>();
            if (Equals(screenLoader, null))
            {
                var screenPresenter = UIScreenPresenterBase.Builder.Build<TScreenPresenter>();
                TryRemoveLoadingScreenInLayer(screenPresenter.Layer);

                // 申请一个新的 Screen 加载器
                screenLoader = _uiRoot.QueryScreenLoader(screenPresenter.Layer);

#if UNITY_EDITOR
                screenLoader.gameObject.name = $"Screen Loader [{typeof(TScreenPresenter).Name}]";
#endif
                // 添加 Screen Loader 到 UI 层级. 加载中的 UI Screen 也算是打开的 UI Screen, 只是某些逻辑还没有执行到, 这样管理会比较统一!
                _uiLayers[(int)screenPresenter.Layer].PushScreen(screenLoader);

                // 开始加载 UI Screen View
                screenLoader.Load(onPreOpenScreen, screenPresenter,OnScreenLoadFail);
            }
            else
            {
                TryRemoveLoadingScreenInLayer(screenLoader.Layer);
                
                // 使用缓冲的 ScreenLoader
                // 添加 Screen Loader 到 UI 层级. 加载中的 UI Screen 也算是打开的 UI Screen, 只是某些逻辑还没有执行到, 这样管理会比较统一!
                _uiLayers[(int)screenLoader.Layer].PushScreen(screenLoader);

                // 开始加载 UI Screen View
                screenLoader.Load(onPreOpenScreen, null,OnScreenLoadFail);
            }

            if (screenLoader == null)
            {
                ExecuteOpenFailCallback(callbackStruct,OpenPresenterFailResult.ScreenLoaderNotExist);
            }
            return screenLoader;
            
            void OnScreenLoadFail()
            { 
                ExecuteOpenFailCallback(callbackStruct,OpenPresenterFailResult.ScreenLoadFailed);
            }
        }

         void ExecuteOpenFailCallback(OpenCallbackStruct callbackStruct,OpenPresenterFailResult failResult)
        {
            try
            {
                callbackStruct.fail?.Invoke(failResult);
            }
            catch (Exception e)
            {
               Debug.LogError($"[UIManager] ExecuteOpenFailCallback throw exception {e}");
            }
        }

        /// <summary>
        /// 获取指定类型的 UI Presenter 对应的加载器
        /// </summary>
        /// <typeparam name="TScreenPresenter">指定获取的 Screen Presenter 的类型</typeparam>
        /// <returns></returns>
        [CanBeNull]
        private UIScreenLoader GetScreenLoader<TScreenPresenter>()
        {
            return GetScreenLoader(typeof(TScreenPresenter));
        }

        /// <summary>
        /// 查找指定类型的 UI Presenter 对应的加载器
        /// </summary>
        /// <param name="screenPresenterType">指定获取加载器的 Screen Presenter 的类型</param>
        /// <returns></returns>
        [CanBeNull]
        private UIScreenLoader GetScreenLoader(Type screenPresenterType)
        {
            // 因为同时存在的 UI 不应该太多, 所以简单直接的线性查找性能并不是问题
            UIScreenLoader retLoader = null;
            foreach (var uiLayer in _uiLayers)
            {
                retLoader = uiLayer.ScreenLoaders.SingleOrDefault(loader => loader.PresenterType == screenPresenterType);
                if (retLoader != null)
                    break;
            }

            return retLoader;
        }
    }
}
using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UIFramework.Base;
using UIFramework.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

// ReSharper disable once CheckNamespace
namespace UIFramework.UIScreen
{
    /// <summary>
    /// UI Screen 的加载器, 同时也一个 UI Screen View 的父节点.
    /// </summary>
    /// <remarks>
    /// AUIScreenLoader 同时也会阻止在加载 UI View 的过程中, UI 点击的透过操作
    /// 本类要特别处理如果 UI 还在加载中时, UI 被关闭或 UI 被重新打开时的情况. 对于上层来讲, UI 的打开与关闭总是是同步的.
    /// </remarks>
    internal class UIScreenLoader : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        [Tooltip("背景")]
        private RawImage m_Background;

        [SerializeField]
        [Tooltip("用于显示加载中信息的 Text")]
        private TextMeshProUGUI m_LoadingText; // TODO: Screen 资源绝大部分时候不会加载超时, 也不会加载失败, 所以这个 Text 可能可以动态创建, 减少消耗???

        [SerializeField]
        [Tooltip("加载失败时强制关闭的按钮")]
        private Button m_ForceCloseButton; // TODO: Screen 资源绝大部分时候不会加载超时, 也不会加载失败, 所以这个 Text 可能可以动态创建, 减少消耗???

        /// <summary>
        /// 用于监控加载是否超时的 Tween
        /// </summary>
        private Sequence _loadTimeoutTween;

        /// <summary>
        /// Screen 的状态
        /// </summary>
        public enum EState
        {
            None,     // 无效值
            Loading,  // 正在加载中
            Retrying, // 正在重试中, 由于资源加载失败导致
            Opened,   // 窗口处于打开状态
            Closed    // 窗口牌关闭状态
        }

#if UNITY_EDITOR
        [SerializeField] [ReadOnly] [Tooltip("UI 的当前状态")]
#else
        [NonSerialized]
#endif
        private EState m_State = EState.None;

        public EState State => m_State;

        /// <summary>
        /// 是否是加载中的状态
        /// </summary>
        public bool IsLoading => m_State == EState.Loading || m_State == EState.Retrying;

        /// <summary>
        /// 是否是打开状态
        /// </summary>
        public bool IsOpened => m_State == EState.Opened;

        /// <summary>
        /// 是否是关闭状态
        /// </summary>
        public bool IsClosed => m_State == EState.Closed;

        /// <summary>
        /// 保存资源加载的句柄
        /// </summary>
        private AsyncOperationHandle<GameObject> _loadOperationHandle;

        /// <summary>
        /// 保存 ScreenView 对应的 GameObject
        /// </summary>
        private GameObject _screenViewGameObject;

        /// <summary>
        /// 保存对应的 UI Screen Presenter
        /// </summary>
        private UIScreenPresenterBase _screenPresenter;

        public UIScreenPresenterBase ScreenPresenter => _screenPresenter;
        
        /// <summary>
        /// 保存 ScreenView 的 Behaviour
        /// </summary>
        private ScreenBehaviour _screenBehaviour;

        public T GetPresenter<T>() where T : UIScreenPresenterBase => _screenPresenter as T;

        /// <summary>
        /// UI Screen 的层级
        /// </summary>
        public EUILayer Layer => _screenPresenter.Layer;

        /// <summary>
        /// UI Screen Presenter 的类型
        /// </summary>
        public Type PresenterType => _screenPresenter.GetType();

        /// <summary>
        /// 保存加载完成后需要调用的回调, 一般用于调用 UI Screen Presenter 的 Open 方法. 如果多次设置, 只保留最后的一个
        /// </summary>
        private Action<UIScreenPresenterBase> _loadCompleteCallback;
        
        /// <summary>
        /// 加载失败后且不重试加载，返回的回调，多次设置，只保留最后一个
        /// </summary>
        private Action _loadFailCallback;
        /// <summary>
        /// 保存加载结果
        /// </summary>
        /// private UniTaskCompletionSource<bool> _completionSource;
        /// <summary>
        /// Return a Task object to wait on when using async await.
        /// </summary>
        /// public UniTask<bool> Task => _completionSource.Task;
        /// <summary>
        /// 加载给定类型的 UI Screen View 及其 Presenter
        /// </summary>
        /// <param name="loadedCallback">加载成功后调用的函数</param>
        /// <param name="screenPresenter">
        /// 指定本 ScreenLoader 对应的 UI Screen Presenter, 之所以要传入是为了在创建 ScreenLoader 之前就要先获取到 Screen 一些加载信息,
        /// 如 Layer 等, 以便把本 Loader 放置到合理的层级上
        /// </param>
        /// <param name="loadFailCallback">加载失败的回调，如果重新尝试加载，不调用该回调</param>
        /// <typeparam name="TScreenPresenter">指定要加载的UI Screen Presenter 类型, 为 null 表示复用</typeparam>
        /// <returns></returns>
        public void Load<TScreenPresenter>(Action<UIScreenLoader, TScreenPresenter> loadedCallback, TScreenPresenter screenPresenter = null,Action loadFailCallback = null)
            where TScreenPresenter : UIScreenPresenterBase, new()
        {
            LogFormat("[UIScreenLoader] 开始加载 Screen: {0}", screenPresenter?.ScreenName ?? _screenPresenter.ScreenName);

            // 设置加载完成后的回调
            SetLoadCompleteCallback(loadedCallback);
            //设置加载失败后的回调
            _loadFailCallback = loadFailCallback;

            // 考虑延迟卸载, 复用的情况
            if (m_State == EState.Closed)
            {
                Debug.Assert(_screenPresenter != null);
                // 已经是加载过资源的状态
                if (_loadOperationHandle.IsDone)
                {
                    // 资源已经加载完成, 直接调用回调吧
                    LoadOperationHandleOnCompleted(_loadOperationHandle);
                }

                // 修改状态为加载中, 等待加载结束
                m_State = EState.Loading;
                return;
            }

            if (m_State != EState.None)
            {
                Debug.LogError($"{nameof(UIScreenLoader)} 的状态不正确: {m_State}");
            }

            Debug.Assert(!_loadOperationHandle.IsValid()); // Load 与 Unload 不匹配???

            m_State          = EState.Loading;
            _screenPresenter = screenPresenter;

            // 第一次加载, 禁用错误提示
            m_LoadingText.gameObject.SetActive(false);
            // 加载资源
            LoadScreenAsset();
        }

        /// <summary>
        /// 加载 _screenPresenter 对应的 Screen 资源
        /// </summary>
        /// <returns></returns>
        private void LoadScreenAsset()
        {
            if (_loadOperationHandle.IsValid())
            {
                Addressables.Release(_loadOperationHandle);
            }

            m_ForceCloseButton.gameObject.SetActive(false);
            m_ForceCloseButton.onClick.RemoveAllListeners();

            var screenName = _screenPresenter.ScreenName;
            var screenViewPrefabPath = UIFrameworkSettings.GetScreenViewPrefabPath(screenName);
            _loadOperationHandle = Addressables.LoadAssetAsync<GameObject>(screenViewPrefabPath);
            if (_loadOperationHandle.IsDone) // 资源已经加载完成
                LoadOperationHandleOnCompleted(_loadOperationHandle);
            else // 资源还在加载中
            {
                if (_screenPresenter.BackgroundShowType != EBackgroundShowType.None)
                {  // 阻止窗口在打开前被玩家操作关闭, 或者在加载过程会触发到其它 UI 的事件
                    BlockInput();
                }
                // 初始时背景为全透明, 避免出现背景先出现, 然后窗口后出现的问题
                m_Background.color = new Color(0, 0, 0, 0);
                
                // 开起加载超时的监督, 卫兵
                {
                    _loadTimeoutTween?.Kill();

                    _loadTimeoutTween = DOTween.Sequence();
                    _loadTimeoutTween.AppendInterval(2);
                    _loadTimeoutTween.AppendCallback(StartLoadingAnimation);
                }

                if (true)
                {
                    _loadOperationHandle.Completed += LoadOperationHandleOnCompleted;
                }
                else
                {
                    // 测试窗口加载延迟的逻辑
                    // _loadOperationHandle.Completed += async (handle) =>
                    // {
                    //     await UniTask.WaitForSeconds(5);
                    //     LoadOperationHandleOnCompleted(handle);
                    // };
                }
            }
        }

        /// <summary>
        /// 设置加载 UI Screen View 完成后的回调
        /// </summary>
        public void SetLoadCompleteCallback<TScreenPresenter>(Action<UIScreenLoader, TScreenPresenter> loadedCallback)
            where TScreenPresenter : UIScreenPresenterBase
        {
            void TempLoadCallback(UIScreenPresenterBase screenPresenter)
            {
                Debug.Assert(screenPresenter is TScreenPresenter);
                loadedCallback(this, (TScreenPresenter)screenPresenter);
            }

            _loadCompleteCallback = TempLoadCallback;
        }

        /// <summary>
        /// 打开 UI Screen View, 只有在确定资源加载完成的情况才可以调用
        /// </summary>
        private void Open()
        {
            Debug.Assert(_loadCompleteCallback != null);
            if (m_State == EState.Opened)
            {
                Debug.LogError($"{name} 已经处于打开状态, 无需重复打开");
                return;
            }

            // 激活 GameObject
            gameObject.SetActive(true);

            _loadCompleteCallback(_screenPresenter);
            _loadCompleteCallback = null;

            m_State = EState.Opened;

            {
                // 根据 Presenter 的属性来设置背景的显示
                var showType = _screenPresenter.BackgroundShowType;
                var backgroundColor = _screenPresenter.BackgroundColor;
                switch (showType)
                {
                    case EBackgroundShowType.None:
                    case EBackgroundShowType.FullyTransparent:
                        m_Background.color = new Color(0, 0, 0, 0);
                        break;
                    case EBackgroundShowType.CustomColor:
                        m_Background.color = backgroundColor;
                        break;
                    case EBackgroundShowType.SemiTransparent:
                        m_Background.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, ScreenBehaviour.SemiTransparentAlpha);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"无效的背景显示类型: {showType}");
                }
            }
            {
                // 根据背景的点击事件类型来处理处理情况
                _screenBehaviour = _screenViewGameObject.GetComponent<UIScreenViewBase>().ScreenBehaviour;
                if (_screenBehaviour != null) // 判null 是为了兼容旧版本的 Screen 导出
                {
                    switch (_screenBehaviour.BackgroundClickEventType)
                    {
                        case EBackgroundClickEventType.PassThrough:
                            m_Background.raycastTarget = false;
                            break;
                        case EBackgroundClickEventType.CloseSelf:
                        case EBackgroundClickEventType.BlockWithoutResponse:
                        case EBackgroundClickEventType.CustomCallback:
                            m_Background.raycastTarget = true;
                            break;
                        
                        default:
                            throw new ArgumentOutOfRangeException($"({_screenPresenter.ScreenName})无效的背景点击事件类型: {_screenBehaviour.BackgroundClickEventType}");
                    }
                }
            }
        }

        /// <summary>
        /// 关闭 UI Screen View
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal UniTask Close()
        {
            // 关闭时自动停止加载动画
            StopLoadingAnimation();

            switch (m_State)
            {
                case EState.Opened: // 资源加载完成, 窗口处于打开状态, 直接关闭
                    m_State = EState.Closed;
                    _screenPresenter.OnClose();
                    _screenPresenter.OnPostClose();
                    gameObject.SetActive(false);
                    break;

                case EState.Loading: // 窗口处于加载中, 停止加载
                case EState.Retrying:
                {
                    m_State = EState.Closed;
                    // 取消加载完成的回调
                    _loadOperationHandle.Completed -= LoadOperationHandleOnCompleted;

                    gameObject.SetActive(false);
                    break;
                }
                case EState.Closed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return UniTask.FromResult(true); // TODO: 等待实现关闭动画的结束通知
        }

        /// <summary>
        /// 阻止输入
        /// </summary>
        private void BlockInput()
        {
            m_Background.raycastTarget = true;
            m_Background.enabled       = true;
        }

        /// <summary>
        /// 卸载加载的 UI Screen View
        /// </summary>
        internal void Unload()
        {
            // 销毁真正的 Screen View 对象
            if (_screenViewGameObject != null)
            {
                Destroy(_screenViewGameObject);
                _screenViewGameObject = null;
                _screenBehaviour      = null;
            }

            _screenPresenter      = null;
            _loadCompleteCallback = null;
            _loadFailCallback = null;

            m_State = EState.None;

            Debug.Assert(_loadOperationHandle.IsValid());
            Addressables.Release(_loadOperationHandle);
            _loadOperationHandle = default;//已经卸载过，将值赋值为默认值，否则如果有其他缓存的地方，会在LoadScreenAsset方法里多次释放，释放掉其他缓存的地方
        }

        // 当资源加载完成时调用
        private void LoadOperationHandleOnCompleted(AsyncOperationHandle<GameObject> handle)
        {
            Debug.Assert(m_State == EState.Loading || m_State == EState.Retrying);
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                LogFormat("[UIScreenLoader] 加载完成 Screen: {0}", _screenPresenter.ScreenName);

                // 加载成功
                // 实例化UI Screen View
                _screenViewGameObject = Instantiate(handle.Result, gameObject.transform, false);
                _screenViewGameObject.SetActive(true);

                // 设置UI Screen View
                _screenPresenter.InitializeView(_screenViewGameObject);

                // 停止加载动画
                StopLoadingAnimation();

                // 通知加载完成, 打开窗口
                Open();
            }
            else if (handle.Status == AsyncOperationStatus.Failed)
            {
                m_State = EState.Retrying;
                if (IsNetWorkError(handle))
                {
                    // 网络错误, 尝试重新加载
                    LogFormat("[UIScreenLoader] 网络错误, 尝试重新加载 Screen: {0}", _screenPresenter.ScreenName);

                    // 重新加载
                    LoadScreenAsset();
                }
                else
                {
                    // 其它错误, 显示错误信息, 并显示关闭按钮，加载失败的回调要放在强制关闭按钮的方法里调用，否则加载状态没清空，再次调用打开的时候，会导致返回不正确的问题
                    m_LoadingText.gameObject.SetActive(true);
                    m_LoadingText.text = $"{_screenPresenter.ScreenName} 加载失败: {handle.OperationException}";
                    m_ForceCloseButton.gameObject.SetActive(true);
                    m_ForceCloseButton.onClick.AddListener(OnForceClose);
                    
                }
            }
            else
            {
                Debug.LogError($"无效的加载结果, UI Screen Presenter 名称: {_screenPresenter.GetType().Name}");
            }
        }

        /// <summary>
        /// 开始加载动画的表演
        /// </summary>
        private void StartLoadingAnimation()
        {
            m_LoadingText.gameObject.SetActive(true);
            m_LoadingText.DOKill();

            // 开始加载中文本的动画表演
            var txt = "";
            var tween = DOTween.To(() => txt, v => txt = v, "... ", 1.5f)
                               .OnUpdate(() => m_LoadingText.text = $"Loading {_screenPresenter.ScreenName} {txt}")
                               .SetTarget(m_LoadingText)
                               .SetLoops(-1);
            tween.Play();
        }

        /// <summary>
        /// 停止加载动画的表演
        /// </summary>
        private void StopLoadingAnimation()
        {
            if (_loadTimeoutTween != null)
            {
                _loadTimeoutTween.Kill();
                _loadTimeoutTween = null;
            }

            m_LoadingText.gameObject.SetActive(false);
            m_LoadingText.DOKill();
        }

        /// <summary>
        /// 加载失败时调用来关闭本 Screen
        /// </summary>
        private void OnForceClose()
        {
            //加载失败回调，重试加载的时候不调用该回调
            _loadFailCallback?.Invoke();
            _loadFailCallback = null;
            Debug.LogErrorFormat("由于非网络原因加载失败, 玩家强制关闭 UI: {0}", _screenPresenter.ScreenName);
            UIManager.CloseScreen(_screenPresenter);
        }

        /// <summary>
        /// 判断加载失败是否是由于网络错误
        /// </summary>
        /// <param name="asyncOperationHandle">要判定的加载器</param>
        /// <returns></returns>
        private static bool IsNetWorkError<TObject>(AsyncOperationHandle<TObject> asyncOperationHandle)
        {
            if (asyncOperationHandle.Status != AsyncOperationStatus.Failed)
                return false;

            if (asyncOperationHandle.OperationException is not OperationException)
                return false;

            if (asyncOperationHandle.OperationException.InnerException is not RemoteProviderException)
                return false;

            return true;
        }

        /// <summary>
        /// 调试用的日志输出函数
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        [Conditional("UNITY_EDITOR")]
        public static void LogFormat(string format, params object[] args)
        {
            return;
            Debug.LogFormat(format, args);
        }

        /// <summary>
        /// 处理背景的点击事件
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_State != EState.Opened)
                return; // 窗口还没有打开, 忽略这个事件

            if (_screenBehaviour == null)
                return; // 判 null 是为了兼容旧的 Screen 导出

            switch (_screenBehaviour.BackgroundClickEventType)
            {
                case EBackgroundClickEventType.CloseSelf: // 关闭 Screen
                    UIManager.CloseScreen(_screenPresenter);
                    break;
                case EBackgroundClickEventType.PassThrough:
                    break;
                case EBackgroundClickEventType.BlockWithoutResponse: // 什么都不做
                    break;
                case EBackgroundClickEventType.CustomCallback:
                    _screenBehaviour.BackgroundClickEventCallback?.Invoke();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
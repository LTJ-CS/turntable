using System;
using UIFramework.Extensions;
using UIFramework.UIScreen;
using UnityEngine;
using UnityEngine.Pool;

// ReSharper disable once CheckNamespace
namespace UIFramework.Base
{
    /// <summary>
    /// UI Root 的组件, 用于实现一些与 UI Root Prefab 相关的功能, 方便管理
    /// </summary>
    internal class UIRootComponent : MonoBehaviour
    {
        [SerializeField] [Tooltip("UI Layer 的根对象")]
        private RectTransform m_LayerRoot;

        [Tooltip("ScreenLoader 的模板")] [SerializeField]
        private GameObject m_ScreenLoaderTemplate;

        /// <summary>
        /// Screen Loader 的对象池
        /// </summary>
        private ObjectPool<GameObject> _screenLoaderPool;

        /// <summary>
        /// 保存每层 UI 的根对象
        /// </summary>
        private readonly RectTransform[] _uiLayers = new RectTransform[(int)EUILayer.Count];

        private void Awake()
        {
            Debug.Assert(m_ScreenLoaderTemplate != null);
            name = "UIRoot";
            DontDestroyOnLoad(gameObject);
            InitLayers();

            // 初始化 Screen Loader 的对象池
            _screenLoaderPool = new ObjectPool<GameObject>(
                () => Instantiate(m_ScreenLoaderTemplate),
                null,
                loaderObj => loaderObj.GetComponent<UIScreenLoader>().Unload(),
                Destroy
            );
        }

        private void OnDestroy()
        {
            _screenLoaderPool.Dispose();
        }

        /// <summary>
        /// 初始化 UI 层
        /// </summary>
        private void InitLayers()
        {
            for (var layerIndex = 0; layerIndex < (int)EUILayer.Count; layerIndex++)
            {
                var layerTrs = RectTransformExtensions.CreateUIRect(m_LayerRoot, Enum.GetName(typeof(EUILayer), layerIndex));
                layerTrs.ResetToFullScreen();
                _uiLayers[layerIndex] = layerTrs;
            }

            // 默认禁用 Cache 层
            _uiLayers[(int)EUILayer.Cache].gameObject.SetActive(false);
        }

        /// <summary>
        /// 从缓冲池中申请一个新的 Screen Loader
        /// </summary>
        internal UIScreenLoader QueryScreenLoader(EUILayer layer)
        {
            var loaderGameObject = _screenLoaderPool.Get();
            loaderGameObject.transform.SetParent(_uiLayers[(int)layer], false);
            loaderGameObject.SetActive(true);
            return loaderGameObject.GetComponent<UIScreenLoader>();
        }

        /// <summary>
        /// 释放 Screen Loader, 还回缓冲池
        /// </summary>
        /// <param name="screenLoader">要释放的 Screen Loader</param>
        internal void ReleaseScreenLoader(UIScreenLoader screenLoader)
        {
            // TODO: 考虑延迟卸载的功能, 提升性能?
            var loaderGameObject = screenLoader.gameObject;
            loaderGameObject.SetActive(false);
            loaderGameObject.transform.SetParent(_uiLayers[(int)EUILayer.Cache], false);
            _screenLoaderPool.Release(loaderGameObject);
        }

        /// <summary>
        /// 获取缓冲的 Screen Loader
        /// </summary>
        /// <typeparam name="TScreenPresenter">指定要获取的 Screen Loader 的 Presenter 类型</typeparam>
        /// <returns></returns>
        internal UIScreenLoader QueryCachedScreenLoader<TScreenPresenter>()
            where TScreenPresenter : UIScreenPresenterBase, new()
        {
            return null;
        }
    }
}
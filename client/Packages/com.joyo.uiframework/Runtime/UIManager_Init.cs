using System;
using Cysharp.Threading.Tasks;
using UIFramework.Base;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ReSharper disable once CheckNamespace
namespace UIFramework
{
    partial class UIManager
    {
#if UNITY_EDITOR
        /// <summary>
        /// 是否需要重新初始化 UIManager, 用于兼容 EditorSettings.enterPlayModeOptionsEnabled 模式
        /// </summary>
        private static bool _reinitializeUIManager = true;

        [InitializeOnLoadMethod]
        static void RegisterPlayModeStateChange()
        {
            EditorApplication.playModeStateChanged += SetUIManagerReInitFlagOnExitPlayMode;
        }

        static void SetUIManagerReInitFlagOnExitPlayMode(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
            {  // 退出 Playing 模式时，关闭所有 Screen, 以兼容 EditorSettings.enterPlayModeOptionsEnabled 模式
                Instance?.CloseAllScreensImmediately();
            }
            
            if (change == PlayModeStateChange.EnteredEditMode || change == PlayModeStateChange.ExitingPlayMode)
                _reinitializeUIManager = true;
        }
#endif
        
        /// <summary>
        /// 加载 UIRoot 资源并实例化
        /// </summary>
        private async UniTask InitUIRoot()
        {
            // 加载 UIRoot 资源
            var uiRootAsset = await EnsureLoadAssetAsync<GameObject>(UIFrameworkSettings.RootPrefabPath);
            var uiRootGo = Object.Instantiate(uiRootAsset);
            _uiRoot = uiRootGo.GetComponent<UIRootComponent>();
            Debug.Assert(_uiRoot != null, $"UIRoot({UIFrameworkSettings.RootPrefabPath})上找不到 UIRootComponent 组件???");
        }
        
        /// <summary>
        /// 确保必须加载完成指定的资源
        /// </summary>
        /// <param name="key">指定要加载的资源的 Key</param>
        /// <typeparam name="TObject">指定要加载的资源类型</typeparam>
        /// <returns></returns>
        private async UniTask<TObject> EnsureLoadAssetAsync<TObject>(string key)
        {
            while (true)
            {
                var handle = Addressables.LoadAssetAsync<TObject>(key);
                await handle;
                if (!handle.IsValid())
                {
                    throw new Exception($"找不到要加载的资源: {key}({typeof(TObject)})");
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(handle);
                    Debug.LogError($"[UIManager] 加载资源失败: {key}({typeof(TObject)}), 重试中 ...");
                    await UniTask.Delay(500); // 可能是网络问题, 等待500ms后重试
                    continue;
                }
                return handle.Result;
            }
        }
    }
}
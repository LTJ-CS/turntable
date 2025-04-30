using System.Collections;
using GameScript.Runtime.GameLogic;
using MyUI;
using Sdk.Runtime.Base;
using UIFramework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace GameScript.Runtime.Util
{
    /// <summary>
    /// 每个过渡场景都需要有这个组件的实例, 以便处理目标场景的加载
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        /// <summary>
        /// 保存加载不成功时需要回退到的场景名称, 仅供 NormalSceneLoadState 类加载过渡场景时使用
        /// </summary>
        public static string fallbackSceneName;

        /// <summary>
        /// 用于保存当前需要过渡到的目标场景
        /// </summary>
        public static string targetSceneName;

        private static SceneTransition _instance = null;

        /// <summary>
        /// 处理场景的加载
        /// </summary>
        /// <returns></returns>
        private IEnumerator Start()
        {
            if (_instance != null)
            {
                // 可能上一个过渡效果还没有完成, 则直接销毁它吧
                DestroyImmediate(_instance.gameObject);
                _instance = null;
            }

            while (!GameInstance.Instance.m_WaitScene)
            {
                yield return null;
            }

            var myScene = gameObject.scene;
            // 卸载当前所有场景
            {
                var sceneCount = SceneManager.sceneCount;
                for (var i = sceneCount - 1; i >= 0; i--)
                {
                    var sceneLoaded = SceneManager.GetSceneAt(i);
                    if (sceneLoaded != myScene)
                        yield return SceneManager.UnloadSceneAsync(sceneLoaded);
                }
            }

            // 释放没有用的资源
            yield return Resources.UnloadUnusedAssets();

            // 确保自己不会被新场景的加载干掉
            // DontDestroyOnLoad(gameObject);
            _instance = this;

            // 设置新的场景为当前场景, 否则会导致 new GameObject 时被放置到本过渡场景中, 然后过渡场景后面销毁时, 会一起销毁掉.
            // 加载目标场景
            var handle = ResLoaderHelper.LoadSceneAsync(targetSceneName);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Failed)
            {
                //场景加载完成后需要把handle释放掉，否则会报already load same file 的错误
                if (handle.IsValid())
                    Addressables.Release(handle);
                // 加载失败, 回退到之前的场景
                Debug.LogError($"过渡目标场景({targetSceneName})时加载失败, 回退到之前的场景({fallbackSceneName})");
                var fallbackHandler = ResLoaderHelper.EnsureLoadSceneAsync(fallbackSceneName);
                fallbackHandler.RetryOnFailure = EResourceLoadFailedAction.Retry;
                while (!fallbackHandler.IsDone)
                {
                    yield return null;
                }
                //fallbackHandler，否则会报already load same file 的错误
                fallbackHandler.Release();
                _instance = null;
                yield break;
            }
            else
            {
                //场景加载完成后需要把handle释放掉，否则会报already load same file 的错误
                if (handle.IsValid())
                    Addressables.Release(handle);
            }

            _instance = null;
        }
    }
}
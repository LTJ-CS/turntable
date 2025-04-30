using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Sdk.Runtime.Base
{
    /// <summary>
    /// 定义资源加载的辅助类
    /// </summary>
    public static class ResLoaderHelper
    {
        /// <summary>
        /// 加载指定名称的场景
        /// </summary>
        /// <param name="sceneName">指定要加载的场景名称, 只需要场景名称, 不要带后缀</param>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading).  The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return Addressables.LoadSceneAsync(PathManager.GetScenePath(sceneName), loadMode, activateOnLoad, priority);
        }

        /// <summary>
        /// 确保指定名称的场景加载成功, 如果下载失败会弹出窗口让玩家选择重试或放弃
        /// </summary>
        /// <param name="sceneName">指定要加载的场景名称, 只需要场景名称, 不要带后缀</param>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading).  The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        /// <returns>
        /// 返回加载句柄, 可以通过加载句柄来修改加载失败时的处理, 见: <see cref="AsyncResourceLoadHandle{T}.RetryOnFailure"/>.  
        /// </returns>
        /// <remarks>
        /// activateOnLoad 为 false 时, 在 Scene 被 Active 之前, 会阻塞住所有的 AssetBundle 的加载, 切记!!!
        /// </remarks>
        public static AsyncResourceLoadHandle<SceneInstance> EnsureLoadSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            var scenePath = PathManager.GetScenePath(sceneName);
            return EnsureLoadAssetAsync(scenePath, LoadSceneFunc);

            AsyncOperationHandle<SceneInstance> LoadSceneFunc(string realScenePath)
            {
                return Addressables.LoadSceneAsync(realScenePath, loadMode, activateOnLoad, priority);
            }
        }

        /// <summary>
        /// 加载 UI 使用的音效资源
        /// </summary>
        /// <param name="soundClipName">指定要加载的音效资源</param>
        /// <returns></returns>
        public static AsyncOperationHandle<AudioClip> LoadUISoundAsync(string soundClipName)
        {
            return Addressables.LoadAssetAsync<AudioClip>(PathManager.GetUISoundClipPath(soundClipName));
        }

        /// <summary>
        /// 确保指定路径的 UI 音效资源不会由于网络错误而导致加载不成功, 如果多次加载失败, 会弹出窗口让玩家选择重试或放弃
        /// </summary>
        /// <param name="soundClipName">指定要加载的UI 音效资源的名称</param>
        /// <returns>
        /// 返回加载句柄, 可以通过加载句柄来修改加载失败时的处理, 见: <see cref="AsyncResourceLoadHandle{T}.RetryOnFailure"/>.  
        /// </returns>
        public static AsyncResourceLoadHandle<AudioClip> EnsureLoadUISoundAsync(string soundClipName)
        {
            return EnsureLoadAssetAsync(PathManager.GetUISoundClipPath(soundClipName), Addressables.LoadAssetAsync<AudioClip>);
        }
        
        /// <summary>
        /// 确保指定路径的sprite资源不会由于网络错误而导致加载不成功, 如果多次加载失败, 会弹出窗口让玩家选择重试或放弃
        /// </summary>
        /// <param name="spriteName">指定要加载的sprite资源的名称</param>
        /// <returns>
        /// 返回加载句柄, 可以通过加载句柄来修改加载失败时的处理, 见: <see cref="AsyncResourceLoadHandle{T}.RetryOnFailure"/>.  
        /// </returns>
        public static AsyncResourceLoadHandle<Sprite> EnsureLoadSpriteAsync(string spriteName)
        {
            return EnsureLoadAssetAsync(spriteName, Addressables.LoadAssetAsync<Sprite>);
        }
        
        /// <summary>
        /// 确保指定路径的配置资源不会由于网络错误而导致加载不成功, 如果多次加载失败, 会弹出窗口让玩家选择重试或放弃
        /// </summary>
        /// <param name="cfgName">指定要加载的配置资源的名称</param>
        /// <returns>
        /// 返回加载句柄, 可以通过加载句柄来修改加载失败时的处理, 见: <see cref="AsyncResourceLoadHandle{T}.RetryOnFailure"/>.  
        /// </returns>
        public static AsyncResourceLoadHandle<TextAsset> EnsureLoadCfgAsync(string cfgName)
        {
            return EnsureLoadAssetAsync(PathManager.GetCfgPath(cfgName), Addressables.LoadAssetAsync<TextAsset>);
        }

        /// <summary>
        /// 加载指定 Id 的关卡数据文件, json 文件???
        /// </summary>
        /// <param name="levelId">指定要加载关卡数据的关卡 Id</param>
        public static AsyncOperationHandle<TextAsset> LoadLevelDataFileAsync(string levelId)
        {
            return Addressables.LoadAssetAsync<TextAsset>(PathManager.GetLevelDataFilePath(levelId));
        }
        
        /// <summary>
        /// 确保指定 Id 的关卡数据文件不会由于网络错误而导致加载不成功, 如果多次加载失败, 会弹出窗口让玩家选择重试或放弃
        /// </summary>
        /// <param name="levelId">指定要加载的关卡数据的关卡 Id</param>
        /// <returns></returns>
        public static AsyncResourceLoadHandle<TextAsset> EnsureLoadLevelDataFileAsync(string levelId)
        {
            return EnsureLoadAssetAsync(PathManager.GetLevelDataFilePath(levelId), Addressables.LoadAssetAsync<TextAsset>);
        }
        
        /// <summary>
        /// 确保指定 Id 的地图数据文件不会由于网络错误而导致加载不成功, 如果多次加载失败, 会弹出窗口让玩家选择重试或放弃
        /// </summary>
        /// <param name="mapId">指定要加载的地图的 Id</param>
        /// <returns></returns>
        public static AsyncResourceLoadHandle<TextAsset> EnsureLoadMapDataAsync(string mapId)
        {
            return EnsureLoadAssetAsync(PathManager.GetOfficialMapFilePath(mapId), Addressables.LoadAssetAsync<TextAsset>);
        }

        /// <summary>
        /// 通过 label 加载资源
        /// </summary>
        /// <param name="resLabel">指定要加载的资源label</param>
        /// <returns></returns>
        public static AsyncOperationHandle<IList<TObject>> LoadAssetListAsync<TObject>(string resLabel)
        {
            return Addressables.LoadAssetsAsync<TObject>(resLabel, null);
        }

        /// <summary>
        /// 确保指定label的资源不会由于网络错误而导致加载不成功, 如果多次加载失败, 会弹出窗口让玩家选择重试或放弃
        /// </summary>
        /// <param name="resLabel">指定要加载的资源的label</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <returns>
        /// 返回加载句柄, 可以通过加载句柄来修改加载失败时的处理, 见: <see cref="AsyncResourceLoadHandle{T}.RetryOnFailure"/>.  
        /// </returns>
        public static AsyncResourceLoadHandle<IList<TObject>> EnsureLoadAssetListAsync<TObject>(string resLabel, Action<TObject> callback = null)
        {
            // TODO: 消除回调部分的 GC
            return EnsureLoadAssetAsync(resLabel, key=>Addressables.LoadAssetsAsync(key, callback));
        }

        /// <summary>
        /// 确保指定路径的资源不会由于网络错误而导致加载不成功, 如果多次加载失败, 会弹出窗口让玩家选择重试或放弃
        /// </summary>
        /// <param name="assetPath">指定要加载的资源的路径</param>
        /// <typeparam name="TObject">指定要加载的资源的类型</typeparam>
        /// <returns>
        /// 返回加载句柄, 可以通过加载句柄来修改加载失败时的处理, 见: <see cref="AsyncResourceLoadHandle{T}.RetryOnFailure"/>.  
        /// </returns>
        public static AsyncResourceLoadHandle<TObject> EnsureLoadAssetAsync<TObject>(string assetPath)
        {
            return EnsureLoadAssetAsync(assetPath, Addressables.LoadAssetAsync<TObject>);
        }
        
           
        /// <summary>
        /// 加载地图数据
        /// </summary>
        /// <param name="mapId">指定要加载的地图的 Id</param>
        /// <returns></returns>
        public static AsyncOperationHandle<TextAsset>LoadMapDataAsync(string mapId)
        {
            return Addressables.LoadAssetAsync<TextAsset>(PathManager.GetOfficialMapFilePath(mapId));
        }

        /// <summary>
        /// 加载bgm
        /// </summary>
        /// <param name="bgmName">bgm 名字</param>
        /// <returns></returns>
        public static AsyncOperationHandle<AudioClip> LoadBgmAsync(string bgmName)
        {
            return Addressables.LoadAssetAsync<AudioClip>(PathManager.GetBgmFilePath(bgmName));
        }

        /// <summary>
        /// 从远端下载图片
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async UniTask<(Texture2D texture2D,string error)> LoadTextureFromUrl(string url)
        {
            UniTaskCompletionSource<(Texture2D texture2D, string error)> taskCompletionSource = new UniTaskCompletionSource<(Texture2D texture2D, string error)>();
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            request.SendWebRequest();
            while (!request.isDone)
            {
                await UniTask.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                taskCompletionSource.TrySetResult((null, request.error));
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                taskCompletionSource.TrySetResult((texture, string.Empty));
               
            }

            return await taskCompletionSource.Task;
        }
        
        /// <summary>
        /// 判断加载失败是否是由于网络错误
        /// </summary>
        /// <param name="asyncOperationHandle">要判定的场景加载器</param>
        /// <returns></returns>
        public static bool IsNetworkError<TObject>(this AsyncOperationHandle<TObject> asyncOperationHandle)
        {
            return IsNetWorkError(asyncOperationHandle);
        }

        /// <summary>
        /// 判断加载失败是否是由于网络错误
        /// </summary>
        /// <param name="asyncOperationHandle">要判定的场景加载器</param>
        /// <returns></returns>
        public static bool IsNetWorkError(AsyncOperationHandle asyncOperationHandle)
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
        /// 如果是网络导致的错误, 会尽量偿试确保加载的资源成功, 如果一直失败, 会弹出窗口让玩家选择重试或放弃 
        /// </summary>
        /// <param name="resPath">指定要加载的资源路径</param>
        /// <param name="loadFunc">指定真正的资源加载函数</param>
        /// <typeparam name="TObject">要加载的资源类型</typeparam>
        /// <returns></returns>
        public static AsyncResourceLoadHandle<TObject> EnsureLoadAssetAsync<TObject>(string resPath, Func<string, AsyncOperationHandle<TObject>> loadFunc)
        {
            var resLoadHandle = new AsyncResourceLoadHandle<TObject>();
            return resLoadHandle.EnsureLoadAsync(resPath, loadFunc);
        }
    }
}
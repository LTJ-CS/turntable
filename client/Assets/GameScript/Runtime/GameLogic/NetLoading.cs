using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MyUI;
using Sdk.Runtime.Base;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;

namespace GameScript.Runtime.GameLogic
{
    /// <summary>
    /// 网络加载失败或者资源加载弹窗，同时只有一个弹窗，如果玩家点击了取消，则全部取消重试
    /// 如果点击了确认重试，则全部重试
    /// </summary>
    public class NetLoading : ComponentSingleton<NetLoading>
    {
        
        /// <summary>
        ///  网络弹窗资源加载的句柄
        /// </summary>
        private AsyncOperationHandle<GameObject> loadResHandle;

        /// <summary>
        /// 网络弹窗资源的地址
        /// </summary>
        public static readonly string LoadingResPath = PathManager.ResRoot + "UI/WaitNet/Prefabs/WaitNetScreenView.prefab";

        /// <summary>
        /// 资源加载失败后，等待用户交互弹窗的返回taskCompletionSource
        /// </summary>
        private readonly List<UniTaskCompletionSource<bool>> completionSourceList = new();
        /// <summary>
        /// 资源失败的等待返回的操作 避免在遍历completionSourceList 时，修改completionSourceList，所以需要先加个过渡的集合，等completionSourceList遍历完成，再加入到completionSourceList里
        /// </summary>
        private readonly List<UniTaskCompletionSource<bool>> waitAddResCompletionSources = new();
        
        
        /// <summary>
        /// 网络失败的等待返回的操作 避免在遍历completionSourceList 时，修改completionSourceList，所以需要先加个过渡的集合，等completionSourceList遍历完成，再加入到completionSourceList里
        /// </summary>
        private readonly List<UniTaskCompletionSource<bool>> waitAddNetCompletionSources = new();
        /// <summary>
        /// 网络等待界面是否已经打开
        /// </summary>
        private bool _isUINetShow = false;
        
        /// <summary>
        /// 网络等待界面是否准备不显示
        /// </summary>
        private bool _isWaitHide = false;
        
        /// <summary>
        /// 网络弹窗的实例
        /// </summary>
        private WaitNetScreenView waitNetScreenView;

        /// <summary>
        /// 网络信息同步失败的回调
        /// </summary>
        public static Func<WaitNetTipType,string, UniTask<bool>> OnNetLoadFailedSelection;


        private NetLoading()
        {
            //注册资源加载失败回调
            AsyncResourceLoadFailed.OnLoadFailedSelection = (errorInfo, resLoadFailedAction) =>
            {
                var completionSource = new UniTaskCompletionSource<bool>();
                waitAddResCompletionSources.Add(completionSource); //避免遍历resCompletionSources时修改集合，所以加了个waitAddResCompletionSources
                if (!_isUINetShow)                                 // 如果没有打开加载界面，则显示加载界面
                {
                    _isWaitHide = false;
                    WaitNetTipType waitNetTipType = resLoadFailedAction == EResourceLoadFailedAction.Retry ? WaitNetTipType.LoadingAndTryConfirmWithoutClose : WaitNetTipType.LoadingAndTryConfirmWithClose;
                    ShowResTryConfirmView(errorInfo,waitNetTipType).ContinueWith((retry) =>
                    {
                        completionSourceList.AddRange(waitAddResCompletionSources);
                        completionSourceList.AddRange(waitAddNetCompletionSources);
                        waitAddResCompletionSources.Clear();
                        waitAddNetCompletionSources.Clear();
                        foreach (var retryEntry in completionSourceList) //如果用户点击了重试，重试所有失败
                        {
                            retryEntry.TrySetResult(retry);
                        }
                        completionSourceList.Clear();
                        _isUINetShow = false;
                        HideViewAsync(); //关闭弹窗
                    }).Forget();
                    _isUINetShow = true;
                }

                return completionSource.Task;
            };

            
            //网络请求失败回调
            OnNetLoadFailedSelection =  (waitNetTipType,errorInfo) =>
            {
           
                var completionSource = new UniTaskCompletionSource<bool>();
                waitAddNetCompletionSources.Add(completionSource); //completionSourceList，waitAddNetCompletionSources
                if (!_isUINetShow)                                 // 如果没有打开加载界面，则显示加载界面
                {
                    _isWaitHide = false;
                    ShowNetTryConfirmView(errorInfo,waitNetTipType).ContinueWith((retry) =>
                    {
                        completionSourceList.AddRange(waitAddResCompletionSources);
                        completionSourceList.AddRange(waitAddNetCompletionSources);
                        waitAddResCompletionSources.Clear();
                        waitAddNetCompletionSources.Clear();
                        foreach (var retryEntry in completionSourceList) //如果用户点击了重试，重试所有失败
                        {
                            retryEntry.TrySetResult(retry);
                        }
                        completionSourceList.Clear();
                        _isUINetShow = false;
                        HideViewAsync(); //关闭弹窗
                    }).Forget();
                    _isUINetShow = true;
                }

                return completionSource.Task;
            };
            
        }

        private void Update()
        {
            //按照文档的需求，延迟打开loading界面
            if (loadingDelay > 0)
            {
                loadingDelay -= Time.deltaTime;
                if (loadingDelay <= 0)
                {
                    DoShowLoading(0);
                }
            }
        }

        /// <summary>
        /// 打开loading
        /// </summary>
        private float loadingDelay = 0;
        public static void ShowLoading(float delay)
        {
           Instance.DoShowLoading(delay);
        }

        /// <summary>
        /// 关闭loading
        /// </summary>
        public static void HideLoading()
        {
            if(Instance.waitNetScreenView != null)
                Instance.waitNetScreenView.HideLoading();
        }

        /// <summary>
        /// 实际打开或者标记打开loading
        /// </summary>
        /// <param name="delay"></param>
        void DoShowLoading(float delay)
        {
            //如果已经有弹窗打开，则不打开loading界面
            if(_isUINetShow)
                return;
            if (delay <= 0)
            {
                LoadWaitNetView();
                if (waitNetScreenView != null && !_isUINetShow)
                {
                    waitNetScreenView.ShowLoading();
                }
            }
            else
            {
                loadingDelay = delay;
            }
        }

    
       
        /// <summary>
        /// 展示提示错误信息的弹窗
        /// </summary>
        /// <param name="errorTip">网络信息提示</param>
        /// <param name="waitNetTipType">弹窗类型</param>
        /// <returns></returns>
        UniTask<bool> ShowResTryConfirmView(string errorTip,WaitNetTipType waitNetTipType)
        {
            LoadWaitNetView();
            if (waitNetScreenView == null)
                return default;
            
            return waitNetScreenView.ShowErrorTipTryConfirm(errorTip,waitNetTipType);
        }
        
        /// <summary>
        /// 展示提示网络错误码的弹窗，适用于网络错误返回
        /// </summary>
        /// <param name="errorCode">网络错误码</param>
        /// <param name="waitNetTipType">弹窗类型</param>
        /// <returns></returns>
        UniTask<bool> ShowNetTryConfirmView(string errorCode,WaitNetTipType waitNetTipType)
        {
            LoadWaitNetView();
            if (waitNetScreenView == null)
                return default;
            
            return waitNetScreenView.ShowErrorCodeTryConfirm(errorCode,waitNetTipType);
        }
        /// <summary>
        /// 弹出二次确认弹出弹窗
        /// 由上层逻辑控制的弹窗
        /// </summary>
        /// <param name="secondConfirmTip">二次弹窗提示信息</param>
        /// <returns></returns>
        UniTask<bool> ShowSecondConfirmView(string secondConfirmTip = "")
        {
            LoadWaitNetView();
            if (waitNetScreenView == null)
                return default;
            var completionSource = new UniTaskCompletionSource<bool>();
            waitNetScreenView.ShowTrySecondConfirm(secondConfirmTip).ContinueWith((retry) =>
            {
                if (retry)
                {
                    HideViewAsync();
                }

                completionSource.TrySetResult(retry);

            }).Forget();
            return completionSource.Task;
        }
        
        async void HideViewAsync()
        {
            if(waitNetScreenView == null)
                return;
            waitNetScreenView.ShowLoading();
            _isWaitHide = true;
            await UniTask.Delay(TimeSpan.FromSeconds(0.01f)); //避免闪烁，延迟关闭
            //如果再次打开了，则不关闭
            if (_isWaitHide)
            {
                _isWaitHide = false;
                waitNetScreenView.ResetView();
            }
          
        }

        /// <summary>
        /// 根据已经加载好的资源，打开弹窗界面，但是不会显示任何内容
        /// </summary>
        void LoadWaitNetView()
        {
            if (waitNetScreenView == null)
            {
                var go = GameObject.Instantiate(loadResHandle.Result, transform);
                waitNetScreenView = go.GetComponent<WaitNetScreenView>();
            }
        }
        
        /// <summary>
        /// 对外接口，调用预加载资源
        /// </summary>
        public static async UniTask Init()
        {
            await Instance.PreLoadLoadingRes();
        }
        /// <summary>
        /// 预先加载网络弹窗资源
        /// </summary>
        private async UniTask PreLoadLoadingRes()
        {
            if (loadResHandle.IsValid())
            {
                if(loadResHandle.Result == null)
                    Addressables.Release(loadResHandle);
                else
                {
                    return;
                }
            }
            // 加载 netLoading 资源
            loadResHandle = await EnsureLoadAssetAsync(LoadingResPath);
        }

        /// <summary>
        /// 尽量确保加载到网络弹窗的资源
        /// </summary>
        /// <param name="key">网络弹窗资源地址</param>
        /// <returns>返回资源加载句柄</returns>
        /// <exception cref="Exception"></exception>
        private async UniTask<AsyncOperationHandle<GameObject>> EnsureLoadAssetAsync(string key)
        {
            while (true)
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(key);
                await handle;
                if (!handle.IsValid())
                { 
                    throw new Exception($"找不到要加载的资源: {key}");
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(handle);
                    Debug.LogError($"[NetLoading] 加载NetLoading资源失败: {key}), 重试中 ...");
                    await UniTask.Delay(500); // 可能是网络问题, 等待500ms后重试
                    continue;
                }

                return handle;
            }
        }

        private void OnDestroy()
        {
            _isUINetShow = false;
            //卸载加载的网络等待资源句柄
            if (loadResHandle.IsValid())
            {
                Addressables.Release(loadResHandle);
            }

            loadResHandle = default;
        }
    }
}


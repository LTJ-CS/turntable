using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Sdk.Runtime.Base
{
    /// <summary>
    /// 资源加载失败时提示的类型
    /// </summary>
    public enum EResourceLoadFailedAction
    {
        /// <summary>
        /// 安静的重试, 加载失败时没有任何对话框提示或错误输出, 一直重试直到成功
        /// </summary>
        None,
        
        /// <summary>
        /// 显示带一个"重试"按钮的对话框
        /// </summary>
        Retry,
        
        /// <summary>
        /// 显示带有"放弃"与"重试"两个按钮的对话框
        /// </summary>
        GiveUpAndRetry,
    }

    /// <summary>
    /// 定义一个统一的AsyncResourceLoadHandle接口, 方便统一释放
    /// </summary>
    public interface IAsyncResourceLoadHandle
    {
        /// <inheritdoc cref="AsyncResourceLoadHandle{TObject}.Release"/>
        public void Release();

        public AsyncOperationStatus Status { get; }
        
        public object ObjectResult { get; }

        public bool IsValid();

    }

    public static class AsyncResourceLoadFailed
    {
      
        public static Func<string, EResourceLoadFailedAction, UniTask<bool>> OnLoadFailedSelection =
            DefaultLoadFailedSelection;
        /// <summary>
        /// 默认的资源加载失败选择器
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="failedAction">资源加载失败的错误类型</param>
        /// <returns></returns>
        static UniTask<bool> DefaultLoadFailedSelection(string message, EResourceLoadFailedAction failedAction)
        {
            Debug.LogError("[AsyncResourceLoadHandle] 资源加载失败的默认实现, 会自动继续重试: " + message);
            return UniTask.FromResult(true);
        }
    
    }
    /// <summary>
    /// 封装了对 Addressables 的 AsyncOperationHandle 的操作, 以支持我们自己资源加载函数的封装
    /// </summary>
    /// <typeparam name="TObject">The object type of the underlying operation.</typeparam>
    public class AsyncResourceLoadHandle<TObject> :  IEnumerator, IEquatable<AsyncResourceLoadHandle<TObject>>, IAsyncResourceLoadHandle
    {
        internal AsyncResourceLoadHandle()
        {
        }
        
        private AsyncOperationHandle<TObject> _internalOp;

        /// <summary>
        /// 完成时的回调, TODO: 参考 <see cref="DelegateList{T}"/>  来实现一个避免 GC 的版本
        /// </summary>
        private Action<AsyncResourceLoadHandle<TObject>> _completedActionT;

        /// <summary>
        /// 销毁时的回调
        /// </summary>
        private Action<AsyncOperationHandle> _destroyedAction;

        /// <summary>
        /// 真正的资源加载操作, 仅供资源加载函数访问, 如 ResLoaderHelper.EnsureLoadSceneAsync() 等
        /// </summary>
        private AsyncOperationHandle<TObject> InternalOp
        {
            get => _internalOp;
            set
            {
                if (_isGiveUp)
                {
                    Debug.LogError("You can't set InternalOp after give up!");
                    return;
                }

                // 确保添加的 Destroyed 事件在更换 InternalOp 后生效
                if (InternalOp.IsValid())
                    InternalOp.Destroyed -= InternalOpOnDestroyed;
                _internalOp = value;
                if (_completedActionT != null && InternalOp.IsValid())
                {
                    InternalOp.Destroyed += InternalOpOnDestroyed;
                }
            }
        }

        /// <summary>
        /// 是否放弃加载
        /// </summary>
        private bool _isGiveUp = false;

        /// <summary>
        /// 是否在资源加载失败时显示重试提示
        /// </summary>
        public EResourceLoadFailedAction RetryOnFailure { get; set; } = EResourceLoadFailedAction.None;
        
        
        /// <summary>
        /// Return the current download status for this operation and its dependencies.
        /// </summary>
        /// <returns>The download status.</returns>
        public DownloadStatus GetDownloadStatus()
        {
            return InternalOp.GetDownloadStatus();
        }

        /// <summary>
        /// 当加载完成时回调
        /// </summary>
        /// <param name="handle"></param>
        internal void OnCompleted(AsyncOperationHandle<TObject> handle)
        {
            if (_completedActionT != null)
            {
                _completedActionT?.Invoke(this);
                _completedActionT = null;
            }
        }

        /// <summary>
        /// Completion event for the internal operation.  If this is assigned on a completed operation, the callback is deferred until the LateUpdate of the current frame.
        /// </summary>
        public event Action<AsyncResourceLoadHandle<TObject>> Completed
        {
            add => _completedActionT += value;
            remove => _completedActionT -= value;
        }

        /// <summary>
        /// Completion event for non-typed callback handlers.  If this is assigned on a completed operation, the callback is deferred until the LateUpdate of the current frame.
        /// </summary>
        // public event Action<AsyncOperationHandle> CompletedTypeless
        // {
        //     add { Completed += s => value(s); }
        //     remove => throw new NotImplementedException();
        //     // Completed -= s => value(s);
        // }

        /// <summary>
        /// Debug name of the operation.
        /// </summary>
        public string DebugName => InternalOp.DebugName;

        /// <summary>
        /// Get dependency operations.
        /// </summary>
        /// <param name="deps">The list of AsyncOperationHandles that are dependencies of a given AsyncOperationHandle</param>
        public void GetDependencies(List<AsyncOperationHandle> deps)
        {
            InternalOp.GetDependencies(deps);
        }

        /// <inheritdoc cref="Destroyed"/>
        public event Action<AsyncOperationHandle> Destroyed
        {
            add
            {
                if (_destroyedAction == null)
                {
                    InternalOp.Destroyed += InternalOpOnDestroyed;
                }

                _destroyedAction += value;
            }
            remove => _destroyedAction -= value;
        }

        /// <inheritdoc cref="Destroyed"/>
        private void InternalOpOnDestroyed(AsyncOperationHandle handle)
        {
            if (_destroyedAction != null)
            {
                _destroyedAction.Invoke(handle);
                _destroyedAction = null;
            }
        }

        /// <summary>
        /// Provide equality for this struct.
        /// </summary>
        /// <param name="other">The operation to compare to.</param>
        /// <returns>True if the the operation handles reference the same AsyncOperation and the version is the same.</returns>
        public bool Equals(AsyncResourceLoadHandle<TObject> other)
        {
            return InternalOp.Equals(other.InternalOp);
        }

        /// <summary>
        /// Get hash code of this struct.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return InternalOp.GetHashCode();
        }

        /// <summary>
        /// True if the operation is complete.
        /// </summary>
        public bool IsDone => _isGiveUp || InternalOp is { IsDone: true, Status: AsyncOperationStatus.Succeeded };

        /// <summary>
        /// Check if the handle references an internal operation.
        /// </summary>
        /// <returns>True if valid.</returns>
        public bool IsValid()
        {
            return InternalOp.IsValid();
        }

        /// <summary>
        /// The exception for a failed operation.  This will be null unless Status is failed.
        /// </summary>
        public Exception OperationException => InternalOp.OperationException;

        /// <summary>
        /// The progress of the internal operation.
        /// This is evenly weighted between all sub-operations. For example, a LoadAssetAsync call could potentially
        /// be chained with InitializeAsync and have multiple dependent operations that download and load content.
        /// In that scenario, PercentComplete would reflect how far the overal operation was, and would not accurately
        /// represent just percent downloaded or percent loaded into memory.
        /// For accurate download percentages, use GetDownloadStatus().
        /// </summary>
        public float PercentComplete => InternalOp.PercentComplete;

        /// <summary>
        /// Release the handle.  If the internal operation reference count reaches 0, the resource will be released.
        /// </summary>
        public void Release()
        {
            _isGiveUp = true;
            Addressables.Release(InternalOp);
        }

        /// <summary>
        /// The result object of the operations.
        /// </summary>
        public TObject Result => InternalOp.Result;

        public object ObjectResult => InternalOp.Result;
        /// <summary>
        /// The status of the internal operation.
        /// </summary>
        public AsyncOperationStatus Status => InternalOp.Status;

        object IEnumerator.Current => Result;

        /// <summary>
        /// Overload for <see cref="IEnumerator.MoveNext"/>.
        /// </summary>
        /// <returns>Returns true if the enumerator can advance to the next element in the collectin. Returns false otherwise.</returns>
        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }

        /// <summary>
        /// Overload for <see cref="IEnumerator.Reset"/>.
        /// </summary>
        void IEnumerator.Reset()
        {
        }

        /// <summary>
        /// 保存真正的资源加载函数
        /// </summary>
        private Func<string, AsyncOperationHandle<TObject>> _loadFunc;
        
        /// <summary>
        /// 保存要加载的资源 Key
        /// </summary>
        private string _assetKey;

        /// <summary>
        /// 如果是网络导致的错误, 会确保加载的资源成功, 如果一直失败, 会弹出窗口让玩家选择重试或放弃 
        /// </summary>
        /// <param name="resPath">指定要加载的资源路径</param>
        /// <param name="loadFunc">指定真正的资源加载函数</param>
        /// <typeparam name="TObject">要加载的资源类型</typeparam>
        /// <returns></returns>
        internal AsyncResourceLoadHandle<TObject> EnsureLoadAsync(string resPath, Func<string, AsyncOperationHandle<TObject>> loadFunc)
        {
            if (InternalOp.IsValid())
            {
                throw new Exception($"[AsyncResourceLoadHandle] 正在加载资源({_assetKey})中");
            }

            _loadFunc = loadFunc;
            _assetKey = resPath;

            // TODO: 在不需要完成后的回调函数时, 优化掉 OnLoadCompleted 的使用 

            void RetryLoad()
            {
                if (InternalOp.IsValid())
                {
                    // 先去掉去 Destroyed 事件的监听, 因为要销毁的 Handle 不是最终的 Handle
                    InternalOp.Destroyed -= InternalOpOnDestroyed;
                    Addressables.Release(InternalOp);
                }

                // 开始一次资源加载
                InternalOp = _loadFunc(_assetKey);
                InternalOp.Completed += OnLoadCompleted;
            }

            // 开始第一次加载
            RetryLoad();

            void OnLoadCompleted(AsyncOperationHandle<TObject> handle)
            {
                if (handle.Status == AsyncOperationStatus.Failed)
                {
                    if (handle.IsNetworkError())
                    {
                        // 只有网络异常才需要重试
                        if (RetryOnFailure != EResourceLoadFailedAction.None && AsyncResourceLoadFailed.OnLoadFailedSelection != null)
                        {
                            // 弹窗重试
                            AsyncResourceLoadFailed.OnLoadFailedSelection($"由于网络原因加载资源({_assetKey})失败", RetryOnFailure)
                                                   .ContinueWith(retry =>
                                                   {
                                                       if (retry)
                                                       {
                                                           // 开始一次新的资源加载
                                                           RetryLoad();
                                                       }
                                                       else
                                                       {
                                                           // 放弃重试, 直接调用加载失败的回调
                                                           OnCompleted(handle);
                                                       }
                                                   }).Forget();
                            return;
                        }

                        // 直接重试
                        RetryLoad();
                        return;
                    }

                    // 其它失败不重试, 认为是放弃, 比如找不到 Key 这种情况
                    _isGiveUp = true;
                    // 加载失败的回调
                    OnCompleted(handle);
                }
                else if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    OnCompleted(handle);
                }
                else
                {
                    throw new Exception($"未知的加载状态: {handle.Status}");
                }
            }

            return this;
        }
    }
}
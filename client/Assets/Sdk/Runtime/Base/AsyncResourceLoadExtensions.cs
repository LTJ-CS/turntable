using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Sdk.Runtime.Base
{
    /// <summary>
    /// 扩展对 <see cref="AsyncResourceLoadHandle{TObject}"/> 的扩展方法
    /// </summary>
    public static class AsyncResourceLoadExtensions
    {
        public static UniTask<T>.Awaiter GetAwaiter<T>(this AsyncResourceLoadHandle<T> handle)
        {
            return ToUniTask(handle).GetAwaiter();
        }

        public static UniTask<T> ToUniTask<T>(this AsyncResourceLoadHandle<T> handle, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update,
                                               CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false, bool autoReleaseWhenCanceled = false)
        {
            if (cancellationToken.IsCancellationRequested) return UniTask.FromCanceled<T>(cancellationToken);

            if (!handle.IsValid())
            {
                throw new Exception("Attempting to use an invalid operation handle");
            }

            if (handle.IsDone)
            {
                if (handle.Status == AsyncOperationStatus.Failed)
                {
                    return UniTask.FromException<T>(handle.OperationException);
                }

                return UniTask.FromResult(handle.Result);
            }

            return new UniTask<T>(AsyncOperationHandleConfiguredSource<T>.Create(handle, timing, progress, cancellationToken, cancelImmediately, autoReleaseWhenCanceled, out var token), token);
        }

        sealed class AsyncOperationHandleConfiguredSource<T> : IUniTaskSource<T>, IPlayerLoopItem, ITaskPoolNode<AsyncOperationHandleConfiguredSource<T>>
        {
            static TaskPool<AsyncOperationHandleConfiguredSource<T>> pool;
            AsyncOperationHandleConfiguredSource<T>                  nextNode;
            public ref AsyncOperationHandleConfiguredSource<T>       NextNode => ref nextNode;

            static AsyncOperationHandleConfiguredSource()
            {
                TaskPool.RegisterSizeGetter(typeof(AsyncOperationHandleConfiguredSource<T>), () => pool.Size);
            }

            readonly Action<AsyncResourceLoadHandle<T>> completedCallback;
            AsyncResourceLoadHandle<T>                  handle;
            CancellationToken                           cancellationToken;
            CancellationTokenRegistration               cancellationTokenRegistration;
            IProgress<float>                            progress;
            bool                                        autoReleaseWhenCanceled;
            bool                                        cancelImmediately;
            bool                                        completed;

            UniTaskCompletionSourceCore<T> core;

            AsyncOperationHandleConfiguredSource()
            {
                completedCallback = HandleCompleted;
            }

            public static IUniTaskSource<T> Create(AsyncResourceLoadHandle<T> handle, PlayerLoopTiming timing, IProgress<float> progress, CancellationToken cancellationToken, bool cancelImmediately,
                                                   bool autoReleaseWhenCanceled, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource<T>.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new AsyncOperationHandleConfiguredSource<T>();
                }

                result.handle = handle;
                result.cancellationToken = cancellationToken;
                result.completed = false;
                result.progress = progress;
                result.autoReleaseWhenCanceled = autoReleaseWhenCanceled;
                result.cancelImmediately = cancelImmediately;

                if (cancelImmediately && cancellationToken.CanBeCanceled)
                {
                    result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(state =>
                    {
                        var promise = (AsyncOperationHandleConfiguredSource<T>)state;
                        if (promise.autoReleaseWhenCanceled && promise.handle.IsValid())
                        {
                            promise.handle.Release();
                        }

                        promise.core.TrySetCanceled(promise.cancellationToken);
                    }, result);
                }

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                handle.Completed += result.completedCallback;

                token = result.core.Version;
                return result;
            }

            void HandleCompleted(AsyncResourceLoadHandle<T> argHandle)
            {
                if (handle.IsValid())
                {
                    handle.Completed -= completedCallback;
                }

                if (completed)
                {
                    return;
                }

                completed = true;
                if (cancellationToken.IsCancellationRequested)
                {
                    if (autoReleaseWhenCanceled && handle.IsValid())
                    {
                        handle.Release();
                    }

                    core.TrySetCanceled(cancellationToken);
                }
                else if (argHandle.Status == AsyncOperationStatus.Failed)
                {
                    core.TrySetException(argHandle.OperationException);
                }
                else
                {
                    core.TrySetResult(argHandle.Result);
                }
            }

            public T GetResult(short token)
            {
                try
                {
                    return core.GetResult(token);
                }
                finally
                {
                    if (!(cancelImmediately && cancellationToken.IsCancellationRequested))
                        TryReturn();
                }
            }

            void IUniTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (completed)
                {
                    return false;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    completed = true;
                    if (autoReleaseWhenCanceled && handle.IsValid())
                    {
                        handle.Release();
                    }

                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                if (progress != null && handle.IsValid())
                {
                    progress.Report(handle.PercentComplete);
                }

                return true;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                handle = default;
                progress = default;
                cancellationToken = default;
                cancellationTokenRegistration.Dispose();
                return pool.TryPush(this);
            }
        }
    }
}
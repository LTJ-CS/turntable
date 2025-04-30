using System;
using Cysharp.Threading.Tasks;
// ReSharper disable once CheckNamespace
using UnityEngine;

namespace MyUI
{
    public enum WaitNetViewType
    {
        Loading         = 1, // 转圈加载
        TryConfirm      = 2, //重试确认
        LoadFailConfirm = 3, //网络数据上传失败的二次确认
    }

    public enum WaitNetTipType
    {
        None,   //在上层做处理
        LoadingAndTryConfirmWithoutClose,       //1.0.5s没返回， 加载loading 2.loading 10s 后，弹出offlineDisplay,不显示关闭取消按钮
        LoadingAndTryConfirmWithClose, //1.0.5s没返回， 加载loading 2.loading 10s 后，弹出offlineDisplay，显示关闭取消按钮
        LoadingAndTryConfirmWithSecond, //1.0.5s没返回， 加载loading 2.loading 10s 后，弹出offlineDisplay，显示关闭取消按钮 3:点击取消，打开二次弹窗提示，
    }

    /// <summary>
    /// WaitNet 的 View 类   
    /// </summary>
    public class WaitNetScreenView : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("loading界面")]
        private LoadingDisplay m_LoadingDisplay; 

        [SerializeField]
        [Tooltip("数据上传失败，二次确认弹窗，适用于比较重要的数据同步")]
        private FailUploadDisplay m_FailUploadDisplay;

        [SerializeField]
        [Tooltip("失败后，确认重试弹窗")]
        private OfflineDisplay m_OfflineConfirmDisplay;
        
        
        private void Start()
        {
            //确认弹窗重试操作
            CommonUIEvents.OnOfflineTry += OnOfflineRetry;
            //确认弹窗关闭操作
            CommonUIEvents.OnCloseOfflineDisplay += OnCloseOfflineDisplay;
            //二次确认弹窗确认放弃操作
            CommonUIEvents.OnGiveUpDataUpLoad += OnGiveUpFailUpLoadDisplay;
            //二次确认弹窗关闭操作
            CommonUIEvents.OnCloseFailUpLoadDisplay += OnCloseFailUpLoadDisplay;
        }

        /// <summary>
        /// 重置界面，关闭所有展示的弹窗
        /// </summary>
        public void ResetView()
        {
            m_LoadingDisplay.Visible = false;
            m_FailUploadDisplay.Visible = false;
            m_OfflineConfirmDisplay.Visible = false;
        }
        //等待玩家操作的返回
        private UniTaskCompletionSource<bool> confirmTaskCompletionSource;

        private WaitNetTipType curWaitNetTipType;
        
        /// <summary>
        /// 展示提示错误信息的弹窗，适用于没有错误码，需要展示具体错误返回的弹窗
        /// </summary>
        /// <param name="errorTip">网络信息提示</param>
        /// <param name="waitNetTipType">弹窗类型</param>
        /// <returns></returns>
        public async UniTask<bool> ShowErrorTipTryConfirm(string errorTip,WaitNetTipType waitNetTipType)
        {
            confirmTaskCompletionSource = new UniTaskCompletionSource<bool>();
            m_LoadingDisplay.Visible = false;
            m_FailUploadDisplay.Visible = false;
            m_OfflineConfirmDisplay.Visible = true;
            PopupShowBtnType popupShowBtnType = waitNetTipType == WaitNetTipType.LoadingAndTryConfirmWithoutClose ? PopupShowBtnType.Confirm : PopupShowBtnType.ConfirmAndCancel;
            curWaitNetTipType = waitNetTipType;
            m_OfflineConfirmDisplay.SetPopupShowBtnType(popupShowBtnType);
            m_OfflineConfirmDisplay.SetConfirmTip(errorTip,string.Empty);
            return await confirmTaskCompletionSource.Task;
        }
        /// <summary>
        /// 展示提示网络错误码的弹窗，适用于网络错误返回
        /// </summary>
        /// <param name="errorCode">网络错误码</param>
        /// <param name="waitNetTipType">弹窗类型</param>
        /// <returns></returns>
        public async UniTask<bool> ShowErrorCodeTryConfirm(string errorCode,WaitNetTipType waitNetTipType)
        {
            confirmTaskCompletionSource = new UniTaskCompletionSource<bool>();
            m_LoadingDisplay.Visible = false;
            m_FailUploadDisplay.Visible = false;
            m_OfflineConfirmDisplay.Visible = true;
            PopupShowBtnType popupShowBtnType = waitNetTipType == WaitNetTipType.LoadingAndTryConfirmWithoutClose ? PopupShowBtnType.Confirm : PopupShowBtnType.ConfirmAndCancel;
            curWaitNetTipType = waitNetTipType;
            m_OfflineConfirmDisplay.SetPopupShowBtnType(popupShowBtnType);
            m_OfflineConfirmDisplay.SetConfirmTip(string.Empty,errorCode);
            return await confirmTaskCompletionSource.Task;
        }
        /// <summary>
        /// 打开loading 界面
        /// </summary>
        public void ShowLoading()
        {
            m_LoadingDisplay.Visible = true;
            m_OfflineConfirmDisplay.Visible = false;
            m_FailUploadDisplay.Visible = false;
        }

        /// <summary>
        /// 当类型是WaitNetTipType.LoadingAndTryConfirmSecond时，
        /// 当点击m_OfflineConfirmDisplay 的关闭按钮时，不会关闭m_OfflineConfirmDisplay，只会 打开二次确认弹窗m_FailUploadDisplay，
        /// 如果点击m_FailUploadDisplay的确认取消，会关闭本次waitNetScreenView，
        /// 如果点击关闭，只会关闭m_FailUploadDisplay
        /// </summary>
        /// <param name="tipContent"></param>
        /// <returns></returns>
        private UniTaskCompletionSource<bool> trySecondTaskCompleteSource;
        public async UniTask<bool> ShowTrySecondConfirm(string tipContent)
        {
            trySecondTaskCompleteSource = new UniTaskCompletionSource<bool>();
            m_FailUploadDisplay.Visible = true;
            m_FailUploadDisplay.SetConfirmTip(tipContent);
            return await trySecondTaskCompleteSource.Task;
        }
  
        /// <summary>
        /// 关闭loading
        /// </summary>
        public void HideLoading()
        {
            m_LoadingDisplay.Visible = false;
        }


        /// <summary>
        /// 确认弹窗的重试操作
        /// </summary>
        void OnOfflineRetry()
        {
            confirmTaskCompletionSource.TrySetResult(true);
        }

        /// <summary>
        /// 确实弹窗的关闭操作
        /// </summary>
        void OnCloseOfflineDisplay()
        {
            if (curWaitNetTipType == WaitNetTipType.LoadingAndTryConfirmWithSecond)
            {
                ShowTrySecondConfirm("").ContinueWith((closeWait) =>
                {
                    if (closeWait)
                    {
                        confirmTaskCompletionSource.TrySetResult(false);
                    }
                    else
                    {
                        m_FailUploadDisplay.Visible = false;
                    }
                }).Forget();
            }
            else
            {
                confirmTaskCompletionSource.TrySetResult(false);
            }
        }

        /// <summary>
        ///  二次确认弹窗确认放弃操作
        /// </summary>
        void OnGiveUpFailUpLoadDisplay()
        {
            trySecondTaskCompleteSource.TrySetResult(true);
            // OnCloseScreenViewEvent?.Invoke(true);
        }

        /// <summary>
        ///  二次确认弹窗确认关闭操作
        /// </summary>
        void OnCloseFailUpLoadDisplay()
        {
            trySecondTaskCompleteSource.TrySetResult(false);
            m_FailUploadDisplay.Visible = false;
            // ShowView(WaitNetViewType.TryConfirm);
        }

        void OnDestroy()
        {
            CommonUIEvents.OnOfflineTry -= OnOfflineRetry;
            CommonUIEvents.OnCloseOfflineDisplay -= OnCloseOfflineDisplay;
            CommonUIEvents.OnGiveUpDataUpLoad -= OnGiveUpFailUpLoadDisplay;
            CommonUIEvents.OnCloseFailUpLoadDisplay -= OnCloseFailUpLoadDisplay;
        }
    }
}
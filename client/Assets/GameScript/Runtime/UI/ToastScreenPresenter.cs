using System;
using System.Collections.Generic;
using UIFramework;
using UIFramework.UIScreen;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace MyUI
{
    /// <summary>
    /// Toast 的 Presenter 类   
    /// </summary>
    public sealed partial class ToastScreenPresenter : IScreenOpen<string, float>, IScreenOpen<string>
    {
        private Queue<(string, float)> contentQueue;
        private string                 curShowContent;
        private bool                   isPlaying;

        #region 一些全局的辅助函数, 方便使用

        /// <inheritdoc cref="OnOpen(string)"/>
        public static void Open(string content)
        {
            if(UIManager.IsValid)
                UIManager.OpenScreen<ToastScreenPresenter, string>(content);
        }
        
        /// <inheritdoc cref="OnOpen(string,float)"/>
        public static void Open(string content, float delayHide)
        {
            if(UIManager.IsValid)
                UIManager.OpenScreen<ToastScreenPresenter, string, float>(content, delayHide);
        }

        /// <summary>
        /// 显示一条错误信息到屏幕上一段时间, 会持续3秒, 以后玩家截图, 同时也会输出一条错误日志
        /// </summary>
        /// <param name="errorDesc">指定要显示的错误</param>
        public static void ShowError(string errorDesc)
        {
            Debug.LogError(errorDesc);
            Open(errorDesc, 3);
        }
        /// <summary>
        /// 显示一条错误信息到屏幕上一段时间, 会持续3秒, 以后玩家截图, 同时也会输出一条错误日志
        /// </summary>
        /// <param name="exception">指定要显示的错误</param>
        public static void ShowException(Exception exception)
        {
            Debug.LogException(exception);
            Open(exception.Message, 3);
        }
        /// <summary>
        /// Logs a formatted error message to the Screen
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        public static void ShowErrorFormat(string format, params object[] args)
        {
            ShowError(string.Format(format, args));
        }        
        /// <summary>
        /// 显示一条弹窗
        /// </summary>
        /// <param name="tip">指定要显示的弹窗信息</param>
        public static void Show(string tip)
        {
            Open(tip);
        }
        #endregion

        /// <summary>
        /// 根据给定 Content 显示指定时间的 Toast
        /// </summary>
        /// <param name="content">指定要显示的内容</param>
        /// <param name="delayHide">延迟多少时间后消失(秒)</param>
        public void OnOpen(string content, float delayHide)
        {
            base.OnOpen();
            ScreenView.OnPlayToast -= OnPlayToast;
            ScreenView.OnPlayToast += OnPlayToast;
            if (contentQueue == null)
                contentQueue = new Queue<(string, float)>();
            if (curShowContent == null || !curShowContent.Equals(content))
                contentQueue.Enqueue((content, delayHide));
            if (!isPlaying)
            {
                OnPlayToast();
            }
        }

        /// <summary>
        /// 显示1秒指定内容的 Toast
        /// </summary>
        /// <param name="content">指定要显示的内容</param>
        public void OnOpen(string content)
        {
            OnOpen(content, 1f);
        }

        void OnPlayToast()
        {
            if (contentQueue.Count > 0)
            {
                var value = contentQueue.Dequeue();
                var content = value.Item1;
                curShowContent = content;
                isPlaying      = true;
                ScreenView.ShowToast(content, value.Item2);
            }
            else
            {
                curShowContent = null;
                UIManager.CloseScreen<ToastScreenPresenter>();
            }
        }

        public override void OnClose()
        {
            ScreenView.OnPlayToast -= OnPlayToast;
            base.OnClose();
        }
    }
}
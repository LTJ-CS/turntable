using System;
using UnityEditor;
using Object = UnityEngine.Object;
namespace Editor.Scripts.Helper
{
    /// <summary>
    /// Unity提示框@sy
    /// </summary>
    public static class UnityTipsHelper
    {
        /// <summary>
        /// 展示提示
        /// </summary>
        /// <param name="content"></param>
        public static void Show(string content)
        {
            EditorUtility.DisplayDialog("提示", content, "确认");
        }

        /// <summary>
        /// 提示 同时error 报错
        /// </summary>
        public static void ShowError(string message)
        {
            Show(message);
            Logger.LogError(message);
        }

        /// <summary>
        /// 提示 同时error 报错
        /// </summary>
        public static void ShowError(object message)
        {
            Show(message.ToString());
            Logger.LogError(message);
        }

        /// <summary>
        /// 提示 同时error 报错
        /// </summary>
        public static void ShowErrorContext(Object context, string message)
        {
            Show(message);
            Logger.LogErrorContext(context, message);
        }

        /// <summary>
        /// 提示 同时error 报错
        /// </summary>
        public static void ShowErrorContext(Object context, object message)
        {
            Show(message.ToString());
            Logger.LogErrorContext(context, message);
        }

        /// <summary>
        /// 确定 取消 回调的提示框
        /// </summary>
        public static void CallBack(string content, Action okCallBack, Action cancelCallBack = null)
        {
            var selectIndex = EditorUtility.DisplayDialogComplex("提示", content, "确认", "取消", null);
            if (selectIndex == 0) //确定
            {
                try
                {
                    okCallBack?.Invoke();
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }
            }
            else
            {
                try
                {
                    cancelCallBack?.Invoke();
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// 只有确定的提示框
        /// </summary>
        public static void CallBackOk(string content, Action okCallBack, Action cancelCallBack = null)
        {
            var result = EditorUtility.DisplayDialog("提示", content, "确认");
            if (result) //确定
            {
                try
                {
                    okCallBack?.Invoke();
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }
            }
            else
            {
                try
                {
                    cancelCallBack?.Invoke();
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }
            }
        }
    }
}
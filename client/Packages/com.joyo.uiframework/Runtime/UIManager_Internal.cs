using UIFramework.UIScreen;
#if UNITY_EDITOR
#endif

// ReSharper disable once CheckNamespace
namespace UIFramework
{
    /// <summary>
    /// UIFramework 内部调用的 UIManager 函数写在这里
    /// </summary>
    partial class UIManager
    {
        /// <inheritdoc cref="UIManager.IsScreenOpenInternal"/>
        internal static bool IsScreenOpen(UIScreenPresenterBase screenPresenter)
        {
            return Instance.IsScreenOpenInternal(screenPresenter);
        }

        /// <summary>
        /// 获取给定的 screenPresenter 是否是打开状态
        /// </summary>
        /// <param name="screenPresenter">指定要获取打开状态的 screenPresenter</param>
        /// <returns></returns>
        private bool IsScreenOpenInternal(UIScreenPresenterBase screenPresenter)
        {
            var uiLoader = GetScreenLoader(screenPresenter.GetType());
            return uiLoader != null && uiLoader.IsOpened;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 内部用函数, 目前只用于退出运行模式时清理窗口的打开状态用
        /// </summary>
        private void CloseAllScreensImmediately()
        {
            for (int i = _uiLayers.Length - 1; i >= 0; i--)
            {
                var uiLayer = _uiLayers[i];
                UIScreenLoader screenLoader;
                while ((screenLoader = uiLayer.PopScreen()) != default)
                {
                    if (screenLoader.IsOpened)
                        screenLoader.Close();
                }
            }
        }
#endif
    }
}
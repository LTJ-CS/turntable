using System.Collections.Generic;
using UIFramework.Base;

// ReSharper disable once CheckNamespace
namespace UIFramework.UIScreen
{
    /// <summary>
    /// 管理一个 UI 层
    /// </summary>
    internal class UILayer
    {
        /// <summary>
        /// 存储本层的已经打开的 Screen
        /// </summary>
        private readonly Stack<UIScreenLoader> _screenStack = new();

        /// <summary>
        /// 获取本层的所有 Screen
        /// </summary>
        public IReadOnlyCollection<UIScreenLoader> ScreenLoaders => _screenStack;
        
        /// <summary>
        /// 保存所有的子层, 关闭本层时会自动关闭子层
        /// </summary>
        private readonly List<EUILayer> _childLayers = new();
        
        internal IReadOnlyList<EUILayer> ChildLayers => _childLayers;
        
        /// <summary>
        /// 添加子层到当前层。
        /// </summary>
        /// <param name="childLayer">要添加的子层。</param>
        public void AddChildLayer(EUILayer childLayer)
        {
            _childLayers.Add(childLayer);
        }

        /// <summary>
        /// 压入一个 Screen 
        /// </summary>
        /// <param name="screen">指定要压入的 Screen</param>
        internal void PushScreen(UIScreenLoader screen)
        {
            _screenStack.Push(screen);
        }

        /// <summary>
        /// 弹出一个 Screen
        /// </summary>
        /// <returns></returns>
        internal UIScreenLoader PopScreen()
        {
            return _screenStack.Count == 0 ? default : _screenStack.Pop();
        }
        
        /// <summary>
        /// 获取当前层栈顶的 Screen
        /// </summary>
        /// <returns></returns>
        internal UIScreenLoader TopScreen()
        {
            return _screenStack.Count == 0 ? default : _screenStack.Peek();
        }

        /// <summary>
        /// 查找给定的 ScreenPresenter 是否在当前层栈中
        /// </summary>
        /// <param name="screenPresenter">指定要查找的 ScreenPresenter</param>
        /// <returns></returns>
        internal bool HasScreen(UIScreenPresenterBase screenPresenter)
        {
            foreach (var uiScreenLoader in _screenStack)
            {
                if (uiScreenLoader.ScreenPresenter == screenPresenter)
                    return true;
            }

            return false;
        }
        
        
    }
}
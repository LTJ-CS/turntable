using System;
using UIFramework.Base;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace UIFramework.UIScreen
{
    /// <summary>
    /// 保存 UI 设计时的信息的类
    /// </summary>
    public class UIScreenDesignInfo
    {
        public EUILayer Layer { get; private set; }

        public UIScreenDesignInfo(EUILayer layer)
        {
            Layer = layer;
        }
    }

    /// <summary>
    /// UI 设计模式中的 Presenter 的基类, 所有 UI 的 Presenter 都从它派生
    /// </summary>
    public abstract class UIScreenPresenterBase
    {
        /// <summary>
        /// 返回对应 View 的类型
        /// </summary>
        protected abstract Type ViewType { get; }

        /// <summary>
        /// 返回 View 对应的层级
        /// </summary>
        public abstract EUILayer Layer { get; }

        /// <summary>
        /// 返回对应的 Screen 的名字
        /// </summary>
        public abstract string ScreenName { get; }

        /// <summary>
        /// 返回背景显示类型
        /// </summary>
        public virtual EBackgroundShowType BackgroundShowType { get; } = EBackgroundShowType.None;

        /// <summary>
        /// 背景的颜色
        /// </summary>
        public virtual Color BackgroundColor => Color.clear;

        protected UIScreenPresenterBase()
        {
        }

        #region 仅供 UIManager 调用的函数

        /// <summary>
        /// 打开前调用
        /// </summary>
        internal virtual void OnPreOpen()
        {
        }

        /// <summary>
        /// 关闭后调用
        /// </summary>
        internal virtual void OnPostClose()
        {
        }

        /// <summary>
        /// Screen 被打开时调用
        /// </summary>
        public virtual void OnOpen()
        {
        }

        /// <summary>
        /// Screen 被关闭时调用
        /// </summary>
        public virtual void OnClose()
        {
        }

        #endregion

        /// <summary>
        /// 窗口是否是打开状态
        /// </summary>
        public bool IsOpen => UIManager.IsScreenOpen(this);

        /// <summary>
        /// 关闭本 Screen
        /// </summary>
        protected void Close()
        {
            UIManager.CloseScreen(this);
        }

        /// <summary>
        /// 用于设置View
        /// </summary>
        /// <param name="screenGameObject">指定包含要设置 View 的GameObject</param>
        /// <remarks>仅供 UIScreenLoader 调用, 用于设置加载完成的 View</remarks>
        internal abstract void InitializeView(GameObject screenGameObject);

        public abstract class Builder
        {
            /// <summary>
            /// Screen Presenter 的构建器, 消除 UIScreenPresenterBase 被外面构建
            /// </summary>
            /// <typeparam name="TScreenPresenter">指定要构建的具体的 ScreenPresenter 类型</typeparam>
            /// <returns></returns>
            internal static TScreenPresenter Build<TScreenPresenter>()
                where TScreenPresenter : UIScreenPresenterBase, new()
            {
                var presenter = new TScreenPresenter();
                return presenter;
            }
        }
    }

    /// <summary>
    /// 基于泛型的屏幕Presenter基类，遵循UI设计模式
    /// </summary>
    /// <typeparam name="TScreenView">UI Screen View 类型，需继承自UIScreenViewBase</typeparam>
    public abstract class GenericScreenPresenterBase<TScreenView> : UIScreenPresenterBase
        where TScreenView : UIScreenViewBase
    {
        /// <summary>
        /// Screen 对应的 View
        /// </summary>
        protected TScreenView ScreenView;

        protected override Type ViewType => typeof(TScreenView);

        internal override void OnPostClose()
        {
            #if UNITY_EDITOR
            ScreenView.CloseCheck();
            #endif
            base.OnPostClose();
        }

        internal override void InitializeView(GameObject screenGameObject)
        {
            ScreenView = screenGameObject.GetComponent<TScreenView>();
            if (ScreenView == null)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve associated View ({typeof(TScreenView)}) component for {GetType().FullName}");
            }
        }
    }
}
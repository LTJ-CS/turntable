using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UIFramework.Base;
using UIFramework.Extensions;
using UIFramework.Utils;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace UIFramework.UIScreen
{
    /// <summary>
    /// UI 设计模式中的 View 的基类, 所有 UI 的 View 都从它派生
    /// </summary>
    public abstract class UIScreenViewBase : MonoBehaviour
    {
#if UNITY_EDITOR
        /// <summary>
        /// 用于保存 View 申请的 DelegateList 列表, 方便监控未释放的 DelegateList
        /// </summary>
        private readonly List<IDelegateList> _delegateLists = new();

        /// <summary>
        /// 关闭前的检测
        /// </summary>
        internal void CloseCheck()
        {
            foreach (var delegateList in _delegateLists)
            {
                if (delegateList.Count > 0)
                {
                    Debug.LogError($"{nameof(UIScreenViewBase)}: {name} 存在还在监听中的 DelegateList, 请检查代码");
                }
            }
        }
#endif

        [SerializeField]
        [Tooltip("Screen 的行为配置, 自动导出项, 切记不要手动修改, 仅供查看")]
        // [ReadOnly]
        private ScreenBehaviour m_ScreenBehaviour;

        public ScreenBehaviour ScreenBehaviour => m_ScreenBehaviour;

        public const string ScreenBehaviourName = nameof(m_ScreenBehaviour);

        /// <summary>
        /// 申请一个指定类型的 DelegateList
        /// </summary>
        /// <typeparam name="T">指定要申请的 DelegateList 的类型</typeparam>
        /// <returns></returns>
        /// <remarks>方便做回调释放的监控, 避免忘记释放, 减少gc, 提升性能</remarks>
        protected DelegateList<T> CreateDelegateList<T>()
        {
            var delegateList = DelegateList<T>.CreateWithGlobalCache();
#if UNITY_EDITOR
            _delegateLists.Add(delegateList);
#endif
            return delegateList;
        }

        /// <summary>
        /// 申请一个不带参数的 DelegateList
        /// </summary>
        /// <returns></returns>
        /// <remarks>方便做回调释放的监控, 避免忘记释放, 减少gc, 提升性能</remarks>
        protected DelegateList CreateDelegateList()
        {
            var delegateList = DelegateList.CreateWithGlobalCache();
#if UNITY_EDITOR
            _delegateLists.Add(delegateList);
#endif
            return delegateList;
        }

        /// <summary>
        /// 申请一个不带参数
        /// </summary>
        protected virtual void OnDestroy()
        {
#if UNITY_EDITOR
            CloseCheck();
            _delegateLists.Clear();
#endif
        }

        /// <summary>
        /// 播放关闭动画，动画结束后执行回调，如果没有绑定动画组件，则直接执行回调
        /// </summary>
        /// <param name="finishAc">结束回调</param>
        protected void PlayCloseAnimation(Action finishAc)
        {
            var screenAnimation = GetComponent<IScreenAnimation>();
            if (screenAnimation == null)
            {
                finishAc?.Invoke();
                return;
            }

            var dur = screenAnimation.PlayCloseAnimation();
            DOTween.Sequence()
                   .SetLink(gameObject)
                   .AppendInterval(dur)
                   .AppendCallback(() =>
                   {
                       if (this != null)
                       {
                           finishAc?.Invoke();
                       }
                   }).Play();
        }
    }
}
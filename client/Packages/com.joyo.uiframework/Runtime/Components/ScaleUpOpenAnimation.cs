using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using UIFramework.Base;
using UIFramework.UIScreen;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace UIFramework.Components
{
    /// <summary>
    /// 实现了打开窗口时由小到大的动画效果
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScaleUpOpenAnimation : MonoBehaviour, IScreenAnimation
    {
        public Transform m_PopRoot;

        /// <summary>
        /// 缓冲本节点的 CanvasGroup
        /// </summary>
        private CanvasGroup _canvasGroup;

        /// <summary>
        /// 页面配置信息
        /// </summary>
        private ScreenBehaviour _behaviour;


        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _behaviour = GetComponent<UIScreenViewBase>()?.ScreenBehaviour;

            if (!m_PopRoot)
            {
                Debug.Log("没有绑定popRoot");
            }

            PlayOpenAnimation();
        }

        /// <summary>
        /// 播放打开动画前的初始化准备
        /// </summary>
        private void PrePlayOpenAnimation()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.alpha = 0;
            if (m_PopRoot)
            {
                m_PopRoot.localScale = Vector3.zero;
            }
        }

        public float PlayOpenAnimation()
        {
            // 判断是否自动播放打开动画
            if (_behaviour?.OpenAnimPlayMode == EOpenAnimPlayMode.ControlBySelf)
            {
                return 0f;
            }

            PrePlayOpenAnimation();

            // 淡入的效果
            var sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.Append(_canvasGroup.DOFade(0.5f, 0.01f))
                    .Append(_canvasGroup.DOFade(1f, 0.03f))
                    .Play();
            var duration1 = sequence.Duration();

            // 从小到大缩放的效果, 参考羊窗口的打开动画
            var duration2 = 0f;
            if (m_PopRoot)
            {
                sequence = DOTween.Sequence().SetLink(gameObject);
                sequence.Append(m_PopRoot.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.01f))
                        .Append(m_PopRoot.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.06f))
                        .Append(m_PopRoot.DOScale(Vector3.one, 0.13f))
                        .Play();
                duration2 = sequence.Duration();
            }

            // 延迟接受点击
            var maxDur = Mathf.Max(duration1, duration2);
            sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.AppendInterval(maxDur)
                    .AppendCallback(() => { _canvasGroup.interactable = true; })
                    .Play();
            return maxDur;
        }

        public float PlayCloseAnimation()
        {
            if (_behaviour?.CloseAnimPlayMode == ECloseAnimPlayMode.ControlBySelf)
            {
                return 0f;
            }

            // 屏蔽点击事件
            _canvasGroup.interactable = false;

            var dur = 0.1f;

            // 淡出效果
            var sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.Append(_canvasGroup.DOFade(0f, dur));
            sequence.Play();

            // 从大到小的缩放效果
            if (m_PopRoot)
            {
                var scaleAim = 0.7f;
                sequence = DOTween.Sequence().SetLink(gameObject);
                sequence.Append(m_PopRoot.DOScale(new Vector3(scaleAim, scaleAim, scaleAim), dur));
                sequence.Play();
            }

            // return Mathf.Max(duration1, duration2);
            return dur;
        }

    }
}
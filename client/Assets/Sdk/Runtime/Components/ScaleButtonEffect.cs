using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sdk.Runtime.Components
{
    /// <summary>
    /// 实现一个按钮点击缩放的效果
    /// </summary>
    [RequireComponent(typeof(Button))]
    [Tooltip("实现一个按钮点击缩放的效果")]
    public class ScaleButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("标识当点击时是否自动禁用交互一段时间")] [SerializeField]
        private bool m_AutoDisableInteractive = false;

        [Tooltip("当 AutoDisableInteractive 为true时表示禁用交互的时间(秒)")] [SerializeField] [Range(0.1f, 5)]
        private float m_AutoDisableInteractiveTime = 0.5f;

        [Tooltip("按钮x轴方向是否是镜像")]
        [SerializeField]
        private bool m_IsFlipX = false;

        [Tooltip("实际缩放的节点, 如果不绑定则缩放当前节点")]
        [SerializeField]
        [CanBeNull]
        private Transform m_Transform = null;

        private Transform _transform;

        private void Awake()
        {
            _transform = m_Transform == null ? transform : m_Transform;
            RefreshButtonClickListener();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_IsFlipX)
            {
                _transform.DOScale(new Vector3(-0.9f, 0.9f, 0.9f), 0.15f)
                          .SetEase(Ease.InOutSine).SetLink(gameObject);
            }
            else
            {
                _transform.DOScale(0.9f, 0.15f)
                          .SetEase(Ease.InOutSine).SetLink(gameObject);
            }
        }

        /// <summary>
        /// 点击按钮时触发
        /// </summary>
        private void OnButtonClick()
        {
            var button = GetComponent<Button>();
            button.interactable = false;
            DOVirtual.DelayedCall(m_AutoDisableInteractiveTime, EnableInteractive);
        }

        private void EnableInteractive()
        {
            if (this == null)
                return;

            var button = GetComponent<Button>();
            if (button == null)
                return;
            button.interactable = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            RefreshButtonClickListener();
        }
#endif

        /// <summary>
        /// 刷新按钮的点击事件监听器
        /// </summary>
        private void RefreshButtonClickListener()
        {
            var button = GetComponent<Button>();
            button.onClick.RemoveListener(OnButtonClick);
            if (m_AutoDisableInteractive)
            {
                button.onClick.AddListener(OnButtonClick);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (m_IsFlipX)
            {
                _transform.DOScale(new Vector3(-1, 1, 1), 0.15f)
                          .SetEase(Ease.InOutSine).SetLink(gameObject);
            }
            else
            {
                _transform.DOScale(1f, 0.15f)
                          .SetEase(Ease.InOutSine).SetLink(gameObject);
            }
        }
    }
}
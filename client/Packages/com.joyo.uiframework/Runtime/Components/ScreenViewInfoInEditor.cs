#if UNITY_EDITOR
using UIFramework.Base;
using UIFramework.Extensions;
using UIFramework.UIScreen;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
namespace UIFramework.Components
{
    /// <summary>
    /// 保存 UI Screen 的编辑时信息, 不可以在运行时使用, 用于保存 UI Screen 的编辑时信息
    /// </summary>
    [RequireComponent(typeof(UIScreenViewBase))]
    public class ScreenViewInfoInEditor : MonoBehaviour, IEditorOnlyComponent
    {
        [ReadOnly]
        [Tooltip("导出的运行时的 UI Screen Prefab, 导出成功后才有")]
        public GameObject m_RunTimePrefab;

        [ReadOnly]
        [Tooltip("制作 UI 时的参考图对象, 导出时会被删除掉")]
        public RawImage m_ReferenceImage;

        [ReadOnly]
        [Tooltip("背景 Image, 编辑时演示用的背景效果")]
        public RawImage m_BackgroundImage;

        [Header("UI 属性")]
        [SerializeField]
        [Tooltip("UI 所在的层级")]
        private EUILayer m_Layer;

        public       EUILayer Layer => m_Layer;
        public const string   NameOfLayer = nameof(m_Layer);

        [SerializeField]
        [Tooltip("Screen 的类型, 简化下面的配置")]
        private EScreenType m_ScreenType;

        public EScreenType ScreenType => m_ScreenType;

        [SerializeField]
        [Tooltip("Screen 的行为")]
        private ScreenBehaviour m_Behaviour = new();

        [SerializeField]
        [Tooltip("背景的显示方式")]
        private EBackgroundShowType m_BackgroundShowShowType;

        public EBackgroundShowType BackgroundShowShowType => m_BackgroundShowShowType;

        [SerializeField]
        [Tooltip("自定义的背景颜色")]
        private Color m_CustomBackgroundColor;

        public Color CustomBackgroundColor => m_CustomBackgroundColor;

        [SerializeField]
        [Tooltip("自定义的背景点击事件的回调")]
        private UnityEvent m_BackgroundClickEventCallback;

        public UnityEvent BackgroundClickEventCallback => m_BackgroundClickEventCallback;

        public       ScreenBehaviour Behaviour => m_Behaviour;
        public const string          NameOfBehaviour = nameof(m_Behaviour);

        protected void Reset()
        {
            m_Layer = EUILayer.Screen;
            m_Behaviour.Reset();
        }

        protected void OnValidate()
        {
            switch (m_ScreenType)
            {
                case EScreenType.Underlay:
                    m_BackgroundShowShowType = EBackgroundShowType.None;
                    break;
                case EScreenType.Overlay:
                    m_BackgroundShowShowType = EBackgroundShowType.None;
                    break;
                case EScreenType.Window:
                    m_BackgroundShowShowType  = EBackgroundShowType.SemiTransparent;
                    m_CustomBackgroundColor.a = ScreenBehaviour.SemiTransparentAlpha;
                    break;
                case EScreenType.Float:
                    m_BackgroundShowShowType = EBackgroundShowType.None;
                    break;
                case EScreenType.System:
                    m_BackgroundShowShowType  = EBackgroundShowType.SemiTransparent;
                    m_CustomBackgroundColor.a = ScreenBehaviour.SemiTransparentAlpha;
                    break;
            }

            m_Behaviour.OnValidate(m_ScreenType);
            if (m_BackgroundImage != null)
            {
                m_BackgroundImage.color = CustomBackgroundColor;
            }
        }
    }
}
#endif
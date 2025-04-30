using UnityEngine;
using System;
using UIFramework.Extensions;
using UnityEngine.Events;

// ReSharper disable once CheckNamespace
namespace UIFramework.Base
{
    /// <summary>
    /// 定义一些常用的 Screen 的类型.
    /// </summary>
    public enum EScreenType
    {
        Underlay,    //【铺垫/场景型/全屏界面、一级功能界面等】：无背景, 不阻挡下方事件, 点击背景不关闭自身；返回键按下时不响应
        Overlay,     //【叠加/局部类型界面（如Home主菜单、较重界面拆分界面等】：无背景, 阻挡下方事件, 点击背景默认关闭自身；返回键按下时不响应
        Window,      //【弹出型功能界面、确认框等】：有背景（黑色半透、阻挡下方事件、点击背景默认关闭自身）；获得焦点；返回键按下时大概率关闭自身
        Float,       //【浮动功能气泡（如聊天气泡）、Toast等】：无背景、不抢夺焦点；返回键按下时大概率不检测
        System,      //【网络转圈等待、引导界面等】：有背景（黑色半透、阻挡下方事件、点击背景不响应）、不抢夺焦点；返回键按下时大概率不响应
        Custom       // 自定义各个属性, 比如是否有背景, 是否阻塞下方事件, 是否阻塞返回键等
    }

    /// <summary>
    /// 背景展示类型
    /// </summary>
    public enum EBackgroundShowType
    {
        None,             // 无背景
        FullyTransparent, // 完全透明
        SemiTransparent,  // 半透明
        CustomColor       // 自定义颜色
    }

    /// <summary>
    /// 定义了背景点击事件的类型, 仅当 EBackgroundType 不为 None 时有效
    /// </summary>
    public enum EBackgroundClickEventType
    {
        PassThrough,          // 点击事件穿透到背景，不进行任何处理
        BlockWithoutResponse, // 点击事件被阻挡，但不触发任何响应动作
        CloseSelf,            // 点击事件导致当前Screen关闭
        CustomCallback        // 点击事件触发自定义的回调函数
    }

    /// <summary>
    /// 定义了获取焦点的类型。
    /// </summary>
    public enum EGetFocusType
    {
        DontGet,      // 不主动获取焦点
        Get,          // 主动获取焦点
        GetWithOthers // 与其它可获取焦点的界面一同获得焦点
    }

    /// <summary>
    /// 枚举表示返回键按下事件的类型。
    /// </summary>
    public enum EEscPressEventType
    {
        DontCheck,    // 不检测、跳过
        DontResponse, // 检测但不响应
        CloseSelf,    // 关闭自身
        Custom        // 触发自定义回调
    }

    /// <summary>
    /// 打开动画的播放方式
    /// </summary>
    public enum EOpenAnimPlayMode
    {
        AutoPlay,     // 自动播放
        ControlBySelf // 由代码控制是否播放
    }

    /// <summary>
    /// 关闭动画的播放方式
    /// </summary>
    public enum ECloseAnimPlayMode
    {
        AutoPlay,     // 自动播放
        ControlBySelf // 由代码控制是否播放
    }

    /// <summary>
    /// 封装 Screen 行为的定义
    /// </summary>
    [Serializable]
    public class ScreenBehaviour
    {
        [SerializeField]
        [Tooltip("背景点击的事件相应类型")]
        [ReadOnly]
        private EBackgroundClickEventType m_BackgroundClickEventType = EBackgroundClickEventType.PassThrough;

        public EBackgroundClickEventType BackgroundClickEventType => m_BackgroundClickEventType;
        
        [SerializeField]
        [Tooltip("自定义的背景点击事件回调函数, 当 BackgroundClickEventType 为 EBackgroundClickEventType.CustomCallback 调用, 切记不要在这里直接修改, 会被导出自动覆盖掉")]
        private UnityEvent m_BackgroundClickEventCallback;

        public UnityEvent BackgroundClickEventCallback
        {
            get => m_BackgroundClickEventCallback;
            #if UNITY_EDITOR  // 仅在编辑器中使用
            set => m_BackgroundClickEventCallback = value;
            #endif
        }

        [SerializeField]
        [Tooltip("返回键按下时的事件响应类型")]
        [ReadOnly]
        private EEscPressEventType m_EscPressEventType = EEscPressEventType.DontResponse;

        public EEscPressEventType EscPressEventType => m_EscPressEventType;

        [SerializeField]
        [Tooltip("打开动画的播放方式")]
        [ReadOnly]
        private EOpenAnimPlayMode m_OpenAnimPlayMode = EOpenAnimPlayMode.AutoPlay;

        public EOpenAnimPlayMode OpenAnimPlayMode => m_OpenAnimPlayMode;

        [SerializeField]
        [Tooltip("关闭动画的播放方式")]
        [ReadOnly]
        private ECloseAnimPlayMode m_CloseAnimPlayMode = ECloseAnimPlayMode.AutoPlay;

        public ECloseAnimPlayMode CloseAnimPlayMode => m_CloseAnimPlayMode;
        
        /// <summary>
        /// 半透明背景的 Alpha 值
        /// </summary>
        public const float SemiTransparentAlpha = 215/255.0f;

        // [SerializeField]
        // [Tooltip("是否主动获取焦点")]
        // private EGetFocusType m_GetFocusType;

#if UNITY_EDITOR
        public const string NameOfBackgroundClickEventType = nameof(m_BackgroundClickEventType);
        public const string NameOfEscPressEventType        = nameof(m_EscPressEventType);
        public const string NameOfOpenAnimPlayMode         = nameof(m_OpenAnimPlayMode);
        public const string NameOfCloseAnimPlayMode        = nameof(m_CloseAnimPlayMode);

        /// <summary>
        /// Editor 中重置 Inspector 时调用
        /// </summary>
        public void Reset()
        {
            m_BackgroundClickEventType = EBackgroundClickEventType.PassThrough;
            m_EscPressEventType        = EEscPressEventType.DontResponse;
            m_OpenAnimPlayMode         = EOpenAnimPlayMode.AutoPlay;
            m_CloseAnimPlayMode        = ECloseAnimPlayMode.AutoPlay;
        }

        /// <summary>
        /// Editor 中修改值时调用
        /// </summary>
        public void OnValidate(EScreenType screenType)
        {
            // 根据不同的 ScreenType 设置一些默认值, 减少修改成本
            switch (screenType)
            {
                case EScreenType.Underlay:
                    m_BackgroundClickEventType = EBackgroundClickEventType.PassThrough;
                    m_EscPressEventType        = EEscPressEventType.DontCheck;
                    break;
                case EScreenType.Overlay:
                    m_BackgroundClickEventType = EBackgroundClickEventType.BlockWithoutResponse;
                    m_EscPressEventType        = EEscPressEventType.DontCheck;
                    break;
                case EScreenType.Window:
                    m_BackgroundClickEventType = EBackgroundClickEventType.CloseSelf;
                    m_EscPressEventType        = EEscPressEventType.CloseSelf;
                    break;
                case EScreenType.Float:
                    m_BackgroundClickEventType = EBackgroundClickEventType.PassThrough;
                    m_EscPressEventType        = EEscPressEventType.DontCheck;
                    break;
                case EScreenType.System:
                    m_BackgroundClickEventType = EBackgroundClickEventType.BlockWithoutResponse;
                    m_EscPressEventType        = EEscPressEventType.DontResponse;
                    break;
                case EScreenType.Custom:
                    break;
            }
        }
#endif
    }
}
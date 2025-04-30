// ReSharper disable once CheckNamespace
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace UIFramework.Base
{
    /// <summary>
    /// 定义 UI 的 Layer
    /// </summary>
    /// <remarks>
    /// 不要随便修改枚举的名称, 因为它们会被保存到 Screen 的 prefab 中, 可以增加枚举, 不要减少枚举, 也可以调整顺序, 决定了渲染的顺序, 按值从小到大的顺序渲染.
    /// 同一层只能同时存在一个 UI
    /// "_" 用于标识子层, 比如: Screen_1, Screen_2, Screen_3 都是 Screen 的子层, 且按先后顺序渲染
    /// </remarks>
    // ReSharper disable once InconsistentNaming
    public enum EUILayer : uint
    {
        Cache = 0,   // 缓存层, 需要缓存的暂存的丢这里, 界面不做显示, 会被强制禁用
        InternalUse, // 以上是内部使用的层

        [Tooltip("最低层, 要显示的这个就是最低的层级")]
        Bottom = InternalUse,

        [Tooltip("场景层, 比如血条飘字不是做在3D时 用2D实现时的层")]
        Scene,

        [Tooltip("主 UI 层, 主界面, 不受回退功能影响, 一般玩家不能手动关闭它")]
        ScreenHome,

        // TODO: 定义可以被一键关闭的层的枚举
        [Tooltip("普通Screen层, 全屏界面, 所有Screen打开关闭受回退功能影响, 一般玩家可以手动关闭")]
        Screen,

        // ReSharper disable once InconsistentNaming
        [Tooltip("普通Screen层的子层, 关闭 Screen 时会自动关闭它, 全屏界面所有Screen打开关闭受回退功能影响, 一般玩家可以手动关闭")]
        Screen_1,

        // ReSharper disable once InconsistentNaming
        [Tooltip("普通Screen层的子层, 关闭 Screen 时会自动关闭它, 全屏界面所有Screen打开关闭受回退功能影响, 一般玩家可以手动关闭")]
        Screen_2,

        // ReSharper disable once InconsistentNaming
        [Tooltip("普通Screen层的子层, 关闭 Screen 时会自动关闭它, 全屏界面所有Screen打开关闭受回退功能影响, 一般玩家可以手动关闭")]
        Screen_3,

        [Tooltip("弹窗层, 一般是非全屏界面, 一般玩家可以手动关闭")]
        Popup,

        [Tooltip("提示层, 一般 提示飘字, 跑马灯之类的")]
        Tips,

        [Tooltip("最高层, 一般新手引导之类的")]
        Top,

        [Tooltip("上面枚举的数量")]
        Count,

        [Tooltip("所有层的位标识")]
        AllLayerFlag = 0xFFFFFFFF
    }
}
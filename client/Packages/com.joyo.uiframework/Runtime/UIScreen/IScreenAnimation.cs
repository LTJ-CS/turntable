// ReSharper disable once CheckNamespace
namespace UIFramework.UIScreen
{
    /// <summary>
    /// 定义 Screen 打开与关闭的动画接口, 实现了此接口表示在 Screen 打开或关闭时会触发动画
    /// </summary>
    public interface IScreenAnimation
    {
        /// <summary>
        /// 当播放打开窗口动画时触发调用
        /// </summary>
        /// <returns>返回动画需要的时间(秒), 小于等于 0f 时表示不需要播放动画</returns>
        /// <remarks>动画的时间需要与动画的表演时长尽量一致, 否则会导致用户感觉不好</remarks>
        float PlayOpenAnimation();
        
        /// <summary>
        /// 当播放关闭窗口动画时触发调用
        /// </summary>
        /// <returns>返回动画需要的时间(秒), 小于等于 0f 时表示不需要播放动画</returns>
        /// <remarks>动画的时间需要与动画的表演时长尽量一致, 否则会导致用户感觉不好</remarks>
        float PlayCloseAnimation();
    }
}
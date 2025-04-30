
// ReSharper disable once CheckNamespace
namespace UIFramework.UIScreen
{
    /// <summary>
    /// UI 打开的接口, 用于扩展不同的打开参数
    /// </summary>
    public interface IScreenOpen
    {
    }

    /// <summary>
    /// 带有一个参数的 UI 打开接口
    /// </summary>
    /// <typeparam name="P1">参数类型 1</typeparam>
    public interface IScreenOpen<in P1> : IScreenOpen
    {
        void OnOpen(P1 p1);
    }
    
    /// <summary>
    /// 带有两个参数的 UI 打开接口
    /// </summary>
    public interface IScreenOpen<in P1, in P2> : IScreenOpen
    {
        void OnOpen(P1 p1, P2 p2);
    }
    
    /// <summary>
    /// 带有三个参数的 UI 打开接口
    /// </summary>
    public interface IScreenOpen<in P1, in P2, in P3> : IScreenOpen
    {
        void OnOpen(P1 p1, P2 p2, P3 p3);
    }
    
    /// <summary>
    /// 带有四个参数的 UI 打开接口
    /// </summary>
    public interface IScreenOpen<in P1, in P2, in P3, in P4> : IScreenOpen
    {
        void OnOpen(P1 p1, P2 p2, P3 p3, P4 p4);
    }
    
    /// <summary>
    /// 带有五个参数的 UI 打开接口
    /// </summary>
    public interface IScreenOpen<in P1, in P2, in P3, in P4, in P5> : IScreenOpen
    {
        void OnOpen(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
    }
}
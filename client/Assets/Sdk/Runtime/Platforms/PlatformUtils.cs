using UnityEditor;
namespace Sdk.Runtime.Platforms
{
    /// <summary>
    /// 定义平台
    /// </summary>
    public static class PlatformUtils
    {
        private static EHostPlatform GetHostPlatform()
        {
            // TODO: 继续完善不同平台的识别
            return EHostPlatform.Editor;
        }

        /// <summary>
        /// 产品级的 Profile 的名称
        /// </summary>
        public const string ProductionProfileName = "Production";

        /// <summary>
        /// 主机的运行平台, 用于与服务器通信
        /// </summary>
        public static EHostPlatform HostPlatform { get; } = GetHostPlatform();

        /// <summary>
        /// 获取当前的 BuildTarget 的名称
        /// </summary>
        public static string BuildTargetName
        {
            get
            {
                #if UNITY_EDITOR
                return EditorUserBuildSettings.activeBuildTarget.ToString();
                #elif UNITY_WEBGL
                return "WebGL";
                #elif UNITY_ANDROID
                return "Android";
                #else
                throw new System.NotImplementedException();
                #endif
            }
        }
    }
}
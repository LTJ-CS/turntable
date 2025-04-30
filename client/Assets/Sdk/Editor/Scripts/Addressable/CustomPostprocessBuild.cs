using UnityEditor.Build;
using UnityEditor.Build.Reporting;
namespace Sdk.Editor.Scripts.Addressable
{
    /// <summary>
    /// 用于在 Build 成功后把 Addressable 的配置文件复制到 打包目录下
    /// </summary>
    public class CustomPostprocessBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder => 9999;
        public void OnPostprocessBuild(BuildReport report)
        {
            
        }
    }
}
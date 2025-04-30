using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace GameScript.Editor.PlayMakerUtilities
{
    /// <summary>
    /// 用于在 Build 成功后把 Addressable 的配置文件复制到 打包目录下
    /// </summary>
    public class PlayMakerPostProcessBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder => 9998;
        public void OnPostprocessBuild(BuildReport report)
        {
            // 暂时不需要这个功能, 因为我们经常要查看导出的 PlayMaker 的 link.xml 文件, 以确定 Action 没有被 Stripping 掉
            // 移除临时生成的 PlayMaker 的 link.xml 文件
            // AssetDatabase.DeleteAsset(PlayMakerBuildProcessor.PlayMakeLinkXmlPath);
            // Debug.Log($"[PlayMakerPostProcessBuild] 删除了 {PlayMakerBuildProcessor.PlayMakeLinkXmlPath}");
        }
    }
}
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
namespace Sdk.Editor.Scripts.TMPBuildScript
{
    /// <summary>
    /// 为了减少首包大小, 我们不需要在 TMP_Settings 中设置默认字体, 否则它会被打包进首包中, 增加了大小
    /// </summary>
    public class TMPBuildScript : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        private TMP_FontAsset originalFont;
        
        
        public void OnPreprocessBuild(BuildReport report)
        {
            // 清理 TMP 的默认字体
            originalFont = TMP_Settings.GetFontAsset();
            var fieldInfo = typeof(TMP_Settings).GetField("m_defaultFontAsset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            fieldInfo.SetValue(TMP_Settings.instance, null);
            EditorUtility.SetDirty(TMP_Settings.instance);
            // AssetDatabase.SaveAssets();
            // AssetDatabase.Refresh();
        }
        
        public void OnPostprocessBuild(BuildReport report)
        {
            // 恢复 TMP 默认字体
            var fieldInfo = typeof(TMP_Settings).GetField("m_defaultFontAsset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            fieldInfo.SetValue(TMP_Settings.instance, originalFont);
            EditorUtility.SetDirty(TMP_Settings.instance);
        }
        
    }
}
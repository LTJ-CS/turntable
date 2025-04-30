using UnityEditor.Build;

namespace Sdk.Editor.Scripts.Addressable
{
    /// <summary>
    /// 实现对 AA 打包过程的干涉, 当打包到 WebGL(微信小游戏)时去掉它自动打包到 StreamingAssets 中的 link.xml, Settings.json 等文件,
    /// 因为这些文件我们会通过远程下载, 减少首包大小以及实现资源的热更新.
    /// 具体见 <see cref="AddressablesPlayerBuildProcessor.PrepareForPlayerbuild"/> 函数中添加文件到 StreamingAssets 目录的逻辑
    /// </summary>
    public class MyAddressablesPlayerBuildProcessor : BuildPlayerProcessor
    {
        /// <summary>
        /// Returns the player build processor callback order.
        /// </summary>
        public override int callbackOrder { get; } = new AddressablesPlayerBuildProcessor().callbackOrder + 1;

        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            #if MY_USE_DYNAMIC_AA // 从远端获取 settings.json 等入口文件, 并初始化 Addressable, 以达到可以不更新小程序版本即可更新资源的目标, 需要从本地的 StreamingAssets 删除掉不需要的 settings.json 等文件, 减少首包大小
            if (buildPlayerContext != null)
            {
                var contextType = typeof(BuildPlayerContext);
                var streamingAssetFilesField = contextType.GetProperty("StreamingAssetFiles", BindingFlags.Instance | BindingFlags.NonPublic)!;
                var streamingAssetFiles = streamingAssetFilesField.GetValue(buildPlayerContext);
                
                Debug.LogWarningFormat("[MyAddressablesPlayerBuildProcessor] 我们清理了所有添加到 StreamingAsset 中的文件, 以减少首包大小\n{0}", JsonConvert.SerializeObject(streamingAssetFiles, Formatting.Indented));
                
                // Clear the dictionary using its public Clear() method
                var clearMethod = streamingAssetFiles.GetType().GetMethod("Clear")!;
                clearMethod.Invoke(streamingAssetFiles, null);
            }
            #endif
        }
    }
}

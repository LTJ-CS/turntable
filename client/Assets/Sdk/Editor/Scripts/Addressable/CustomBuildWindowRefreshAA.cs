using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

namespace Sdk.Editor.Scripts.Addressable
{
    /// <summary>
    /// 在本文件中实现一些与资源自动收集相关的逻辑函数的实现
    /// </summary>
    public partial class CustomBuildWindow
    {
        [MenuItem("Tools/同步 AA 资源", false, -200)]
        static void SyncAddressableEntries()
        {
            // 收集AA 资源, 版本号无所谓, 并不真正打包
            RefreshAddressableGroupEntries("0000");
        }
        
        /// <summary>
        /// 由 YooAsset 的资源自动收集功能，刷新 Addressable Group Entries
        /// </summary>
        private static UniTask RefreshAddressableGroupEntries(string resVersion)
        {
            // 只设置几个我们一定会使用的参数, 其它参数暂时没用
            var buildParameters = new ScriptableBuildParameters
                                  {
                                      BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot(),
                                      BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot(),
                                      BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString(),
                                      BuildTarget = EditorUserBuildSettings.activeBuildTarget,
                                      BuildMode = EBuildMode.ForceRebuild,
                                      PackageName = "DefaultPackage",
                                      PackageVersion = resVersion
                                  };

            ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);
            if (!buildResult.Success)
            {
                throw new Exception($"资源自动收集失败：{JsonUtility.ToJson(buildResult)}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            return UniTask.CompletedTask;
        }
    }
}
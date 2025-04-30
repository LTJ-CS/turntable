using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;

namespace Sdk.Editor.Scripts.Addressable
{
    /// <summary>
    /// Build scripts used for player builds and running with bundles in the editor.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildScriptSeparatelyPacked.asset", menuName = "Addressables/Content Builders/Separately Packed Build Script")]
    public class BuildScriptSeparatelyPackedMode : BuildScriptPackedMode
    {
        public override string Name => "Separately Packed Build Script";

        /// <summary>
        /// 创建一个本类的资源
        /// </summary>
        // [MenuItem("Tools/CreateScriptPackedModeAsset")]
        public static void CreateInstance()
        {
            var script = CreateInstance<BuildScriptSeparatelyPackedMode>();
            var path = AddressableAssetSettingsDefaultObject.Settings.DataBuilderFolder
                       + "/" + nameof(BuildScriptSeparatelyPackedMode) + ".asset";
            if (!File.Exists(path))
                AssetDatabase.CreateAsset(script, path);
        }

        protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
        {
            // 暂时还没有实现, 后续看需求再定
            throw new NotImplementedException();
        }
    }
}
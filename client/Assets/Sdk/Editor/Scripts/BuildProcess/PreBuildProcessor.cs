using System;
using System.IO;
using Sdk.Editor.Scripts.Addressable;
using Sdk.Runtime.Base;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Sdk.Editor.Scripts.BuildProcess
{
    public class PreBuildProcessor : BuildPlayerProcessor,IPostprocessBuildWithReport
    {
        // 定义接口的回调顺序，数值越小优先级越高
        public override int callbackOrder { get; } = new AddressablesPlayerBuildProcessor().callbackOrder - 1;
      

        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            Debug.Log("构建开始前的回调执行");
            
            BuildScript.buildCompleted += OnBuildCompleted;
            if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
                EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;
            
            bool isByte = PlatformSwitcher.IsByteGame();
            Debug.Log($"是否是抖音环境：{isByte}");
            if (isByte)
            {
                var curTarget = EditorUserBuildSettings.activeBuildTarget;
                if (curTarget == BuildTarget.Android)
                {
                    PlatformSwitcher.AddDefineSymbols(PlatformSwitcher.DouYinDefine,BuildTargetGroup.Android);
                }else if (curTarget == BuildTarget.WebGL)
                {
                    PlatformSwitcher.AddDefineSymbols(PlatformSwitcher.DouYinDefine,BuildTargetGroup.WebGL);
                }
            }
            bool isWeChat = PlatformSwitcher.IsWeChat();
            Debug.Log($"是否是微信环境：{isWeChat}");
            if (isWeChat)
            {
                PlatformSwitcher.AddDefineSymbols(PlatformSwitcher.WeiXinDefine,BuildTargetGroup.WebGL);
            }
            Debug.Log($"修改AddressableAssetSetting Version:{GameVersion.GameVer}");
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            settings.BuildRemoteCatalog = true;
            settings.OverridePlayerVersion = GameVersion.GameVer;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            if (!Enum.IsDefined(typeof(LaunchSettings.EnvironmentType), GameVersion.GameEnv))
            {
                throw new BuildFailedException($"gameEnvInt:{GameVersion.GameEnv},不在LaunchSettings.EnvironmentType枚举的定义里");
            }

            LaunchSettings.EnvironmentType environmentType = (LaunchSettings.EnvironmentType)GameVersion.GameEnv;
            Debug.Log($"GameVer:{GameVersion.GameVer}\n GameEnvInt:{GameVersion.GameEnv}\n GameEnvStr:{environmentType}" +
                      $"\nLaunchSetting:{LaunchSettings.GetEnvironmentSettings(environmentType).ToString()}" +
                      $"\n cdnSuffix{LaunchSettings.GetCdnSuffix()}"+
                      $"\nAddressableVersion:{settings.OverridePlayerVersion}");
            //throw new BuildFailedException("应用标识符不符合要求！");
        }

        /// <summary>
        /// Addressable Asset 构建后的处理, 比如复制 settings.json 等文件到 ServerData 目录
        /// </summary>
        private static void OnBuildCompleted(AddressableAssetBuildResult result)
        {
            // 复制 settings.json, catalog.json 等文件到 ServerData/[BuildTarget] 目录, 以实现 settings.json, catalog.json 文件的热更新 
            var srcPath = result.OutputPath;

            // var settings = AddressableAssetSettingsDefaultObject.Settings;
            var remoteBuildPath = CustomBuildWindow.RemoteBuildPath;
            var targetPath = Path.Combine(remoteBuildPath, $"settings_{GameVersion.GameVer}.json");
            Debug.Log("### OnBuildCompleted settings targetPath " + targetPath);
            File.Copy(srcPath, targetPath,true);
        }
        public void OnPostprocessBuild(BuildReport report)
        {
            BuildScript.buildCompleted -= OnBuildCompleted;
        }
    }
}
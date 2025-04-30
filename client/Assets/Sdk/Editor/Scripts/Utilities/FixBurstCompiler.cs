using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Sdk.Editor.Scripts.Utils
{
    /// <summary>
    /// 修正 Burst 编译时找不到 NDK 的错误.
    /// Error: Burst requires the android NDK to be correctly installed (it can be installed via the unity installer add component) in order to build a standalone player for Android with ARMV7A_NEON32
    /// </summary>
    public class FixBurstCompiler : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get => 0;
        }
        public void OnPreprocessBuild(BuildReport report)
        {
        #if UNITY_2021_3_14
            var ndkRoot = EditorPrefs.GetString("AndroidNdkRootR16b");
            Debug.Log("Android NDK Root: " + ndkRoot);
            Environment.SetEnvironmentVariable("ANDROID_NDK_ROOT", ndkRoot);
            Debug.Log("Android NDK Root: " + Environment.GetEnvironmentVariable("ANDROID_NDK_ROOT"));
        #else
            Debug.Log("不支持的编辑器版本, 不会自动修正 Burst 的编译错误. Android NDK Root: " + Environment.GetEnvironmentVariable("ANDROID_NDK_ROOT"));
        #endif
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class PlatformSwitcher
{
    private const string EditorMenuPath   = "Tools/平台切换/编辑器";
    private const string WeChatMenuPath   = "Tools/平台切换/微信";
    private const string ByteGameMenuPath = "Tools/平台切换/抖音";

    private static List<string> _platformMenuPathList = new List<string>()
                                                        {
                                                            EditorMenuPath,
                                                            WeChatMenuPath,
                                                            ByteGameMenuPath,
                                                        };

    private const string WeiXinDllPath         = "WX-WASM-SDK-V2/Runtime/Plugins/wx-runtime.dll";
    public const  string WeiXinDefine          = "PLATFORM_WEIXIN";
    private const string WeiXinDirectory       = "WX-WASM-SDK-V2";
    private const string WeiXinSourceDirectory = "PlatformSDK/WX-WASM-SDK-V2";
    
    
    private const string DouYinDllPath         = "Plugins/ByteGame/com.bytedance.starksdk/starksdk.dll";
    public const string DouYinDefine          = "PLATFORM_DOUYIN";
    private const string DouYinDirectory       = "Plugins/ByteGame";
    private const string DouYinSourceDirectory = "PlatformSDK/ByteGame";
    
    private const string TeAndroidDefine       = "TE_DISABLE_ANDROID_JAVA";
    

    static PlatformSwitcher()
    {
        EditorApplication.delayCall += () =>
        {
            int index = 0;
            if (IsWeChat())
            {
                index = 1;
            }
            else if (IsByteGame())
            {
                index = 2;
            }

            SwitchPlatformChecked(index);
        };
    }

    /// <summary>
    /// 切换菜单的平台选择
    /// </summary>
    /// <param name="index"></param>
    private static void SwitchPlatformChecked(int index)
    {
        for (int i = 0; i < _platformMenuPathList.Count; i++)
        {
            Menu.SetChecked(_platformMenuPathList[i], index == i);
        }
    }

    [MenuItem(EditorMenuPath)]
    private static void SwitchToEditor()
    {
        SwitchPlatformChecked(0);
        SwitchEditor();
    }

    [MenuItem(WeChatMenuPath)]
    private static void SwitchToWeChat()
    {
        SwitchPlatformChecked(1);
        SwitchWeChat();
    }

    [MenuItem(ByteGameMenuPath)]
    private static void SwitchToByteGame()
    {
        SwitchPlatformChecked(2);
        SwitchByteGame();
    }

    public static bool IsWeChat()
    {
        var dllPath = Path.Combine(Application.dataPath, WeiXinDllPath);
        return File.Exists(dllPath);
    }

    public static bool IsByteGame()
    {
        var dllPath = Path.Combine(Application.dataPath, DouYinDllPath);
        return File.Exists(dllPath);
    }

    /// <summary>
    /// 检查对应平台并切换
    /// </summary>
    /// <param name="buildTargetGroup"></param>
    /// <param name="buildTarget"></param>
    /// <returns></returns>
    static bool SwitchActiveBuildTarget(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget)
    {
        bool isSupported = BuildPipeline.IsBuildTargetSupported(buildTargetGroup, buildTarget);
        if (!isSupported)
        {
            Debug.LogError($"### 没有安装 {buildTarget} 平台");
            return false;
        }
        EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
        return true;
    }
    /// <summary>
    /// 切换到编辑器
    /// </summary>
    /// <returns></returns>
    public static bool SwitchEditor()
    {
        Debug.Log("### 0 Start 开始切换WebGL平台");
        bool isSwitchSuccess = SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        if (!isSwitchSuccess)
        {
            Debug.LogError("### 0 End 没有安装WebGL模块");
            return false;
        }
        RemoveDefineSymbols(WeiXinDefine);
        RemoveDefineSymbols(DouYinDefine);

        DeleteAsset(WeiXinDirectory);
        DeleteAsset(DouYinDirectory);
        Debug.Log("### 0 End 切换WebGL平台结束");
        return true;
    }

    /// <summary>
    /// 切换到微信
    /// </summary>
    /// <returns></returns>
    public static bool SwitchWeChat()
    {
        Debug.Log("### 0 Start 开始切换微信平台");
        bool isSwitchSuccess = SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        if (!isSwitchSuccess)
        {
            Debug.LogError("### 0 End 没有安装WebGL模块");
            return false;
        }
        // EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

        RemoveDefineSymbols(DouYinDefine);
        AddDefineSymbols(WeiXinDefine);

        DeleteAsset(IsWeChat() ? WeiXinDirectory : DouYinDirectory);
        CreateAsset(WeiXinSourceDirectory, WeiXinDirectory);

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("### 0 End 微信平台切换结束");
        return true;
    }

    /// <summary>
    /// 切换到抖音
    /// </summary>
    /// <returns></returns>
    public static bool SwitchByteGame()
    {
        Debug.Log("### 0 开始切换抖音平台");
        bool isSwitchSuccess = SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        if (!isSwitchSuccess)
        {
            Debug.LogError("### 0 End 没有安装WebGL模块");
            return false;
        }
        // EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

        RemoveDefineSymbols(WeiXinDefine);
        AddDefineSymbols(DouYinDefine);
        AddDefineSymbols(TeAndroidDefine);

        if (IsByteGame())
        {
            Debug.Log("### 0 End 抖音SDK已存在");
            return true;
        }

        DeleteAsset(WeiXinDirectory);
        CreateAsset(DouYinSourceDirectory, DouYinDirectory);

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("### 0 End 抖音平台切换结束");
        return true;
    }
    /// <summary>
    /// 添加宏
    /// </summary>
    /// <param name="defineSymbols"></param>
    private static void AddDefineSymbols(string defineSymbols)
    {
        AddDefineSymbols(defineSymbols, BuildTargetGroup.WebGL);
        AddDefineSymbols(defineSymbols, BuildTargetGroup.Android);
    }

    /// <summary>
    /// 为对应平台添加宏
    /// </summary>
    /// <param name="defineSymbols"></param>
    /// <param name="targetGroup"></param>
    public static void AddDefineSymbols(string defineSymbols, BuildTargetGroup targetGroup)
    {
        string content = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        List<string> symbols = new List<string>();
        if (!string.IsNullOrEmpty(content))
        {
            symbols = content.Split(";").ToList();
        }

        if (symbols.IndexOf(defineSymbols) != -1)
        {
            return;
        }

        symbols.Add(defineSymbols);
        content = string.Join(";", symbols.ToArray());
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, content);
    }

    private static void RemoveDefineSymbols(string defineSymbols)
    {
        RemoveDefineSymbols(defineSymbols, BuildTargetGroup.WebGL);
        RemoveDefineSymbols(defineSymbols, BuildTargetGroup.Android);
    }

    /// <summary>
    /// 为对应平台移除宏
    /// </summary>
    /// <param name="defineSymbols"></param>
    /// <param name="targetGroup"></param>
    private static void RemoveDefineSymbols(string defineSymbols, BuildTargetGroup targetGroup)
    {
        string content = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        List<string> symbols = new List<string>();
        if (!string.IsNullOrEmpty(content))
        {
            symbols = content.Split(";").ToList();
        }

        if (symbols.IndexOf(defineSymbols) == -1)
        {
            return;
        }

        symbols.Remove(defineSymbols);
        content = string.Join(";", symbols.ToArray());
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, content);
    }

    /// <summary>
    /// 删除文件或目录
    /// </summary>
    /// <param name="path"></param>
    private static void DeleteFileOrDirectory(string path)
    {
        FileUtil.DeleteFileOrDirectory(path);
        FileUtil.DeleteFileOrDirectory(path + ".meta");
    }
    
    /// <summary>
    /// 拷贝资源
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    private static void CreateAsset(string src, string dst)
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            src = Path.Combine(Environment.CurrentDirectory, src);
            dst = Path.Combine(Application.dataPath, dst);
            FileUtil.CopyFileOrDirectory(src, dst);
        }
        finally
        {
            // 确保在逻辑结束后恢复资源的自动刷新
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
       
    }
    /// <summary>
    /// 删除资源
    /// </summary>
    /// <param name="assetPath"></param>
    private static void DeleteAsset(string assetPath)
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            assetPath = Path.Combine(Application.dataPath, assetPath);
            DeleteFileOrDirectory(assetPath);
        }
        finally
        {
            // 确保在逻辑结束后恢复资源的自动刷新
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
      
    }
    
    
}
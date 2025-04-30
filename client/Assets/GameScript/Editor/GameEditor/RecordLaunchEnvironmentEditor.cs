using System;
using System.Collections.Generic;
using Sdk.Runtime.Base;
using UnityEditor;
using UnityEngine;


[InitializeOnLoad]
public class EnvSwitcher
{
   
   
    private const string Development = "Tools/切换服务器环境/局域网";
    private const string Testing     = "Tools/切换服务器环境/内网";
    private const string Beat        = "Tools/切换服务器环境/测试服 测试appid";
    private const string Production  = "Tools/切换服务器环境/正式服 正式appid";
    private const string Panda       = "Tools/切换服务器环境/熊猫";
    
    private static List<string> _evnList = new List<string>()
                                           {
                                               Development,
                                               Testing,
                                               Beat,
                                               Production,
                                               Panda,
                                           };

    static EnvSwitcher()
    {
        EditorApplication.delayCall += () =>
        {
            int key = EditorPrefs.GetInt(LaunchSettings.EnvKey, 0);
            SwitchEnvChecked(key);
        };
    }

    private static void SwitchEnvChecked(int index)
    {
        for (int i = 0; i < _evnList.Count; i++)
        {
            Menu.SetChecked(_evnList[i], index == i);
        }
      
       
    }
    
    [MenuItem(Development)]
    private static void SwitchToDevelopment()
    {
        EditorPrefs.SetInt(LaunchSettings.EnvKey, (int)LaunchSettings.EnvironmentType.Development);
        SwitchEnvChecked(0);
    }

    [MenuItem(Testing)]
    private static void SwitchToTesting()
    {
        EditorPrefs.SetInt(LaunchSettings.EnvKey, (int)LaunchSettings.EnvironmentType.Testing);
        SwitchEnvChecked(1);
    }

    [MenuItem(Beat)]
    private static void SwitchToBeat()
    {
        EditorPrefs.SetInt(LaunchSettings.EnvKey, (int)LaunchSettings.EnvironmentType.Beta);
        SwitchEnvChecked(2);
    }

    [MenuItem(Production)]
    private static void SwitchToProduction()
    {
        EditorPrefs.SetInt(LaunchSettings.EnvKey, (int)LaunchSettings.EnvironmentType.Production);
        SwitchEnvChecked(3);
    }
    
    [MenuItem(Panda)]
    private static void SwitchToPanda()
    {
        EditorPrefs.SetInt(LaunchSettings.EnvKey, (int)LaunchSettings.EnvironmentType.Panda);
        SwitchEnvChecked(4);
    }
}


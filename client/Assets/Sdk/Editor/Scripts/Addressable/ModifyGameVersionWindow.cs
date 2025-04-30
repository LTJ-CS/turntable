using System;
using System.Linq;
using System.Text.RegularExpressions;
using LitJson;
using Sdk.Runtime.Base;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using YooAsset.Editor;

namespace Sdk.Editor.Scripts.Addressable
{
    public class ModifyGameVersionWindow : EditorWindow
    {
        /// <summary>
        /// 停靠窗口类型集合, 与 YooAsset 的窗口合并在一起显示, 以后再重构这块窗口显示逻辑吧.
        /// </summary>
        public static readonly Type[] DockedWindowTypes = WindowsDefine.DockedWindowTypes.Select(type => type).Append(typeof(ModifyGameVersionWindow)).ToArray();
       // [MenuItem("Tools/修改脚本中游戏版本号", false, -202)]
        private static void OpenWindow()
        {
            GetWindow<ModifyGameVersionWindow>("ModifyGameVersion", true, DockedWindowTypes);
            
        }
       private string _gameVersion;
       string         pattern =@"^\d+$"; // 正则表达式模式
       
       private const string ModifyKey = "ModifyGameVersionKey";
        private void OnGUI()
        {
            GUIStyle textMiddleCenter = new GUIStyle(GUI.skin.label);
            textMiddleCenter.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("--- 版本信息 格式是 “数字” ---", textMiddleCenter);
            CreateVersion();
            EditorGUILayout.Space();
            if (GUILayout.Button("修改游戏版本号"))
            {
                if (string.IsNullOrEmpty(_gameVersion))
                {
                    EditorUtility.DisplayDialog("提示", "版本号不能为空", "ok");
                    return;
                }

                if (Regex.IsMatch(_gameVersion, pattern))
                {
                    GenerateGameVersionTool.GenerateGameVersionCodeFile(_gameVersion,GameVersion.GameEnv);
                    
                    EditorPrefs.SetBool(ModifyKey,true);
                    CompilationPipeline.RequestScriptCompilation();
                }
                else
                {
                    EditorUtility.DisplayDialog("提示","输入不符合格式 :数字","ok");
                }
            }
        }
        
        private  void CreateVersion()
        {
            _gameVersion = EditorGUILayout.TextField("游戏版本号", _gameVersion);
        }
        
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnDidReloadScripts()
        {
            if (!EditorPrefs.GetBool(ModifyKey))
            {
                return;
            }
            EditorPrefs.SetBool(ModifyKey,false);
            EditorApplication.update += CompilationFinished;
        }

        private static void CompilationFinished()
        {
            EditorApplication.update -= CompilationFinished;
            GenerateGameVersionTool.OutputCodeContent();
            
        }
    }
}

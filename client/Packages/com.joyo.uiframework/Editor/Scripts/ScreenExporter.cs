using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DG.DemiEditor;
using JetBrains.Annotations;
using UIFramework;
using UIFramework.Base;
using UIFramework.Components;
using UIFramework.UIScreen;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Editor.Scripts
{
    /// <summary>
    /// UI Screen 的导出器, 静态函数方便以后命令行调用
    /// </summary>
    internal static class ScreenExporter
    {
        /// <summary>
        /// 存储检测到的错误信息
        /// </summary>
        private static readonly List<ErrorInfo> errorInfos = new();

        internal static List<ErrorInfo> ErrorInfos => errorInfos;
        private static  int             _uiLayer = 0;

        /// <summary>
        /// 开始导出时调用
        /// </summary>
        public static void BeganExport()
        {
            errorInfos.Clear();
            _uiLayer = LayerMask.NameToLayer("UI");
        }

        /// <summary>
        /// 检查是否符合导出的要求
        /// </summary>
        /// <param name="screenRootGameObject">指定要导出的 Screen 的根 GameObject</param>
        /// <param name="screenName">指定要导出的 Screen 的名称</param>
        internal static bool CheckValid(GameObject screenRootGameObject, string screenName)
        {
            var screenSourceName = UIFrameworkSettings.GetScreenViewSourceName(screenName);
            if (screenSourceName != screenRootGameObject.name)
            {
                AddErrorInfo(
                    $"{screenRootGameObject.name} 的 name 不是 {screenName}, 请检查 ScreenView 的 name, 名称必须符合规范: xxxScreenSource",
                    screenRootGameObject);
            }

            var screenViewInfoInEditor = screenRootGameObject.GetComponent<ScreenViewInfoInEditor>();
            if (screenViewInfoInEditor.m_ReferenceImage == null)
            {
                AddErrorInfo("ScreenViewInfoInEditor 指向的参考图 GameObject 必须有效, 是否被误删除了?", screenRootGameObject);
            }

            // TODO: 实现各种合法性的检查, 并提示用户, 定位有问题的 GameObject 等等
            if (screenRootGameObject.GetComponent<CanvasGroup>() == null)
            {
                AddErrorInfo("ScreenView 必须包含 CanvasGroup 组件", screenRootGameObject);
            }

            // 获取所有的 GameObject, 包含子节点
            var trsList = screenRootGameObject.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < trsList.Length; i++)
            {
                var trs = trsList[i];
                if (EditorUtility.DisplayCancelableProgressBar("导出检查", $"正在检查: {trs.name} ...", (float)i / trsList.Length))
                {
                    return false;
                }

                // 检查这个 GameObject 的组件是否合法
                CheckGameObject(trs);
            }

            return true;
        }

        /// <summary>
        /// 忽略检查的组件类型
        /// </summary>
        private static readonly HashSet<Type> IgnoredComponents = new()
                                                                  {
                                                                      typeof(Image),
                                                                      typeof(RawImage),
                                                                      typeof(Text),
                                                                  };

        /// <summary>
        /// 递归检查所有的组件是否符合导出规则
        /// </summary>
        /// <param name="trs">指定要检测的 GameObject</param>
        private static void CheckGameObject(Transform trs)
        {
            if (trs is not RectTransform)
            {
                AddErrorInfo($"{trs.name} 没有 RectTransform 组件, 所有的 UI 组件都必须是 RectTransform", trs);
            }

            if (trs.gameObject.layer != _uiLayer)
            {
                AddErrorInfo($"{trs.name} 的 layer 不是 UI, UI 层 GameObject 的 layer 必须为 UI", trs);
            }

            // 检测是否包含有 TMP 的文本组件
            // if (go.GetComponent<TextMeshProUGUI>() != null)
            // {
            //     AddErrorInfo($"{go.name} 包含了 TMP 的文本组件, 请使用 UIFramework 的文本组件", go);
            // }

            var components = trs.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null)
                {
                    AddErrorInfo($"{trs.name} 有丢失的组件", trs);
                    continue;
                }

                CheckForUnassignedSerializedFields(component);
            }

            return;

            // ====================== 辅助函数 ======================

            // 检查是否有未赋值的字段
            void CheckForUnassignedSerializedFields(Component checkComponent)
            {
                // if (IgnoredComponents.Contains(checkComponent.GetType()))
                //    return;
                var assemblyName = checkComponent.GetType().Assembly.FullName;
                if (assemblyName.StartsWith("UnityEngine.") ||
                    assemblyName.StartsWith("Unity.") || assemblyName.StartsWith("PlayMaker"))
                    return; // 跳过 UnityEngine 的组件, 它们里面经常有可以不用赋值的字段

                SerializedObject so = null;
                try
                {
                    var fields = checkComponent.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    foreach (var field in fields)
                    {
                        // Check if the field has the SerializedField attribute
                        if (Attribute.IsDefined(field, typeof(SerializeField)))
                        {
                            // Check if the field is unassigned
                            var fieldValue = field.GetValue(checkComponent);
                            if (fieldValue == null || fieldValue.Equals(null))
                            {
                                // 如果这个属性标记为可空, 则不处理判空逻辑
                                if (Attribute.IsDefined(field, typeof(CanBeNullAttribute)) || checkComponent.GetType().Name == "SkeletonSubmeshGraphic")
                                {
                                    continue;
                                }

                                if (so == null)
                                    so = new SerializedObject(checkComponent);

                                if (checkComponent.GetType().Name == "SkeletonGraphic" && field.Name == "physicsMovementRelativeTo" || checkComponent.GetType().Name == "UiParticles")
                                {
                                    continue;
                                }
                                
                                var displayName = so.FindProperty(field.Name).displayName;
                                AddErrorInfo($"组件 '{checkComponent.name}.{checkComponent.GetType().Name}' 有未赋值的字段: '{displayName}'", trs);
                            }
                        }
                    }
                }
                finally
                {
                    so?.Dispose();
                }
            }
        }

        /// <summary>
        /// 导出给定的 UI Screen
        /// </summary>
        /// <param name="screenRootGameObject">指定要导出的 UI Screen 的根 GameObject</param>
        /// <param name="screenName">指定要导出的 Screen 的名称</param>
        public static void Export(GameObject screenRootGameObject, string screenName)
        {
            // 生成 Presenter 代码
            GenerateScreenPresenterCodeFile(screenRootGameObject, screenName);

            // 复制 Screen, 我们需要从 Prefab 来复制, 否则内嵌的 Prefab 会被 Unpack
            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(stage.assetPath);
            var exportedScreenGameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            // 把最外层的 GameObject 与 源 Prefab Unpack, 以便下面我们修改这个 UI 相关的属性
            PrefabUtility.UnpackPrefabInstance(exportedScreenGameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            exportedScreenGameObject.name = screenName;
            var prefabPath = UIFrameworkSettings.GetScreenViewPrefabPath(screenName);

            var screenViewInfoInEditor = exportedScreenGameObject.GetComponent<ScreenViewInfoInEditor>();
            var screenBehaviour = screenViewInfoInEditor.Behaviour;
            {  // 添加或设置一些运行时需要的组件
                
                // 从 screenViewInfoInEditor.BackgroundClickEventCallback 复制背景点击回调函数到 ScreenBehaviour 中, 这是由于 ScreenBehaviour.BackgroundClickEventCallback 无法在 UIToolkit 中直接编辑的原因
                if (screenBehaviour.BackgroundClickEventType == EBackgroundClickEventType.CustomCallback)
                {
                    screenBehaviour.BackgroundClickEventCallback = screenViewInfoInEditor.BackgroundClickEventCallback;
                }
                else
                {
                    screenBehaviour.BackgroundClickEventCallback?.RemoveAllListeners();
                }
                
                
                // 设置 m_ScreenBehaviour 属性
                var viewBase = exportedScreenGameObject.GetComponent<UIScreenViewBase>();
                // 通过反射设置 ScreenBehaviour
                var fieldInfo = typeof(UIScreenViewBase).GetField(UIScreenViewBase.ScreenBehaviourName, BindingFlags.NonPublic | BindingFlags.Instance)!;
                fieldInfo.SetValue(viewBase, screenBehaviour);
                EditorUtility.SetDirty(viewBase);
            }
            
            {
                // 清理一些不需要的组件或 GameObject
                // 删除参考图
                Object.DestroyImmediate(screenViewInfoInEditor.m_ReferenceImage.gameObject);

                // 删除之前误加载的 Canvas Group 组件
                RemoveComponent<CanvasGroup>(exportedScreenGameObject);
                
                // 删除背景组件
                if (screenViewInfoInEditor.m_BackgroundImage)
                {
                    Object.DestroyImmediate(screenViewInfoInEditor.m_BackgroundImage);
                }
                
                // 删除 Canvas Renderer
                RemoveComponent<CanvasRenderer>(exportedScreenGameObject);

                // 删除不需要的组件
                var editorComponents = exportedScreenGameObject.GetComponentsInChildren(typeof(IEditorOnlyComponent), true);
                foreach (var editorComponent in editorComponents)
                {
                    Object.DestroyImmediate(editorComponent);
                }
            }
            
            // 存储运行时的 Screen
            var newPrefab = PrefabUtility.SaveAsPrefabAsset(exportedScreenGameObject, prefabPath);
            Object.DestroyImmediate(exportedScreenGameObject);

            // 更新编辑信息中导出的运行时 Prefab 的引用, 方便查看
            var orgViewInfoInEditor = screenRootGameObject.GetComponent<ScreenViewInfoInEditor>();
            orgViewInfoInEditor.m_RunTimePrefab = newPrefab;
            EditorUtility.SetDirty(orgViewInfoInEditor);

            return;

            void RemoveComponent<T>(GameObject go)
                where T : Component
            {
                var component = go.GetComponent<T>();
                if (component != null)
                {
                    Object.DestroyImmediate(component);
                }
            }
        }

        /// <summary>
        /// 为 Screen 生成对应的 Presenter 类, 用于把一些选项配置到 Presenter 中, 方便打开 UI 时访问, 如 Layer 信息
        /// </summary>
        /// <param name="screenRootGameObject">指定要导出的 UI Screen 的根 GameObject</param>
        /// <param name="screenName">指定要导出的 Screen 的名称</param>
        /// <returns></returns>
        private static bool GenerateScreenPresenterCodeFile(GameObject screenRootGameObject, string screenName)
        {
            var screenViewInfoInEditor = screenRootGameObject.GetComponent<ScreenViewInfoInEditor>();
            var presenterCodeFilePath = UIFrameworkSettings.GetScreenPresenterCodeFilePath(screenName);
            if (false /* && File.Exists(viewCodeFilePath)*/)
            {
                // if (!EditorUtility.DisplayDialog("UIFramework", $"{screenName} 的 View 类已经存在, 是否覆盖生成? ", "确定", "取消"))
                // {
                //     return false;
                // }
            }

            var sourcePrefabName = UIFrameworkSettings.GetScreenViewSourceName(screenName);
            var screenPresenterClassName = UIFrameworkSettings.GetScreenPresenterClassName(screenName);
            var screenViewClassName = UIFrameworkSettings.GetScreenViewClassName(screenName);
            var uiLayer = screenViewInfoInEditor.Layer;
            if (uiLayer < EUILayer.InternalUse)
            {
                AddErrorInfo($"UI Layer 的配置不正确, 不应该小于 {nameof(EUILayer.InternalUse)}", screenRootGameObject);
                return false;
            }

            var backgroundColor = screenViewInfoInEditor.CustomBackgroundColor;
            var backgroundColorCode = screenViewInfoInEditor.BackgroundShowShowType == EBackgroundShowType.CustomColor
                ? $"public override Color BackgroundColor => new Color({backgroundColor.r}, {backgroundColor.g}, {backgroundColor.b}, {backgroundColor.a});"
                : "";

            // 定义 Screen 的 Presenter 类的自动生成部分的代码
            var screenPresenterPartialCode = $@"
// <auto-generated>
//     Generated by the ui framework.  DO NOT EDIT!
//     Source: {sourcePrefabName}.prefab
//     Remark: 可以另写一个文件来扩展此类来实现具体的与 UI 相关的逻辑, 如 OnOpen(), OnClose() 等
// </auto-generated>

using UIFramework.Base;
using UIFramework.UIScreen;
using UnityEngine;
// ReSharper disable once CheckNamespace
namespace {UIFrameworkSettings.UINamespace}
{{
    /// <summary>
    /// {screenName} 的 Presenter 类   
    /// </summary>
    public sealed partial class {screenPresenterClassName} : GenericScreenPresenterBase<{screenViewClassName}>
    {{
        public override EUILayer Layer => {nameof(EUILayer)}.{uiLayer.ToString()};
        public override string ScreenName => ""{screenName}"";
        public override EBackgroundShowType BackgroundShowType => EBackgroundShowType.{screenViewInfoInEditor.BackgroundShowShowType};
        {backgroundColorCode}
    }}
}}
";

            // 写入 View 的代码
            File.WriteAllText(presenterCodeFilePath, screenPresenterPartialCode);

            return true;
        }


        /// <summary>
        /// 添加一个错误描述
        /// </summary>
        /// <param name="error">错误的描述</param>
        /// <param name="trs">发生错误的 Transform</param>
        private static void AddErrorInfo(string error, Transform trs)
        {
            AddErrorInfo(error, trs.gameObject);
        }

        /// <summary>
        /// 添加一个错误描述
        /// </summary>
        /// <param name="error">错误的描述</param>
        /// <param name="gameObject">发生错误的 GameObject</param>
        private static void AddErrorInfo(string error, GameObject gameObject)
        {
            errorInfos.Add(new ErrorInfo
                           {
                               Error      = error,
                               GameObject = gameObject
                           });
        }
    }
}
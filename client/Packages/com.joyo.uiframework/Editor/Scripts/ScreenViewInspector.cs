using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Editor.Scripts.Helper;
using UIFramework;
using UIFramework.Base;
using UIFramework.Components;
using UIFramework.UIScreen;
using UIFramework.Utils;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Image = UnityEngine.UI.Image;

namespace Editor.Scripts
{
    /// <summary>
    /// 为 UI Screen 的 View 提供 inspector
    /// </summary>
    [CustomEditor(typeof(ScreenViewInfoInEditor))]
    public class ScreenViewInspector : UnityEditor.Editor
    {
        /// <summary>
        /// Screen 的根节点
        /// </summary>
        private GameObject _screenRootGameObject;

        /// <summary>
        /// Screen View 的引用
        /// </summary>
        private UIScreenViewBase _screenView;

        /// <summary>
        /// 导出按钮
        /// </summary>
        private Button _exportButton;

        /// <summary>
        /// 保存当前编辑的 ScreenViewInfoInEditor
        /// </summary>
        private ScreenViewInfoInEditor _screenViewInfoInEditor;

        /// <summary>
        /// 用于标识Unity中被禁用的元素的类名。
        /// </summary>
        private const string UnityDisableClassName = "unity-disabled";

        public override VisualElement CreateInspectorGUI()
        {
            var visualTreeAsset = Resources.Load<VisualTreeAsset>("UXML/UIScreenInspector");
            var root = visualTreeAsset.Instantiate();
            _exportButton = root.Q<Button>("Export");

            _screenViewInfoInEditor = (target as ScreenViewInfoInEditor)!;
            _screenRootGameObject   = _screenViewInfoInEditor.gameObject;
            _screenView             = _screenViewInfoInEditor.GetComponent<UIScreenViewBase>();
            var uiBehaviour = _screenViewInfoInEditor.Behaviour;

            {
                // 更新 导出 按钮的状态
                EditorApplicationOnplayModeStateChanged(default);
            }

            // 注册导出按钮点击事件
            _exportButton.RegisterCallback<ClickEvent>(_ => Export().ConfigureAwait(false));

            // 确保一些属性只读
            {
                var objectField = root.Q<ObjectField>("RunTimePrefab");
                objectField.AddToClassList(UnityDisableClassName);
                objectField = root.Q<ObjectField>("ReferenceImage");
                objectField.AddToClassList(UnityDisableClassName);
            }

            // Hack 一下 UILayer 可以显示的枚举值
            HackUILayer(root.Q<EnumField>("UILayer"));

            var backgroundShowType = root.Q<EnumField>("BackgroundShowType");
            var customBackgroundColor = root.Q<ColorField>("CustomBackgroundColor");
            var backgroundClickEventType = root.Q<EnumField>("BackgroundClickEventType");
            var backgroundClickCustomCallback = root.Q<PropertyField>("CustomClickEvent");
            var escPressEventType = root.Q<EnumField>("EscPressEventType");
            var openAnimPlayMode = root.Q<EnumField>("OpenAnimPlayMode");
            var closeAnimPlayMode = root.Q<EnumField>("CloseAnimPlayMode");

            // 监听 ScreenType 的变化
            void UpdateBackgroundProperty(ChangeEvent<Enum> _)
            {
                var enabled = _screenViewInfoInEditor.ScreenType == EScreenType.Custom;
                backgroundShowType.SetEnabled(enabled);
                backgroundClickEventType.SetEnabled(enabled);
            }

            // 更新背景显示类型的显示
            UpdateBackgroundProperty(default);
            root.Q<EnumField>("ScreenType").RegisterCallback<ChangeEvent<Enum>>(UpdateBackgroundProperty);

            // 更新背景颜色属性的显示
            UpdateBackgroundColorProperty(default);
            backgroundShowType.RegisterCallback<ChangeEvent<Enum>>(UpdateBackgroundColorProperty);

            // 更新背景点击事件属性的显示
            UpdateBackgroundClickEventType(default);
            backgroundClickEventType.RegisterCallback<ChangeEvent<Enum>>(UpdateBackgroundClickEventType);
            
            // 根据是否有动画接口的组件来决定是否显示动画选项
            var hasAnimationInterface = _screenViewInfoInEditor.GetComponents<IScreenAnimation>().Any();
            openAnimPlayMode.style.display  = hasAnimationInterface ? DisplayStyle.Flex : DisplayStyle.None;
            closeAnimPlayMode.style.display = hasAnimationInterface ? DisplayStyle.Flex : DisplayStyle.None;

            return root;

            // =============== Local Function ===============

            // 监听背景显示类型的变化
            void UpdateBackgroundColorProperty(ChangeEvent<Enum> _)
            {
                // 决定是否需要显示颜色的选择
                customBackgroundColor.style.display =
                    _screenViewInfoInEditor.BackgroundShowShowType == EBackgroundShowType.CustomColor ||
                    _screenViewInfoInEditor.BackgroundShowShowType == EBackgroundShowType.SemiTransparent
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                customBackgroundColor.showAlpha = _screenViewInfoInEditor.BackgroundShowShowType == EBackgroundShowType.CustomColor;

                // 是否需要同步创建编辑时演示的背景 Image
                if (_screenViewInfoInEditor.BackgroundShowShowType != EBackgroundShowType.CustomColor ||
                    _screenViewInfoInEditor.BackgroundShowShowType == EBackgroundShowType.SemiTransparent)
                {
                    if (_screenViewInfoInEditor.m_BackgroundImage == null)
                    {
                        if (_screenViewInfoInEditor.gameObject.GetComponent<RawImage>() || _screenViewInfoInEditor.gameObject.GetComponent<Image>())
                        {
                            EditorUtility.DisplayDialog("提示", "检测到根节点上存在自己添加的 Image 组件，请删除该组件，否则可能影响后续逻辑", "确定");
                            return;
                        }

                        _screenViewInfoInEditor.m_BackgroundImage = _screenViewInfoInEditor.gameObject.AddComponent<RawImage>();
                    }

                    ScreenUtility.SetImageColor(_screenViewInfoInEditor.m_BackgroundImage, _screenViewInfoInEditor.BackgroundShowShowType, _screenViewInfoInEditor.CustomBackgroundColor);
                }
                else
                {
                    // 删除不再需要的 Image
                    if (_screenViewInfoInEditor.m_BackgroundImage != null)
                    {
                        DestroyImmediate(_screenViewInfoInEditor.m_BackgroundImage);
                        _screenViewInfoInEditor.m_BackgroundImage = null;
                    }
                }
            }

            // 监听背景点击事件的变化
            void UpdateBackgroundClickEventType(ChangeEvent<Enum> _)
            {
                backgroundClickCustomCallback.style.display =
                    _screenViewInfoInEditor.Behaviour.BackgroundClickEventType == EBackgroundClickEventType.CustomCallback
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
        }

        private void HackEnumField<TEnum>(EnumField enumField, TEnum[] validEnumValues)
            where TEnum : Enum
        {
            enumField.RegisterCallback<MouseEnterEvent>(OnPointerDownEvent);

            void OnPointerDownEvent(MouseEnterEvent evt)
            {
                var values = new Enum[validEnumValues.Length];
                var flagValues = new int[validEnumValues.Length];
                var tooltips = new string[validEnumValues.Length];
                var names = new string[validEnumValues.Length];
                var displayNames = names;

                for (var i = 0; i < validEnumValues.Length; i++)
                {
                    var validEnumValue = validEnumValues[i];
                    values[i]     = validEnumValue as Enum;
                    names[i]      = validEnumValue.ToString();
                    flagValues[i] = Convert.ToInt32(validEnumValue);
                    tooltips[i]   = EnumTooltipFromEnumField<EUILayer>(validEnumValue.ToString());
                }

                var enumDataFieldInfo = enumField.GetType().GetField("m_EnumData", BindingFlags.NonPublic | BindingFlags.Instance)!;
                var enumData = enumDataFieldInfo.GetValue(enumField);
                var enumDataType = enumData.GetType();
                var fieldInfo = enumDataType.GetField("values", BindingFlags.Public | BindingFlags.Instance)!;
                fieldInfo.SetValue(enumData, values);
                fieldInfo = enumDataType.GetField("flagValues", BindingFlags.Public | BindingFlags.Instance)!;
                fieldInfo.SetValue(enumData, flagValues);
                fieldInfo = enumDataType.GetField("displayNames", BindingFlags.Public | BindingFlags.Instance)!;
                fieldInfo.SetValue(enumData, displayNames);
                fieldInfo = enumDataType.GetField("tooltip", BindingFlags.Public | BindingFlags.Instance)!;
                fieldInfo.SetValue(enumData, tooltips);
                fieldInfo = enumDataType.GetField("names", BindingFlags.Public | BindingFlags.Instance)!;
                fieldInfo.SetValue(enumData, names);

                enumDataFieldInfo.SetValue(enumField, enumData);

                enumField.UnregisterCallback<MouseEnterEvent>(OnPointerDownEvent);
            }
        }

        /// <summary>
        /// 去掉一些 UILayer 不需要对外暴露的 Enum 值
        /// </summary>
        /// <param name="enumFieldLayer">指定要修改的 EnumField</param>
        private void HackUILayer(EnumField enumFieldLayer)
        {
            var validEnumValues = new[]
                                  {
                                      EUILayer.Bottom,
                                      EUILayer.Scene,
                                      EUILayer.ScreenHome,
                                      EUILayer.Screen,
                                      EUILayer.Screen_1,
                                      EUILayer.Screen_2,
                                      EUILayer.Screen_3,
                                      EUILayer.Popup,
                                      EUILayer.Tips,
                                      EUILayer.Top,
                                  };
            HackEnumField(enumFieldLayer, validEnumValues);
        }

        private static string EnumTooltipFromEnumField<TEnum>(string fieldName)
        {
            var field = typeof(TEnum).GetField(fieldName, BindingFlags.Public | BindingFlags.Static)!;
            object[] customAttributes = field.GetCustomAttributes(typeof(TooltipAttribute), false);
            return customAttributes.Length != 0 ? ((TooltipAttribute)customAttributes.First()).tooltip : string.Empty;
        }

        /// <summary>
        /// 导出运行时 Prefab及代码
        /// </summary>
        private Task Export()
        {
            if (!_screenView.GetType().Name.EndsWith(UIFrameworkSettings.ScreenViewSuffix))
            {
                UnityTipsHelper.ShowError($"要导出的 Screen 的名称不符合要求: xxx{UIFrameworkSettings.ScreenViewSuffix}");
                return Task.CompletedTask;
            }

            var screenName = _screenView.GetType().Name.Replace(UIFrameworkSettings.ScreenViewSuffix, string.Empty);

            {
                // 禁用一会导出按钮, 免得再次误点
                _exportButton.SetEnabled(false);

                IEnumerator DelayEnable()
                {
                    yield return new EditorWaitForSeconds(2);
                    _exportButton.SetEnabled(true);
                }

                EditorCoroutineUtility.StartCoroutine(DelayEnable(), this);
            }

            var toExportRootGameObject = _screenRootGameObject;

            try
            {
                EditorUtility.DisplayCancelableProgressBar("正在导出", "请稍等...", 0);

                // 保证是在打开的 PrefabStage 中导出 Screen, 方便在弹出的错误提示窗口中定位发生错误的对象
                if (IsSelectedInProject(_screenRootGameObject))
                {
                    toExportRootGameObject = OpenPrefabStage();
                }

                ScreenExporter.BeganExport();

                // 检查是否有不符合导出规则的情况
                if (!ScreenExporter.CheckValid(toExportRootGameObject, screenName))
                {
                    return Task.CompletedTask;
                }

                {
                    // 如果有错误信息的话, 则显示它们, 统一解决错误
                    UIExportErrorsWindow.Open(ScreenExporter.ErrorInfos);
                    if (ScreenExporter.ErrorInfos.Count > 0)
                        return Task.CompletedTask;
                }

                // 继续导出
                ScreenExporter.Export(toExportRootGameObject, screenName);

                UnityTipsHelper.Show($"导出成功: {screenName}");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 打开 Prefab Stage
        /// </summary>
        /// <returns></returns>
        private GameObject OpenPrefabStage()
        {
            var prefabPath = AssetDatabase.GetAssetPath(_screenRootGameObject);
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.assetPath == prefabPath)
                return stage.prefabContentsRoot;

            // 在 Project 窗口选中的 Prefab, 我们需要打开 PrefabStage, 以便定位错误对象
            stage = PrefabStageUtility.OpenPrefab(prefabPath);
            return stage.prefabContentsRoot;
        }

        /// <summary>
        /// 切换模式时, 更新导出按钮的状态
        /// </summary>
        private void EditorApplicationOnplayModeStateChanged(PlayModeStateChange _)
        {
            if (_exportButton == null)
                return;

            if (!EditorApplication.isPlaying && (
                    IsSelectedInProject(_screenRootGameObject) ||       // 在 Project 窗口选中了 Prefab Asset 
                    IsSelectedInPrefabStageAlone(_screenRootGameObject) // 在 Prefab Stage 窗口选中了 Prefab, 且不是嵌入在其他 Prefab 中
                ))
            {
                _exportButton.SetEnabled(true);
                _exportButton.tooltip = "导出此 Screen 的运行时 Prefab 及所有对象";
            }
            else
            {
                _exportButton.SetEnabled(false);
                _exportButton.tooltip = "只有非运行模式下, 在 Project 或 Prefab Stage Mode 窗口选中此 prefab 时才可以导出";
            }
        }

        public static bool IsSelectedInProject(GameObject go)
        {
            return PrefabUtility.IsPartOfPrefabAsset(go);
        }

        public static bool IsSelectedInPrefabStageAlone(GameObject go)
        {
            return PrefabStageUtility.GetPrefabStage(go) != null && !PrefabUtility.IsPartOfPrefabInstance(go);
        }
    }
}
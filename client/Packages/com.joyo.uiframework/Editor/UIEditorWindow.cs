using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.Scripts;
using Editor.Scripts.Helper;
using UIFramework;
using UIFramework.Utils;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Assembly = System.Reflection.Assembly;
using Button = UnityEngine.UIElements.Button;
using Logger = Editor.Scripts.Helper.Logger;
using TextField = UnityEngine.UIElements.TextField;
namespace Editor
{
    /// <summary>
    /// UI 的工具窗口
    /// </summary>
    public partial class UIEditorWindow : EditorWindow
    {
        [MenuItem("Tools/UIFramework 配置")]
        public static void OpenWindow()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;
            GetWindow<UIEditorWindow>("UIFramework");
        }

        private const string UIResourcePathName = "UIResourcePath";
        private const string CreateNewScreenButtonName = "CreateNewScreen";
        private const string NewScreenName = "NewScreenName";
        private const string btnReInitUIFrameworkName = "btnReInitUIFramework";

        private VisualElement _root;
        private VisualElement _publishContentRoot;
        private TabbedMenuController _tabbedMenuController;
        private Button _createNewScreenButton;
        private TextField _newScreenNameTextField;
        private TextField _screenViewClassNameTextField;
        private TextField _screenPresenterClassNameTextField;
        private Button _btnReInitUIFramework;

        public void CreateGUI()
        {
            // 加载 UXml
            var visualTree = Resources.Load<VisualTreeAsset>("UXML/UIEditorWindow");
            _root = visualTree.Instantiate();
            rootVisualElement.Add(_root);

            // 查找 UIFramework 的配置类
            if (UIFrameworkSettings.Instance == null)
            {
                // 关闭窗口
                DelayClose();
                return;
            }

            // TODO: 如果 UI 的设置发生了变化, 应该提示用户, 以免不小心修改了重要目录导致生成出错

            // 添加各种事件处理
            // 处理 Tab 的切换
            _tabbedMenuController = new(_root);
            _tabbedMenuController.RegisterTabCallbacks();
            _tabbedMenuController.OnTabSelected += OnTabSelected;

            _publishContentRoot = _root.Q<VisualElement>("PublishContent");

            { // 绑定 UI Screen 创建控件
                _createNewScreenButton = _root.Q<Button>(CreateNewScreenButtonName);
                _createNewScreenButton.clicked += OnCreateScreenButtonClick;
                _newScreenNameTextField = _root.Q<TextField>(NewScreenName);
                _screenViewClassNameTextField = _root.Q<TextField>("ScreenViewClassName");
                _screenPresenterClassNameTextField = _root.Q<TextField>("ScreenPresenterClassName");
                _newScreenNameTextField.RegisterValueChangedCallback(changeEvent =>
                {
                    // 自动同步 View 与 Presenter 类的名称
                    var screenName = changeEvent.newValue;
                    if (string.IsNullOrEmpty(screenName))
                    {
                        _screenViewClassNameTextField.value = String.Empty;
                        _screenPresenterClassNameTextField.value = String.Empty;
                        return;
                    }
                    screenName = NameUtility.ToFirstUpper(screenName);
                    _screenViewClassNameTextField.value = UIFrameworkSettings.GetScreenViewClassName(screenName);
                    _screenPresenterClassNameTextField.value = UIFrameworkSettings.GetScreenPresenterClassName(screenName);
                });
            }

            { // 绑定 UI Framework 重新初始化控件
                _btnReInitUIFramework = _root.Q<Button>(btnReInitUIFrameworkName);
                _btnReInitUIFramework.clicked += OnReInitUIFrameworkClicked;
            }

            if (!IsUIInitialized())
            { // 如果还没有初始化 UI 系统, 则提示是否初始化
                if (EditorUtility.DisplayDialog("UIFramework", "UIFramework 还没有初始化, 是否初始化?", "是", "否"))
                {
                    if (!ResetUI())
                    {
                        Logger.LogError("UIFramework 初始化失败");
                        DelayClose();
                        return;
                    }
                }
                else
                {
                    // 关闭工具窗口
                    DelayClose();
                    return;
                }
            }

            // 监听 PlayMode 的切换, 不在编辑状态时无法使用本 UI
            UpdatePlayModeUI();
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;

            { // 更新 UI 配置的显示
                var uiSettingsFoldout = _root.Q<Foldout>("UISettingsFoldout");
                uiSettingsFoldout.value = false; // 默认关闭 UI 的设置
                SetTextField("tfSettingsClassName", UIFrameworkSettings.Instance.GetType().FullName);
                SetTextField("tfUINamespace", UIFrameworkSettings.UINamespace);
                SetTextField("tfUIProjectResPath", UIFrameworkSettings.UIProjectResPath);
                SetTextField("tfUIGenerationPath", UIFrameworkSettings.UIGenerationPath);
                SetTextField("tfUICodeScriptsPath", UIFrameworkSettings.UICodeScriptsPath);
            }

        }

        private void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
        }

        /// <summary>
        /// 初始化 UI 的显示
        /// </summary>
        private bool ResetUI()
        {
            try
            {
                // 确认一些关键目录必须存在
                Directory.CreateDirectory(UIFrameworkSettings.UIGenerationPath);
                Directory.CreateDirectory(UIFrameworkSettings.UIProjectResPath);
                Directory.CreateDirectory(UIFrameworkSettings.UICodeScriptsPath);

                // 生成默认的 Common Screen
                CreateUIScreenDirectory(UIFrameworkSettings.UICommonName);

                // 复制 UIRoot.prefab 到项目目录
                CopyUIRoot();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                // 自动选中 UIRoot.prefab
                {
                    var uiRoot = AssetDatabase.LoadAssetAtPath<GameObject>(UIFrameworkSettings.RootPrefabPath);
                    Selection.activeObject = uiRoot;
                    // 将焦点移动到 Project 窗口
                    EditorUtility.FocusProjectWindow();
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 设置文本字段的文本内容。
        /// </summary>
        /// <param name="tfName">文本字段的名称。</param>
        /// <param name="text">要设置的文本内容。</param>
        private void SetTextField(string tfName, string text)
        {
            var tf = _root.Q<TextField>(tfName);
            tf.value = text;
        }

        /// <summary>
        /// 重新初始化 UI Framework
        /// </summary>
        private void OnReInitUIFrameworkClicked()
        {
            if (!EditorUtility.DisplayDialog("UIFramework", "重新初始化 UI Framework 可能会丢失一些数据, 如 UIRoot.prefab 的修改等, 是否重新初始化 UI Framework?", "是", "否"))
            {
                return ;
            }

            ResetUI();
        }

        /// <summary>
        /// 当点击了创建 Screen 按钮时调用
        /// </summary>
        private void OnCreateScreenButtonClick()
        {
            var screenName = _newScreenNameTextField.text;
            if (string.IsNullOrEmpty(screenName))
            {
                EditorUtility.DisplayDialog("UIFramework", "请输入 Screen 名称", "OK");
                return;
            }
            
            screenName = NameUtility.ToFirstUpper(screenName);

            // 生成 Screen 相关的代码与 prefab
            if (!GenerateScreenViewCodeFileAndCompile(screenName))
                return;
            
            // 编译后继续此任务
            ContinueScreenViewGenerateTask(screenName);
            
            // 禁用此按钮, 避免重复点击, 等待代码的刷新
            _createNewScreenButton.SetEnabled(false);
        }

        private void OnTabSelected(string tabName)
        {

        }

        /// <summary>
        /// 延迟一帧关闭, 有时在某些调用栈中直接关闭窗口会出错. 比如在 CreateGUI() 函数中调用了 Close()
        /// </summary>
        /// <param name="refresh">关闭后是否刷新 AssetDatabase</param>
        private void DelayClose(bool refresh = false)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(_delayClose());

            IEnumerator _delayClose()
            {
                yield return null;
                Close();
                if (refresh)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        /// <summary>
        /// 返回 UI 系统是否已经初始化过了
        /// </summary>
        /// <returns></returns>
        private bool IsUIInitialized()
        {
            var uiRootAsset = AssetDatabase.LoadAssetAtPath<GameObject>(UIFrameworkSettings.RootPrefabPath);
            if (uiRootAsset == null)
                return false;
            return true;
        }

        void EditorApplicationOnplayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            UpdatePlayModeUI();
        }

        private void UpdatePlayModeUI()
        {
            var functionRoot = _root.Q("veFunctionRoot");
            functionRoot.style.display = EditorApplication.isPlaying ? DisplayStyle.None : DisplayStyle.Flex;
            var pauseUsingWarning = _root.Q("lbPauseUsingWarning");
            pauseUsingWarning.style.display = EditorApplication.isPlaying ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// 复制 UIRoot.prefab 到项目目录下
        /// </summary>
        private void CopyUIRoot()
        {
            // ReSharper disable once Unity.UnknownResource
            var loadRoot = Resources.Load<GameObject>("Prefabs/UIRoot");
            if (loadRoot == null)
            {
                UnityTipsHelper.ShowError($"原始的 UIRoot.prefab 不存在, 请确认它在 Packages/com.joyo.uiframework/Editor/Resources/Prefabs/UIRoot.prefab 目录下");
                throw new OperationCanceledException();
            }

            var newUIRoot = Instantiate(loadRoot);
            var canvasScaler = newUIRoot.GetComponentInChildren<CanvasScaler>();
            canvasScaler.referenceResolution = UIFrameworkSettings.ReferenceResolution;
            PrefabUtility.SaveAsPrefabAsset(newUIRoot, UIFrameworkSettings.RootPrefabPath);
            DestroyImmediate(newUIRoot);

            EditorUtility.DisplayDialog("UIFramework", $"复制 UIRoot.prefab 到项目目录({UIFrameworkSettings.RootPrefabPath})下成功, 请对应修改其中的全局参数, 如 Screen Match Mode 等, UIRoot 会在 UI 系统初始化时加载且全局唯一", "OK");
        }

        /// <summary>
        /// 初始化指定名称的 UI Screen 的目录
        /// </summary>
        /// <param name="screenName">指定要创建的 UI Screen 的名称</param>
        private static bool CreateUIScreenDirectory(string screenName)
        {
            // 创建各个目录
            var basePath = $"{UIFrameworkSettings.UIProjectResPath}/{screenName}";
            var prefabsPath = $"{basePath}/{UIFrameworkSettings.UIPrefabs}";
            var spritesPath = $"{basePath}/{UIFrameworkSettings.UISprites}";
            var spritesAtlas1Path = $"{basePath}/{UIFrameworkSettings.UISprites}/{UIFrameworkSettings.UISpritesAtlas1}";
            var atlasIgnorePath = $"{basePath}/{UIFrameworkSettings.UISprites}/{UIFrameworkSettings.UIAtlasIgnore}";
            var atlasPath = $"{basePath}/{UIFrameworkSettings.UIAtlas}";
            var sourcePath = $"{basePath}/{UIFrameworkSettings.UISource}";

            Directory.CreateDirectory(prefabsPath);
            Directory.CreateDirectory(spritesPath);
            Directory.CreateDirectory(spritesAtlas1Path);
            Directory.CreateDirectory(atlasIgnorePath);
            Directory.CreateDirectory(atlasPath);
            Directory.CreateDirectory(sourcePath);

            return true;
        }
    }
}
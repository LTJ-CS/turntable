using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace UIFramework
{
    /// <summary>
    /// 项目应该从此类派生一个类, 用于自定义UI框架的设置
    /// </summary>
    /// <remarks>一但开始生成 UI 相关的代码与资源后, 不应该随便修改 UIFrameworkSettings 派生类的设置, 以免导致找不到之前生成的资源, 运行出错</remarks>
    /// <remarks>
    /// 所有的重载的属性的值应该是运行时不变的
    /// </remarks>
    public abstract class UIFrameworkSettings
    {
        private static UIFrameworkSettings _instance;
        public static UIFrameworkSettings Instance
        {
            get
            {
                #if UNITY_EDITOR
                // 只在编辑器下才通过反射来查找派生的 UIFrameworkSettings, 并创建实例, 运行时需要手动做
                if (_instance == null)
                    FindUISettings();
                #endif
                return _instance;
            }
        }

        #region 需要重载的变量值

        /// <summary>
        /// UI根目录名称
        /// </summary>
        protected virtual string m_ProjectName => "UI";
        /// <summary>
        /// 所有生成的 UI 代码的全名空间
        /// </summary>
        protected virtual string m_Namespace => "MyUI";
        /// <summary>
        /// 项目资源路径, 项目的UI预设/图片等资源存放的地方
        /// </summary>
        protected virtual string m_UIProjectResPath => "Assets/GameRes/Runtime/" + UIProjectName;
        /// <summary>
        /// 自动生成的UI代码的路径
        /// </summary>
        protected virtual string m_GenerationPath => "Assets/GameScript/Runtime/UIGeneration";
        /// <summary>
        /// 玩家可编写的代码的路径
        /// </summary>
        protected virtual string m_CodeScriptsPath => "Assets/GameScript/Runtime/UI"; //玩家可编写的核心代码部分
        /// <summary>
        /// UI 的设计分辨率
        /// </summary>
        protected virtual Vector2 m_ReferenceResolution => new Vector2(1920, 1080);
        
        #endregion

        #region 公共静态属性

        // 简化使用
        /// <inheritdoc cref="m_ProjectName"/>
        public static string UIProjectName => Instance.m_ProjectName;
        /// <inheritdoc cref="m_Namespace"/>
        public static string UINamespace => Instance.m_Namespace;
        /// <inheritdoc cref="m_UIProjectResPath"/>
        public static string UIProjectResPath => Instance.m_UIProjectResPath;
        /// <inheritdoc cref="m_GenerationPath"/>
        public static string UIGenerationPath => Instance.m_GenerationPath;
        /// <inheritdoc cref="m_CodeScriptsPath"/>
        public static string UICodeScriptsPath => Instance.m_CodeScriptsPath;
        /// <inheritdoc cref="m_RootPrefabPath"/>
        public static string RootPrefabPath => Instance.m_RootPrefabPath;
        /// <inheritdoc cref="m_ReferenceResolution"/>
        public static Vector2 ReferenceResolution => Instance.m_ReferenceResolution;
        /// <inheritdoc cref="m_UIPanelSourceName"/>
        public static string UIScreenSourceName => Instance.m_UIPanelSourceName;
        /// <inheritdoc cref="m_UIDisplayParentName"/>
        public static string UIDisplayParentName => Instance.m_UIDisplayParentName;
        /// <inheritdoc cref="m_UIDisplayName"/>
        public static string UIDisplayName => Instance.m_UIDisplayName;
        
        public const string UIRootName = "UIRoot";
        public const string UICommonName             = "Common";
        public const string UIPanelName              = "Screen";
        public const string UIViewName               = "View";
        public const string UIParentName             = "Parent";
        public const string UIPrefabs                = "Prefabs";
        public const string UIPrefabsCN              = "预制";
        public const string UISprites                = "Sprites";
        public const string UISpritesCN              = "精灵";
        public const string UIAtlas                  = "Atlas";
        public const string UIAtlasCN                = "图集";
        public const string UISource                 = "Source";
        public const string UISourceCN               = "源文件";
        public const string UIAtlasIgnore            = "AtlasIgnore"; //图集忽略文件夹名称
        public const string UISpritesAtlas1          = "Atlas1";      //图集1 不需要华丽的取名 每个包内的自定义图集就按顺序就好 当然你也可以自定义其他
        public const string UIAllViewParentName      = "AllViewParent";
        public const string UIAllPopupViewParentName = "AllPopupViewParent";
        public const string UIPanelSourceName        = UIPanelName + UISource;
        public const string ScreenViewSuffix        = "ScreenView";
        public const string ScreenPresenterSuffix   = "ScreenPresenter";

        /// <summary>
        /// 给定 Screen 的名称返回它对应的 View 的类名. MVP 模式
        /// </summary>
        /// <param name="screenName">Screen 的名称</param>
        /// <returns></returns>
        public static string GetScreenViewClassName(string screenName)
        {
            return $"{screenName}{ScreenViewSuffix}";
        }

        /// <summary>
        /// 给定 Screen 的名称返回它对应的 Presenter 的类名. MVP 模式
        /// </summary>
        /// <param name="screenName">Screen 的名称</param>
        /// <returns></returns>
        public static string GetScreenPresenterClassName(string screenName)
        {
            return $"{screenName}{ScreenPresenterSuffix}";
        }

        /// <summary>
        ///  由给定的 ScreenPresenter 的名称来获取 Screen 的名称
        /// </summary>
        /// <param name="presenterName">指定要获取 Screen 的名称的 ScreenPresenter 名称</param>
        /// <returns></returns>
        public static string GetScreenNameFromPresenter(string presenterName)
        {
            return presenterName.Substring(0, presenterName.Length - ScreenPresenterSuffix.Length);
        }

        /// <summary>
        /// 返回Screen 的 View 代码文件路径
        /// </summary>
        /// <param name="screenName">指定要获取 View 代码文件路径的的 Screen</param>
        /// <returns></returns>
        public static string GetScreenViewCodeFilePath(string screenName)
        {
            return Path.Combine(UIGenerationPath, GetScreenViewClassName(screenName) + ".cs");
        }
        
        /// <summary>
        /// 返回Screen 的 Presenter 代码文件路径
        /// </summary>
        /// <param name="screenName">指定要获取 Presenter 代码文件路径的的 Screen</param>
        /// <returns></returns>
        public static string GetScreenPresenterCodeFilePath(string screenName)
        {
            return Path.Combine(UIGenerationPath, GetScreenPresenterClassName(screenName) + ".cs");
        }

        /// <summary>
        /// 返回 Screen 的 View 的编辑源 prefab 的名称
        /// </summary>
        /// <param name="screenName">指定要获取 View 的编辑源 prefab 的名称的 Screen</param>
        /// <returns></returns>
        public static string GetScreenViewSourceName(string screenName)
        {
            return $"{screenName}{UIPanelSourceName}";
        }

        /// <summary>
        /// 获取指定名称的 Screen 的 prefab 的路径
        /// </summary>
        /// <returns></returns>
        public static string GetScreenViewPrefabPath(string screenName)
        {
            var basePath          = $"{UIProjectResPath}/{screenName}";
            var prefabPath       = $"{basePath}/{UIPrefabs}/{screenName}{ScreenViewSuffix}.prefab";
            return prefabPath;
        }
        
        /// <summary>
        /// 获取指定名称的 Display 的 prefab 的路径
        /// </summary>
        /// <returns></returns>
        public static string GetScreenDisplayPrefabPath(string screenName,string displayName)
        {
            var basePath          = $"{UIProjectResPath}/{screenName}";
            var prefabPath       = $"{basePath}/{UIPrefabs}/{displayName}.prefab";
            return prefabPath;
        }
        
        #endregion

        #region 私有属性
        
        /// <summary>
        /// UI根节点的预设路径
        /// </summary>
        private readonly string m_RootPrefabPath;
        private readonly string m_UIDisplayParentName;
        private readonly string m_UIPanelSourceName;
        private readonly string m_UIDisplayName;
        
        #endregion
        
        protected UIFrameworkSettings()
        {
            _instance = this;
            m_RootPrefabPath = $"{UIProjectResPath}/Common/Prefabs/{UIRootName}.prefab";
            m_UIPanelSourceName = UIPanelName + UISource;
            m_UIDisplayName        = UIProjectName + UIViewName;
            m_UIDisplayParentName  = UIProjectName + UIViewName + UIParentName;
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// 查找 UI 的设置类
        /// </summary>
        public static UIFrameworkSettings FindUISettings()
        {
            // 获取当前应用程序域中的所有程序集
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<Type> uiSettingsTypes = new();

            // 遍历所有程序集
            foreach (Assembly assembly in assemblies)
            {
                // 获取程序集中的所有类型
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(UIFrameworkSettings)))
                    {
                        uiSettingsTypes.Add(type);
                    }
                }
            }

            if (uiSettingsTypes.Count == 0)
            {
                EditorUtility.DisplayDialog("UIFramework 配置", "未找到从 UIFrameworkSettings 派生的类, 请确保从\"UIFramework.UIFrameworkSettings\"派生一个类, 并设置相关的路径值, 然后再重新打开本窗口", "确定");
                return null;
            }

            if (uiSettingsTypes.Count > 1)
            {
                // 把 uiSettingsTypes 的全名拼接起来
                string fullNames = string.Join(", ", uiSettingsTypes.Select(x => x.FullName).ToArray());
                Debug.LogError($"找到多个从 UIFrameworkSettings 派生的类: {fullNames}");
                EditorUtility.DisplayDialog("UIFramework 配置", "找到多个从 UIFrameworkSettings 派生的类, 请确保只有一个类, 然后再重新查找", "确定");
                return null;
            }

            return (UIFrameworkSettings)Activator.CreateInstance(uiSettingsTypes[0]);
        }
        #endif  
    }
}
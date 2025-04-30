using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Sdk.Editor.Scripts.Utilities
{
    /// <summary>
    /// This class auto-loads a bootstrap screen (first scene in Build Settings) while working in the Editor.
    /// It also adds menu items to toggle behavior. 
    /// </summary>
    /// 
    // This SceneBootstrapper is adapted from here:
    // https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop/blob/main/Assets/Scripts/Editor/SceneBootstrapper.cs
    [InitializeOnLoad]
    public class SceneBootstrapper
    {
        // Use these keys for Editor preferences
        private const string k_PreviousScene       = "PreviousScene";
        private const string k_ShouldLoadBootstrap = "LoadBootstrapScene";
        private const int    MenuPriority          = 100;

        // These appear as menu names
        private const string k_LoadBootstrapMenu     = "Tools/Load Bootstrap Scene On Play";
        private const string k_DontLoadBootstrapMenu = "Tools/Don't Load Bootstrap Scene On Play";

        // This gets the bootstrap scene, which must be first scene in Build Settings
        private static string BootstrapScene => EditorBuildSettings.scenes[0].path;

        /// <summary>
        /// 是否临时禁用一次启动场景
        /// </summary>
        private static bool _isDisableBootstrapOnce = false;

        // This string is the scene name where we entered Play mode 
        private static string PreviousScene
        {
            get => EditorPrefs.GetString(k_PreviousScene);
            set => EditorPrefs.SetString(k_PreviousScene, value);
        }

        // Is the bootstrap behavior enabled?
        private static bool ShouldLoadBootstrapScene
        {
            get => EditorPrefs.GetBool(k_ShouldLoadBootstrap, true);
            set => EditorPrefs.SetBool(k_ShouldLoadBootstrap, value);
        }

        // A static constructor runs with InitializeOnLoad attribute
        static SceneBootstrapper()
        {
            // Listen for the Editor changing play states
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        /// <summary>
        /// 临时禁用打开启动场景一次
        /// </summary>
        public static void DisableBootstrapOnce()
        {
            _isDisableBootstrapOnce = true;
        }

        // This runs when the Editor changes play state (e.g. entering Play mode, exiting Play mode)
        private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            // Do nothing if disabled from the menus
            if (!ShouldLoadBootstrapScene)
            {
                return;
            }

            switch (playModeStateChange)
            {
                // This loads bootstrap scene when entering Play mode
                case PlayModeStateChange.ExitingEditMode:

                    PreviousScene = SceneManager.GetActiveScene().path;

                    if (!_isDisableBootstrapOnce)
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() && IsSceneInBuildSettings(BootstrapScene))
                        {
                            EditorSceneManager.OpenScene(BootstrapScene);
                        }
                    }
                    _isDisableBootstrapOnce = false; // 关闭临时启动场景的禁用的标识
                    break;

                // This restores the PreviousScene when exiting Play mode
                case PlayModeStateChange.EnteredEditMode:

                    if (!string.IsNullOrEmpty(PreviousScene))
                    {
                        EditorSceneManager.OpenScene(PreviousScene);
                    }

                    break;
            }
        }

        // These menu items toggle behavior.

        // This adds a menu item called "Load Bootstrap Scene On Play" to the GameSystems menu and
        // enables the behavior if selected.
        [MenuItem(k_LoadBootstrapMenu, false, MenuPriority)]
        private static void EnableBootstrapper()
        {
            ShouldLoadBootstrapScene = !ShouldLoadBootstrapScene;
        }

        // Validates the above function and menu item, which grays out if ShouldLoadBootstrapScene is true.
        [MenuItem(k_LoadBootstrapMenu, true, MenuPriority)]
        private static bool ValidateEnableBootstrapper()
        {
            Menu.SetChecked(k_LoadBootstrapMenu, ShouldLoadBootstrapScene);
            return true;
        }

        // Is a scenePath a valid scene in the File > Build Settings?
        private static bool IsSceneInBuildSettings(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
                return false;

            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.path == scenePath)
                {
                    return true;
                }
            }

            return false;
        }

        // This is a more compact version of the same IsSceneInBuildSettings logic:

        //private static bool IsSceneInBuildSettings(string scenePath)
        //{
        //    return !string.IsNullOrEmpty(scenePath) &&
        //           System.Array.Exists(EditorBuildSettings.scenes, scene => scene.path == scenePath);
        //}
    }
}
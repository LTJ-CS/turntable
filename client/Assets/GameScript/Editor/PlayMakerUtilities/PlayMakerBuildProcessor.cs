using System;
using System.Collections.Generic;
using GameScript.Runtime.GameLogic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace GameScript.Editor.PlayMakerUtilities
{
    /// <summary>
    /// 在构建时自动收集 PlayMaker 使用的 Action 类型, 并生成相应的 Link.xml
    /// </summary>
    public class PlayMakerBuildProcessor : BuildPlayerProcessor
    {
        /// <summary>
        /// 生成的 PlayMaker 使用的 Action 类型的 Link.xml 路径
        /// </summary>
        public const string PlayMakeLinkXmlPath = "Assets/ThirdParty/PlayMaker/link.xml";

        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            CreateLinkXml();
        }

        /// <summary>
        /// 生成 PlayMaker 使用的 Action 类型的 Link.xml
        /// </summary>
        [MenuItem("PlayMaker/Debug/Create Link.xml")]
        private static void CreateLinkXml()
        {
            // 用于生成 PlayMaker 使用的 Action 类型的 Link.xml
            var linker = UnityEditor.Build.Pipeline.Utilities.LinkXmlGenerator.CreateDefault();
            var actionsType = new List<Type>();


            // 加载直接使用了 PlayMaker 的场景, 我们需要一个一个场景来加载并处理, 否则 PlayMaker 通过 Resources.FindObjectsOfTypeAll(typeof (PlayMakerFSM)) 查找不到对应的 PlayMakerFSM.
            {
                var activeScene = SceneManager.GetActiveScene();
                var activeScenePath = activeScene.path;
                
                var scenes = AssetDatabase.FindAssets("t:Scene");
                foreach (var sceneGuid in scenes)
                {
                    var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                    if (scenePath.StartsWith("Assets/GameRes/Runtime/"))
                    {
                        foreach (string dependency in AssetDatabase.GetDependencies(scenePath))
                        {
                            if (dependency.Contains("/PlayMaker.dll"))
                            {
                                EditorSceneManager.OpenScene(scenePath);

                                // 每打开一个场景就添加一次这个场景用到的 PlayMakerFSM
                                AddActionsType();
                                break;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(activeScenePath))
                {
                    EditorSceneManager.OpenScene(activeScenePath);
                }
            }
            {
                // 先加载所有使用了 PlayMaker 的 Prefab, 这样才能保证查找到所有的 Action
                HutongGames.PlayMakerEditor.Files.LoadAllTemplates();
                HutongGames.PlayMakerEditor.Files.LoadAllPlaymakerPrefabs();
                AddActionsType();
            }

            linker.AddTypes(actionsType);
            if(false){
                // 保留所有的 Assembly-CSharp 中的类型, 方便通过只更新资源来扩展游戏功能. 这会至少增加 0.5M 的首包大小
                linker.AddAssemblies(typeof(GameInstance).Assembly);
                Debug.Log("[PlayMakerBuildProcessor] 保留了 Assembly-CSharp 中的所有类型, 至少增加 0.5M 首包大小!");
            }
            linker.Save(PlayMakeLinkXmlPath);
            AssetDatabase.ImportAsset(PlayMakeLinkXmlPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.DontDownloadFromCacheServer);
            Debug.Log($"[PlayMakerBuildProcessor] 为 PlayMaker 自动收集使用的 Action 类型: {PlayMakeLinkXmlPath}");

            // 添加 PlayMaker 使用的 Action 类型
            void AddActionsType()
            {
                // 收集 FSM
                HutongGames.PlayMakerEditor.FsmEditor.RebuildFsmList();

                // 遍历所有的 FSM
                foreach (var fsm in HutongGames.PlayMakerEditor.FsmEditor.FsmList)
                {
                    // 遍历所有的 FSM 中的所有 State
                    foreach (FsmState state in fsm.States)
                    {
                        // 遍历所有的 State 中的所有 Action
                        foreach (var stateAction in state.Actions)
                        {
                            var type = stateAction.GetType();
                            actionsType.Add(type);

                            if (type == typeof(StartCoroutine))
                            {
                                var startCoroutine = (StartCoroutine)stateAction;
                                actionsType.Add(ReflectionUtils.GetGlobalType(startCoroutine.behaviour.Value));
                            }
                            else if (type == typeof(CallMethod))
                            {
                                var callMethod = (CallMethod)stateAction;
                                actionsType.Add(callMethod.behaviour.Value.GetType());
                            }
                            else if (type == typeof(CallStaticMethod))
                            {
                                var callStaticMethod = (CallStaticMethod)stateAction;
                                actionsType.Add(ReflectionUtils.GetGlobalType(callStaticMethod.className.Value));
                            }
                        }
                    }
                }
            }
        }
    }
}
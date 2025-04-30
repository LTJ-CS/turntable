using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameScript.Runtime.GameLogic;
using MyUI;
using Sdk.Runtime.Base;
using UIFramework;
using UnityEngine;
namespace GameScript.Runtime.UI
{
    /// <summary>
    /// UI 的初始化管理器, 全局唯一
    /// </summary>
    public class UIBootstrap : MonoBehaviour,IInitializable
    {
        /// <summary>
        /// 供 FirstScreenComponent 调用, 用于添加 UI 初始化等动作
        /// </summary>
        /// <param name="actions">用于添加登录动作的列表</param>
        public void RegisterUIInitAction(List<Func<UniTask>> actions)
        {
            actions.Add(InitUI);
        }

        /// <summary>
        /// 初始化 YIUI
        /// </summary>
        private async UniTask InitUI()
        {
            // 等待 UIFramework 初始化完成
            await UIFramework.UIManager.Init<MyUIFrameworkSettings>();
            await NetLoading.Init();
        }
        public void Register()
        {
           
        }
        public  void GetPreloadPath(ref List<string> pathList)
        {
           pathList.Add(NetLoading.LoadingResPath);
        }

        public async UniTask InitializeAsync()
        {
            await InitUI();
        }
    }
}
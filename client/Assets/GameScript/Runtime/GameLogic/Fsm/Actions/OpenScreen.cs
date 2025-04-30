using System;
using System.Collections.Generic;
using GameScript.Runtime.GameLogic.Events;
using GameScript.Runtime.GameLogic.Fsm.Base;
using HutongGames.PlayMaker;
using MyUI;
using UIFramework;
using UnityEngine;

namespace GameScript.Runtime.GameLogic.Fsm.Actions
{
    /// <summary>
    /// 实现要打开的 UI 的类型
    /// </summary>
    public enum EUiType
    {
        None,               // 无效值
        UserAuthorization,  // 用户授权 Screen
        MainMenu,           // 主菜单 Screen
        PlotScreen,         // 剧情 Screen
        SelectScene,        // 选择进入关卡 Screen
    }
    
    [ActionCategory(MyActionCategory.Helper)]
    [HutongGames.PlayMaker.Tooltip("打开一个 Screen")]
    public class OpenScreen : FsmStateAction
    {
        [RequiredField]
        [HutongGames.PlayMaker.Tooltip("要打开的 Screen 的类型")]
        
        [ObjectType(typeof(EUiType))]
        // ReSharper disable once UnassignedField.Global
        public FsmEnum ScreenType;
        
        private static readonly Dictionary<EUiType, Action> UITypeToAction = new()
        {
            // {EUiType.UserAuthorization, UIManager.OpenScreen<UserAuthorizationScreenPresenter>},
            // {EUiType.SelectScene, UIManager.OpenScreen<SelectSceneScreenPresenter>},
            // {EUiType.PlotScreen, UIManager.OpenScreen<FirstPlotScreenPresenter>},
            // TODO: 添加其它 UI 的打开代码
        };
        
        public override void OnEnter()
        {
            if (UITypeToAction.TryGetValue((EUiType)ScreenType.Value, out var uiOpenAction))
            {
                if ((EUiType)ScreenType.Value == EUiType.MainMenu)
                {
                    // 尝试调用MainUI的ReEnter方法，因为Display切换时并不会重新创建UI
                    MainUIEvents.RiseReEnterMainScreenEvent();
                }
                uiOpenAction();
            }
            else
            {
                Debug.LogError($"{nameof(OpenScreen)}: {ScreenType.Value} is not supported");
            }
            
            Finish();
        }
    }
}
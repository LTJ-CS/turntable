using GameScript.Runtime.GameLogic.Events;
using GameScript.Runtime.GameLogic.Fsm.Base;
using GameScript.Runtime.GameLogic.Managers;
using HutongGames.PlayMaker;
using MyUI;
using Protocol;
using UIFramework;
using UnityEngine;

namespace GameScript.Runtime.GameLogic.Fsm.MainFsm
{
    /// <summary>
    /// 关闭主场景的所有弹窗页面
    /// </summary>
    [ActionCategory(MyActionCategory.MainUI)]
    [HutongGames.PlayMaker.Tooltip("Main场景关闭所有Screen")]
    public class MainCloseAllScreenState : FsmStateAction
    {
        public override void OnEnter()
        {
            base.OnEnter();

            if (UIManager.IsValid)
            {

            }

            Finish();
        }
    }
}
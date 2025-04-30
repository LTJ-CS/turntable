using DG.Tweening;
using GameScript.Runtime.GameLogic.Events;
using GameScript.Runtime.GameLogic.Fsm.Base;
using HutongGames.PlayMaker;
using UnityEngine;

namespace GameScript.Runtime.GameLogic.Fsm.MainFsm
{
    /// <summary>
    /// 过渡结束状态
    /// </summary>
    [ActionCategory(MyActionCategory.MainUI)]
    [HutongGames.PlayMaker.Tooltip("过渡结束状态")]
    public class TransitionEndState : FsmStateAction
    {
        public override void OnEnter()
        {
            GameInstance.Instance.m_TransEnd = true;
            Finish();
        }
    }
}
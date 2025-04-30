using GameScript.Runtime.GameLogic.Fsm.Base;
using HutongGames.PlayMaker;

[ActionCategory(MyActionCategory.Helper)]
[HutongGames.PlayMaker.Tooltip("播放音效")]
public class PlaySoundState : FsmStateAction
{
    [RequiredField]
    [HutongGames.PlayMaker.Tooltip("填1或者2 哪一阶段的音乐,")]
    public string SoundName;

    [RequiredField]
    [HutongGames.PlayMaker.Tooltip("是否立刻播放")]
    public bool IsImmediately;

    public override void OnEnter()
    {
        SoundManager.Instance.PlayUISound(SoundName, IsImmediately);
        
        Finish();
    }
}
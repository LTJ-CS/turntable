using System;
using UnityEngine;
using UnityEngine.UI;

public class SoundToggleEffect : MonoBehaviour
{
   private Toggle toggle;
   public  string m_SoundName = "uiClick";
   public  bool   m_IsGroupToggle = false;
   private void Start()
   {
      if (toggle == null)
         toggle = GetComponent<Toggle>();
      if(toggle != null)
         toggle.onValueChanged.AddListener(OnPlaySound);
   }

   void OnPlaySound(bool isOn)
   {
      if(m_IsGroupToggle && !isOn) //如果是组的toggle,则只有被选中时播放声音
         return;
         
      if(string.IsNullOrEmpty(m_SoundName))
         SoundManager.Instance.PlayUISound(SoundNameUtil.UiClick);
      else
      {
         SoundManager.Instance.PlayUISound(m_SoundName);
      }
   }

   private void OnDestroy()
   {
      if (toggle != null)
         toggle.onValueChanged.RemoveListener(OnPlaySound);
   }
}

using System;
using UnityEngine;
using UnityEngine.UI;

public class SoundButtonEffect : MonoBehaviour
{
   private Button button;
   public string m_SoundName = "uiClick";
   private void Start()
   {
      if (button == null)
         button = GetComponent<Button>();
      if(button != null)
         button.onClick.AddListener(OnPlaySound);
   }

   void OnPlaySound()
   {
      if(string.IsNullOrEmpty(m_SoundName))
         SoundManager.Instance.PlayUISound(SoundNameUtil.UiClick);
      else
      {
         SoundManager.Instance.PlayUISound(m_SoundName);
      }
   }

   private void OnDestroy()
   {
      if (button != null)
         button.onClick.RemoveListener(OnPlaySound);
   }
}

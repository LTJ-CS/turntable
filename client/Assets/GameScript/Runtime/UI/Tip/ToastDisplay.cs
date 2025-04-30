
using TMPro;
using UnityEngine;

namespace MyUI
{
    public class ToastDisplay : DisplayView
    {
        public Animation       aniGo;
        public TextMeshProUGUI descText;

        private readonly float[] aniLength = new float[] { -1, -1 };

        private float timeCounter = 0;

        private bool   isPlayingHideAni = false;
        private float  hideAniCounter   = 0;
        private string curShowContent;

        void InitAni()
        {
            int count = 0;
            foreach (AnimationState state in aniGo)
            {
                if (state.clip != null)
                {
                    aniLength[count] = state.clip.length;
                    count++;
                }

                if (count == 2)
                    break;
            }
        }
        
        public void ShowToast(string content, float delayHide = 1f)
        {
            if (aniLength[0] == -1)
            {
                InitAni();
            }

            Visible = true;
            timeCounter = delayHide + aniLength[0];
            descText.text = content;
            aniGo.Play("waitNetShowTip");
        }

        private void Update()
        {
            if (timeCounter > 0)
            {
                timeCounter -= Time.deltaTime;
                if (timeCounter <= 0)
                {
                    aniGo.Play("waitNetHideTip");
                    isPlayingHideAni = true;
                    hideAniCounter = aniLength[1];
                }
            }

            if (isPlayingHideAni)
            {
                if (hideAniCounter <= 0)
                {
                    isPlayingHideAni = false;
                    hideAniCounter = 0;
                }
                else
                {
                    hideAniCounter -= Time.deltaTime;
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using TMPro;
using UIFramework.UIScreen;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace MyUI
{
    /// <summary>
    /// Toast 的 View 类   
    /// </summary>
    public sealed partial class ToastScreenView
    {
        public Animation aniGo;
        public TextMeshProUGUI descText;

        private Queue<(string, float)> contentQueue;
        private float timeCounter = 0;
        private float[] aniLength = new float[2];
        private bool initAniLength = false;
        private bool isPlayingHideAni = false;
        private float hideAniCounter = 0;
        private string curShowContent;
        public event Action OnPlayToast;

        public void ShowToast(string content, float delayHide = 1f)
        {
            if (!initAniLength)
            {
                initAniLength = true;
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
            
            timeCounter = delayHide + aniLength[0];
            descText.text = content;
            aniGo.Play("showToastAni");
        }

        private void Update()
        {
            if (timeCounter > 0)
            {
                timeCounter -= Time.deltaTime;
                if (timeCounter <= 0)
                {
                    aniGo.Play("hideToastAni");
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
                    OnPlayToast?.Invoke();
                }
                else
                {
                    hideAniCounter -= Time.deltaTime;
                }
            }
        }
    }
}
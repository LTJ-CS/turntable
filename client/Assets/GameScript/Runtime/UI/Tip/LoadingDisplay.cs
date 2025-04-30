using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

namespace MyUI
{
    public class LoadingDisplay : DisplayView
    {
        [SerializeField]
        private TextMeshProUGUI m_Tip;

        [Header("省略号个数")]
        [SerializeField] private int m_TotalDotCount = 3;

        private readonly StringBuilder baseText = new StringBuilder("网络连接中");

        private readonly WaitForSeconds wait = new WaitForSeconds(0.5f);

        private int dotCount;

        private readonly StringBuilder curText = new StringBuilder();

        private Coroutine coroutine;
        private string[]  dotStates;

        void InitDot()
        {
            if(dotStates != null)
                return;
            dotStates = new string[m_TotalDotCount];
            for (int i = 0; i < dotStates.Length; i++)
            {
                if (i == 0)
                    dotStates[i] = ".";
                else
                {
                    dotStates[i] = dotStates[i - 1] + ".";
                }
            }

        }

        private void OnEnable()
        {
            InitDot();
            // 开始协程
            coroutine = StartCoroutine(UpdateText());
        }

        private void OnDisable()
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = null;
        }

        IEnumerator UpdateText()
        {
            while (true)
            {
                curText.Append(baseText).Append(dotStates[dotCount]);

                m_Tip.text = curText.ToString();
                curText.Clear();
                // 更新点的数量
                dotCount = (dotCount + 1) % m_TotalDotCount;

                // 等待一段时间
                yield return wait;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        void OnDestroy()
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}
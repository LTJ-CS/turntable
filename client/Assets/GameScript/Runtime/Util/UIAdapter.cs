using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameScript.Runtime.Util
{
    public class UIAdapter : MonoBehaviour
    {
        [Header("分辨率1422*640时")]
        [SerializeField]
        private float m_High;

        [Header("分辨率1152*640时")]
        [SerializeField]
        private float m_Low;

        private void Start()
        {
            var value = (float)Screen.width / Screen.height * 640;
            var y = Mathf.Lerp(m_Low, m_High, (value - 1152) / (1422 - 1152));
            var rect = GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
        }
    }
}
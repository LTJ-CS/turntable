using UnityEngine;
using UnityEngine.UI;

namespace MyUI
{
    public class CardItem : MonoBehaviour
    {
        [SerializeField] private Image m_Icon;

        public void SetImage(Sprite sprite)
        {
            m_Icon.sprite = sprite;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
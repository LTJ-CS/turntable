using UnityEngine;

namespace MyUI
{
    public enum PopupShowBtnType
    {
        ConfirmAndCancel,
        Confirm,
    }
    public class DisplayView : MonoBehaviour
    {
        private bool _visible;
        public bool Visible
        {
            get
            {
                if (gameObject != null)
                    return gameObject.activeSelf;
                return false;
            }
            set
            {
                if (gameObject!= null && gameObject.activeSelf != value)
                {
                    gameObject.SetActive(value);
                }
            }
        }
    }
}

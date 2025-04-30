using System;
using TMPro;
using UnityEngine.UI;
// ReSharper disable once CheckNamespace
using UnityEngine;

namespace MyUI
{
    /// <summary>
    /// Offline 的 View 类   
    /// </summary>
    public class OfflineDisplay : DisplayView
    {
        [SerializeField]
        private TextMeshProUGUI m_ConfirmTipText;
        
        [SerializeField]
        private TextMeshProUGUI m_ErrorCodeText;
        
        [SerializeField]
        private Button m_TryBtn;

        [SerializeField]
        private Button m_BackBtn;

        [SerializeField]
        private Button m_CloseBtn;


        private PopupShowBtnType _popupShowBtnType = PopupShowBtnType.ConfirmAndCancel;
        private void Start()
        {
            m_TryBtn.onClick.AddListener(OnClickTry);
            m_BackBtn.onClick.AddListener(OnClickBack);
            m_CloseBtn.onClick.AddListener(OnClickBack);
        }

        public void SetPopupShowBtnType(PopupShowBtnType btnType)
        {
            if (btnType != _popupShowBtnType)
            {
                _popupShowBtnType = btnType;
                switch (_popupShowBtnType)
                {
                    case PopupShowBtnType.ConfirmAndCancel:
                        m_TryBtn.gameObject.SetActive(true);
                        m_BackBtn.gameObject.SetActive(true);
                        m_CloseBtn.gameObject.SetActive(true);
                        break;
                    case PopupShowBtnType.Confirm:
                        m_TryBtn.gameObject.SetActive(true);
                        m_BackBtn.gameObject.SetActive(false);
                        m_CloseBtn.gameObject.SetActive(false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        void OnClickTry()
        {
            CommonUIEvents.RaiseOfflineTry();
        }
        public void SetConfirmTip(string tip,string errorCode)
        {
            m_ConfirmTipText.text = string.IsNullOrEmpty(tip) ? "网络异常，请稍后重试" : tip;
            m_ErrorCodeText.text = string.IsNullOrEmpty(errorCode) ? "" : errorCode;
        }
        void OnClickBack()
        {
           CommonUIEvents.RaiseCloseOfflineDisplay();
        }
        
        
    }
}

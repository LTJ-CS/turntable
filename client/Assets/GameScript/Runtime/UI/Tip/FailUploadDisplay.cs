
using TMPro;
using UnityEngine.UI;
// ReSharper disable once CheckNamespace
using UnityEngine;

namespace MyUI
{
    /// <summary>
    /// FailUploadLevel 的 View 类   
    /// </summary>
    public  class FailUploadDisplay : DisplayView
    {
        [SerializeField]
        private TextMeshProUGUI m_ConfirmTipText;
        
        [SerializeField]
        private Button m_ConfirmBackBtn;

        [SerializeField]
        private Button m_BackBtn;

        [SerializeField]
        private Button m_CloseBtn;

        private void Start()
        {
            m_ConfirmBackBtn.onClick.AddListener(OnClickConfirmBack);
            m_BackBtn.onClick.AddListener(OnClickBack);
            m_CloseBtn.onClick.AddListener(OnClickBack);
        }

        public void SetConfirmTip(string tip)
        {
            m_ConfirmTipText.text = string.IsNullOrEmpty(tip) ? "数据有可能会丢失哦" : tip;
        }
        void OnClickConfirmBack()
        {
            CommonUIEvents.RaiseGiveUpDataUpLoad();
        }

        void OnClickBack()
        {
            CommonUIEvents.RaiseCloseFailUpLoadDisplay();
        }

        
    }
}

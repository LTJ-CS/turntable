using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// ReSharper disable once CheckNamespace
namespace UIFramework.Components
{
    /// <summary>
    /// 实现对 TextMeshProUGUI 超链接的处理回调
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMPTextLink : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        [Tooltip("超链接点击回调")]
        private UnityEvent<TMP_LinkInfo> m_OnLinkClick;
        
        public void OnPointerClick(PointerEventData eventData)
        {   
            var text = GetComponent<TextMeshProUGUI>();
            // If you are not in a Canvas using Screen Overlay, put your camera instead of null
            Debug.AssertFormat(text.canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay, "目前只支持 RenderMode.ScreenSpaceOverlay");
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, null);
            if (linkIndex != -1)
            {
                var linkInfo = text.textInfo.linkInfo[linkIndex];
                m_OnLinkClick.Invoke(linkInfo);
                return;
            }
            
            // 没有点击到超链接, 传递点击事件到父对象, 有些组件需要处理这个事件, 比如 Toggle
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.pointerClickHandler);
        }
    }
}
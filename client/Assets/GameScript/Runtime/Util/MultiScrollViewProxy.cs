using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameScript.Runtime.Util
{
    public class MultiScrollViewProxy : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IScrollHandler
    {
        [SerializeField] private ScrollRect parentScrollView;
        [SerializeField] private ScrollRect childScrollView;

        private bool    isDraggingParent        = false;
        private bool    isDraggingChild         = false;
        private Vector2 pointerStartLocalCursor = Vector2.zero;
        private bool    isDragging              = false;

        private void OnDisable()
        {
            isDragging = false;
            isDraggingParent = false;
            isDraggingChild = false;
        }

        private bool IsReachTop(ScrollRect scrollView)
        {
            return scrollView.content.anchoredPosition.y < 0;
        }

        private bool IsReachBottom(ScrollRect scrollView)
        {
            return scrollView.content.anchoredPosition.y > 0;
        }


        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentScrollView.viewport, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            var deltaPos = localCursor - pointerStartLocalCursor;

            if (!isDraggingParent && !isDraggingChild)
            {
                if (deltaPos.y > 0) //向上滑动
                {
                    childScrollView.OnBeginDrag(eventData);
                    isDraggingChild = true;
                }
                else if (deltaPos.y < 0) //向下滑动
                {
                    if (IsReachTop(childScrollView))
                    {
                        parentScrollView.OnBeginDrag(eventData);
                        childScrollView.OnEndDrag(eventData);
                        isDraggingParent = true;
                    }
                    else
                    {
                        childScrollView.OnBeginDrag(eventData);
                        isDraggingChild = true;
                    }
                }
            }

            if (isDraggingParent)
            {
                if (eventData.delta.y > 0)
                {
                    return;
                }
                parentScrollView.OnDrag(eventData);
                if (deltaPos.y > 0)
                {
                    if (IsReachBottom(parentScrollView))
                    {
                        isDraggingParent = false;
                        isDraggingChild = true;
                        parentScrollView.OnEndDrag(eventData);
                        childScrollView.OnBeginDrag(eventData);
                    }
                }
            }

            if (isDraggingChild)
            {
                parentScrollView.content.anchoredPosition = new Vector3(parentScrollView.content.anchoredPosition.x,0, 0);
                childScrollView.OnDrag(eventData);
                if (deltaPos.y < 0)
                {
                    if (IsReachTop(childScrollView))
                    {
                        isDraggingChild = false;
                        isDraggingParent = true;
                        childScrollView.OnEndDrag(eventData);
                        parentScrollView.OnBeginDrag(eventData);
                    }
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            pointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentScrollView.viewport, eventData.position, eventData.pressEventCamera, out pointerStartLocalCursor);
            isDragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            isDragging = false;
            isDraggingParent = false;
            isDraggingChild = false;
            parentScrollView.OnEndDrag(eventData);
            childScrollView.OnEndDrag(eventData);
        }

        public void OnScroll(PointerEventData eventData)
        {
            Debug.LogError(22222);
            if (!this.enabled)
                return;
            Vector2 delta = eventData.scrollDelta;

            if (delta.y > 0) //向下滚动
            {
                if (IsReachTop(childScrollView))
                {
                    parentScrollView.OnScroll(eventData);
                }
                else
                {
                    childScrollView.OnScroll(eventData);
                }
            }
            else if (delta.y < 0) //向上滚动
            {
                if (IsReachBottom(parentScrollView))
                {
                    childScrollView.OnScroll(eventData);
                }
                else
                {
                    parentScrollView.OnScroll(eventData);
                }
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

namespace ZFrame.UGUI
{
    public class UIDragged : Selectable, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static UIDragged current;
        public static BaseEventData eventData;
                
        public bool cloneOnDrag = false;
        public GameObject target;
        
        public UnityAction onBeginDrag;
        public UnityAction onDragging;
        public UnityAction onEndDrag;

        private GameObject m_Dragging;
        public GameObject DraggingObject { get { return m_Dragging; } }
        private RectTransform m_DraggingPlane;
        
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
			if (!IsInteractable()) {
                eventData.pointerDrag = null;
                return;
            }

            var canvas = GetComponentInParent<Canvas>();
		    if (canvas == null) return;
            m_DraggingPlane = canvas.GetComponent<RectTransform>();

            if (cloneOnDrag) {
                if (target) {
                    m_Dragging = GameObject.Instantiate(target);
                    var grp = m_Dragging.AddComponent<CanvasGroup>();
                    grp.blocksRaycasts = false;
                }
            } else {
                m_Dragging = gameObject;
            }
            if (m_Dragging) {
                m_Dragging.transform.SetAsLastSibling();
                m_Dragging.transform.SetParent(canvas.transform, false);
            }

            SetDraggedPosition(eventData);

            current = this;
            UIDragged.eventData = eventData;
            if (onBeginDrag != null) onBeginDrag.Invoke();
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!IsInteractable()) return;
            
            SetDraggedPosition(eventData);
            
            current = this;
            UIDragged.eventData = eventData;
            if (onDragging != null) onDragging.Invoke();
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
			if (!IsInteractable()) return;

            current = this;
            UIDragged.eventData = eventData;
            if (onEndDrag != null) onEndDrag.Invoke();

            if (cloneOnDrag) {
                Destroy(m_Dragging);
            }
        }

        private void SetDraggedPosition(PointerEventData eventData)
        {
            if (m_Dragging) {
                var rt = m_Dragging.GetComponent<RectTransform>();
                Vector3 globalMousePos;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    m_DraggingPlane, eventData.position, eventData.pressEventCamera, out globalMousePos)) {
                    rt.position = globalMousePos;
                    rt.rotation = m_DraggingPlane.rotation;
                }
            }
        }
    }

}
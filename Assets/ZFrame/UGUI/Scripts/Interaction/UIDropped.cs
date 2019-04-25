using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace ZFrame.UGUI
{
    public class UIDropped : UIBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public static UIDropped current;
        public static BaseEventData eventData;

        public Selectable selectable;

        public UnityAction onPointerEnter, onPointerExit, onDrop;

		private bool m_IsHover;
		public bool isHover { get { return m_IsHover; } }

        public bool IsInteractable() { return !selectable || selectable.IsInteractable(); }

        public void OnDrop(PointerEventData data)
        {
            if (!IsInteractable()) return;

			m_IsHover = false;

            current = this;
            eventData = data;
            if (onDrop != null) onDrop.Invoke();
        }

        public void OnPointerEnter(PointerEventData data)
        {
            if (!IsInteractable()) return;

			m_IsHover = true;

            current = this;
            eventData = data;
            if (onPointerEnter != null) onPointerEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData data)
        {
            if (!IsInteractable()) return;

			m_IsHover = false;

            current = this;
            eventData = data;
            if (onPointerExit != null) onPointerExit.Invoke();
        }
    }
}

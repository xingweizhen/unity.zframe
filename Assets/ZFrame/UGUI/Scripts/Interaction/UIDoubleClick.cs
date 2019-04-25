using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZFrame.UGUI
{
    [RequireComponent(typeof(UIEventTrigger)), DisallowMultipleComponent]
    public class UIDoubleClick : UIBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private float m_Threshold = 0.3f;

        private float m_ClickTime = 0f;
        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_ClickTime > 0f && Time.unscaledTime - m_ClickTime < m_Threshold) {
                var tgr = GetComponent(typeof(UIEventTrigger)) as UIEventTrigger;
                if (tgr) tgr.Execute(TriggerType.DoubleClick, eventData);
                m_ClickTime = 0;
            }
            m_ClickTime = Time.unscaledTime;
        }
    }
}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
//using NoToLua = XLua.BlackListAttribute;

namespace ZFrame.UGUI
{
    public class UISliding : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public UnityAction<UISliding, Vector2> onSlide;

        private float m_BeginTime;
        private Vector2 m_BeginPoint;

        //[NoToLua]
        public void OnBeginDrag(PointerEventData eventData)
        {
            m_BeginTime = Time.realtimeSinceStartup;
            m_BeginPoint = eventData.position;    
        }

        //[NoToLua]
        public void OnEndDrag(PointerEventData eventData)
        {
            var velocity = (eventData.position - m_BeginPoint) / (Time.realtimeSinceStartup - m_BeginTime);
            if (onSlide != null) onSlide.Invoke(this, velocity);
        }

        //[NoToLua]
        public void OnDrag(PointerEventData eventData)
        {
            
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace ZFrame.UGUI
{
    [RequireComponent(typeof(UIEventTrigger))][DisallowMultipleComponent]
    public class UILongpress : UIBehaviour, ITickable, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler
    {
        [SerializeField] private float m_OverridedLongpressTime = 0f;

        [SerializeField] private float m_Interval = 0;

        private float m_Time = -1;
        private float m_Last = 0;

        private BaseEventData m_CurrentData;

        private void SendPressEvent(bool pressed)
        {
            var tgr = GetComponent(typeof(UIEventTrigger)) as UIEventTrigger;
            if (tgr) tgr.Execute(TriggerType.Longpress, pressed ? m_CurrentData : null);
        }
        
        private void DonePressing()
        {
            m_Time = -1;
            m_CurrentData = null;
            TickManager.Remove(this);
        }

        private void BreakPressing()
        {
            if (m_Time < 0) {
                SendPressEvent(false);
            }
            DonePressing();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_Time = 0;
            m_Last = 0;
            m_CurrentData = eventData;
            TickManager.Add(this);
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {  
            BreakPressing();
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {   
            BreakPressing();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            TickManager.Remove(this);
        }

        bool ITickBase.ignoreTimeScale { get { return true; } }
        
        void ITickable.Tick(float deltaTime)
        {
            if (m_Time < 0) return;

            float lasting = m_Time;
            m_Time += deltaTime;
            var longpressTime = m_OverridedLongpressTime > 0 ? 
                m_OverridedLongpressTime : UGUITools.settings.longpressTime;
            if (lasting < longpressTime) {
                // 第一次触发长按
                if (m_Time >= longpressTime) {
                    m_Last = longpressTime;
                    SendPressEvent(true);
                    // 是否支持反复触发
                    if (m_Interval == 0) DonePressing();
                }
            } else {
                // 反复触发
                float dura = m_Time - m_Last;
                int n = Mathf.FloorToInt(dura / m_Interval);
                if (n > 0) {
                    //m_Last += interval * n;
                    m_Last = m_Time;
                    for (int i = 0; i < n; ++i) SendPressEvent(true);
                }
            }
        }
    }
}

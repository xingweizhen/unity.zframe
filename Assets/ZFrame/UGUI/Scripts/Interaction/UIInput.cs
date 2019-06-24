using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
//using NoToLua = XLua.BlackListAttribute;

namespace ZFrame.UGUI
{
    public class UIInput : UnityEngine.UI.InputField, IEventSender
    {
        #region 事件通知
        [SerializeField]
        private EventData m_ValueChanged = new EventData(TriggerType.None, UIEvent.Send);
        [SerializeField]
        private EventData m_Submit = new EventData(TriggerType.Submit);

        IEnumerator<EventData> IEnumerable<EventData>.GetEnumerator()
        {
            yield return m_ValueChanged;
            yield return m_Submit;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return m_Submit;
        }

        private IEventTransfer __Wnd;
        protected IEventTransfer Wnd {
            get {
                if (__Wnd == null) {
                    __Wnd = GetComponentInParent(typeof(IEventTransfer)) as IEventTransfer;
                }
                return __Wnd;
            }
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            __Wnd = null;
        }

        #endregion
        
        //[NoToLua]
        public void SetEvent(TriggerType id, UIEvent eventName, string param)
        {
            if (id == TriggerType.Submit) {
                m_Submit.name = eventName;
                m_Submit.param = param;
            } else if (id == TriggerType.None) {
                m_ValueChanged.name = eventName;
                m_ValueChanged.param = param;
            }
        }
        
        private void OnValueChanged(string data)
        {
            m_ValueChanged.Send(this, Wnd, data);
        }

        private void OnSubmit(string data)
        {
            m_Submit.Send(this, Wnd, data);
        }

        protected override void Awake()
        {
            base.Awake();
            onEndEdit.AddListener(OnSubmit);
            onValueChanged.AddListener(OnValueChanged);
        }
    }
}

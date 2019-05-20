using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
//using NoToLua = XLua.BlackListAttribute;

namespace ZFrame.UGUI
{
    public class UIDropdown : Dropdown, IEventSender
    {
        #region 事件通知
        [SerializeField]
        private EventData m_Event = new EventData(TriggerType.PointerClick);

        IEnumerator<EventData> IEnumerable<EventData>.GetEnumerator()
        {
            yield return m_Event;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return m_Event;
        }

        private UIWindow __Wnd;
        protected UIWindow Wnd {
            get { 
                if (__Wnd == null) {
                    __Wnd = GetComponentInParent(typeof(UIWindow)) as UIWindow;
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
            if (id == TriggerType.PointerClick) {
                m_Event.name = eventName;
                m_Event.param = param;
            }
        }

        private void doValueChanged(int index)
        {
            m_Event.Send(this, Wnd, NoBoxingValue<int>.Apply(index));
        }

        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(doValueChanged);
        }
    }
}

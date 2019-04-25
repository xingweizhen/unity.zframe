//
//  UIInputField.cs
//  survive
//
//  Created by xingweizhen on 10/31/2017.
//
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZFrame.UGUI
{
#if USING_TMP
    public class UIInputField : TMPro.TMP_InputField, IEventSender
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
            
            onSubmit.AddListener(OnSubmit);
            onValueChanged.AddListener(OnValueChanged);
        }

    }
#endif
}

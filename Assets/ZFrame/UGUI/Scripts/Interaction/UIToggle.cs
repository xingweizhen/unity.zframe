using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
//using NoToLua = XLua.BlackListAttribute;

namespace ZFrame.UGUI
{
    using Tween;

    public class UIToggle : Toggle, IEventSender, IStateTransTarget
    {
        public static UnityAction<GameObject> onToggleClick;
        public static string defaultSfx;

        [AudioRef]
        public string clickSfx;

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

        protected void OnEventTrigger()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            m_Event.Send(this, Wnd, NoBoxingValue<bool>.Apply(value));
        }
        #endregion

        [SerializeField]
        private RectTransform m_CheckedTrans;
        [SerializeField]
        private RectTransform m_UncheckedTrans;

        public bool value {
            get { return isOn; }
            set { isOn = value; }
        }

        private bool m_Disable;
        public bool disabled {
            get { return m_Disable; }
            set {
                m_Disable = value;
                DoStateTransition(SelectionState.Normal, true);
            }
        }

        private bool m_PrevState;

        private void SetVisible(RectTransform target, bool visible)
        {
            if (target) {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif
                var cv = (CanvasGroup)target.gameObject.NeedComponent(typeof(CanvasGroup));
                if (toggleTransition == ToggleTransition.None) {
                    cv.alpha = visible ? 1 : 0;
                } else {
                    ZTween.Stop(target);
                    cv.blocksRaycasts = visible;
                    cv.interactable = visible;
                    cv.TweenAlpha(visible ? 1 : 0, 0.1f).SetUpdate(UpdateType.Normal, true);
                }
            }
        }

        private void DoValueChanged(bool currentState)
        {
            if (disabled) return;
            
            SetVisible(m_CheckedTrans, currentState);
            SetVisible(m_UncheckedTrans, !currentState);

            if (m_PrevState != currentState) {
                OnEventTrigger();
            }
            m_PrevState = currentState;
        }

        protected override void Awake()
        {
            base.Awake();

            m_PrevState = isOn;
            
            SetVisible(m_CheckedTrans, isOn);
            SetVisible(m_UncheckedTrans, !isOn);

            onValueChanged.AddListener(DoValueChanged);
        }

        //[NoToLua]
        public void SetEvent(TriggerType id, UIEvent eventName, string param)
        {
            if (id == TriggerType.PointerClick) {
                m_Event.name = eventName;
                m_Event.param = param;
            }
        }
        
        //[NoToLua]
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (IsActive() && IsInteractable()) {
                if (!disabled) {
                    base.OnPointerClick(eventData);
                } else {
                    OnEventTrigger();
                }

                if (onToggleClick != null) onToggleClick.Invoke(gameObject);

                // 点击音效
                UGUITools.PlayUISfx(clickSfx, defaultSfx);
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (disabled) state = SelectionState.Disabled;

            base.DoStateTransition(state, instant);
            this.TryStateTransition((SelectingState)state, instant);
        }

        public void SetInteractable(bool interactable)
        {
            this.interactable = interactable;
            UGUITools.SetGrayscale(gameObject, !interactable);
        }

        public void SetValueChanged(UnityAction<bool> action)
        {
            onValueChanged.RemoveAllListeners();
            onValueChanged.AddListener(DoValueChanged);
            onValueChanged.AddListener(action);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    public class UIEventTrigger : UISelectable, IEventSender, IStateTransTarget,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler,
        IScrollHandler,
        IUpdateSelectedHandler,
        ISelectHandler,
        IDeselectHandler,
        IMoveHandler,
        ISubmitHandler,
        ICancelHandler
    {
        [SerializeField]
        protected Graphic m_TragetGraphic;

        public Graphic targetGraphic { get { return m_TragetGraphic; } }
                
        [SerializeField]
        private bool m_FreeScrolling;

        [SerializeField, HideInInspector]
        protected List<EventData> m_Events = new List<EventData>();

        [AudioRef]
        public string clickSfx;

        private UIWindow __Wnd;
        protected UIWindow Wnd {
            get {
                if (__Wnd == null) {
                    __Wnd = GetComponentInParent(typeof(UIWindow)) as UIWindow;
                }
                return __Wnd;
            }
        }
        
        IEnumerator<EventData> IEnumerable<EventData>.GetEnumerator()
        {
            for (int i = 0; i < m_Events.Count; ++i) {
                yield return m_Events[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < m_Events.Count; ++i) {
                yield return m_Events[i];
            }
        }


        private bool m_EligibleForClick;

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            __Wnd = null;
        }

        private ScrollRect m_Scroll;
        private bool m_Scrolling;
        protected bool CheckScrolling(PointerEventData eventData)
        {
            if (m_Scroll != null && m_Scroll.content && m_Scroll.viewport) {
                if (m_FreeScrolling) return true;

                var delta = eventData.delta;
                float absX = Mathf.Abs(delta.x), absY = Mathf.Abs(delta.y);

                if (m_Scroll.vertical && absX < absY && m_Scroll.content.rect.height > m_Scroll.viewport.rect.height
                    || m_Scroll.horizontal && absX > absY && m_Scroll.content.rect.width > m_Scroll.viewport.rect.width) {
                    return true;
                }
            }

            return false;
        }

        protected override void Start()
        {
            base.Start();
            m_Scroll = GetComponentInParent(typeof(ScrollRect)) as ScrollRect;
            m_Scrolling = false;

            if (GetComponent(typeof(UIButton))) {
                clickSfx = AudioManager.DUMMY_SFX;
            }
        }

        public void SetEvent(TriggerType id, UIEvent eventName, string param)
        {
            for (int i = 0; i < m_Events.Count; ++i) {
                var Event = m_Events[i];
                if (Event.type == id) {
                    Event.name = eventName;
                    Event.param = param;
                    return;
                }
            }

            m_Events.Add(new EventData(id) {
                name = eventName, param = param,
            });
        }

        protected EventData FindEvent(TriggerType id)
        {
            for (int i = 0; i < m_Events.Count; ++i) {
                var evt = m_Events[i];
                if (evt.IsActive() && evt.type == id) {
                    return evt;
                }
            }

            return null;
        }

        public virtual bool Execute(TriggerType id, object data)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            var eventData = data as PointerEventData;
            if (eventData != null && eventData.pointerId != PointerInputModule.kMouseLeftId) return false;
#endif      
            var evt = FindEvent(id);
            if (evt != null) {
                evt.Send(this, Wnd, data);
                switch (id) {
                    case TriggerType.PointerDown:
                    case TriggerType.PointerClick:
                    case TriggerType.Select:
                        this.TryStateTransition(SelectingState.Pressed, true);
                        break;
                    case TriggerType.Deselect:
                    case TriggerType.PointerUp:
                    case TriggerType.Cancel:
                        this.TryStateTransition(SelectingState.Normal, true);
                        break;
                }
               
                return true;
            }

            return false;
        }
        
        private IEnumerator DelayExecute(EventData evt, BaseEventData data)
        {
            yield return Yields.EndOfFrame;
            evt.Send(this, Wnd, data);
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            Execute(TriggerType.PointerEnter, eventData);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            Execute(TriggerType.PointerExit, eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            if (m_Scrolling) {
                m_Scroll.OnDrag(eventData);
            } else {
                Execute(TriggerType.Drag, eventData);
            }
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            Execute(TriggerType.Drop, eventData);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            m_EligibleForClick = true;
            Execute(TriggerType.PointerDown, eventData);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            Execute(TriggerType.PointerUp, eventData);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            if (m_EligibleForClick) {
                Execute(TriggerType.PointerClick, eventData);

                // 点击音效
                UGUITools.PlayUISfx(clickSfx, UIButton.defaultSfx);
            }
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (!IsInteractable()) return;

            Execute(TriggerType.Select, eventData);
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            if (!IsInteractable()) return;

            Execute(TriggerType.Deselect, eventData);
            var evt = FindEvent(TriggerType.Unselect);
            if (evt != null) {
                StartCoroutine(DelayExecute(evt, eventData));
            }
        }

        public virtual void OnScroll(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            if (m_Scroll) m_Scroll.OnScroll(eventData);
            Execute(TriggerType.Scroll, eventData);
        }

        public virtual void OnMove(AxisEventData eventData)
        {
            if (!IsInteractable()) return;

            Execute(TriggerType.Move, eventData);
        }

        public virtual void OnUpdateSelected(BaseEventData eventData)
        {
            if (!IsInteractable()) return;

            Execute(TriggerType.UpdateSelected, eventData);
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            if (m_Scroll) m_Scroll.OnInitializePotentialDrag(eventData);
            Execute(TriggerType.InitializePotentialDrag, eventData);
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            m_EligibleForClick = false;
            
            m_Scrolling = CheckScrolling(eventData);
            if (m_Scrolling) {
                m_Scroll.OnBeginDrag(eventData);
            } else {
                Execute(TriggerType.BeginDrag, eventData);
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            if (m_Scrolling) {
                m_Scroll.OnEndDrag(eventData);
            } else {
                Execute(TriggerType.EndDrag, eventData);
            }
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            if (!IsInteractable()) return;

            Execute(TriggerType.Submit, eventData);
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            if (!IsInteractable()) return;

            Execute(TriggerType.Cancel, eventData);
        }
    }
}

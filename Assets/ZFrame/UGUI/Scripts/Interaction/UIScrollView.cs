using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
//using NoToLua = XLua.BlackListAttribute;

namespace ZFrame.UGUI
{
    using Tween;
    using UnityEngine.UI;

    public class UIScrollView : ScrollRect, ITweenable, ITweenable<float>, IEventSender
    {
        public const int TGR_OVERLAP = 100;

        #region 事件通知
        [SerializeField, HideInInspector]
        private EventData m_Event = new EventData(TriggerType.None, UIEvent.Send);
        [SerializeField, HideInInspector]
        private EventData m_BeginDrag = new EventData(TriggerType.BeginDrag, UIEvent.Send);
        [SerializeField, HideInInspector]
        private EventData m_Drag = new EventData(TriggerType.Drag, UIEvent.Send);
        [SerializeField, HideInInspector]
        private EventData m_EndDrag = new EventData(TriggerType.EndDrag, UIEvent.Send);
        [SerializeField, HideInInspector]
        private EventData m_ChildOverlap = new EventData((TriggerType)TGR_OVERLAP, UIEvent.Send);

        IEnumerator<EventData> IEnumerable<EventData>.GetEnumerator()
        {
            yield return m_Event;
            yield return m_BeginDrag;
            yield return m_Drag;
            yield return m_EndDrag;
            yield return m_ChildOverlap;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return m_Event;
            yield return m_BeginDrag;
            yield return m_Drag;
            yield return m_EndDrag;
            yield return m_ChildOverlap;
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

        protected void OnScrollValueChanged(Vector2 value)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (viewRect && m_ItemList.Count > 0 && m_ChildOverlap.IsActive()) {
                for (var i = 0; i < m_ItemList.Count; ++i) {
                    var trans = (RectTransform)m_ItemList[i].transform;
                    var overlap = viewRect.Overlabs(trans, uiCamera);
                    if (overlap != m_ItemList[i].overlap) {
                        m_ItemList[i].overlap = overlap;
                        m_ChildOverlap.Send(m_ItemList[i].transform, Wnd, NoBoxingValue<bool>.Apply(overlap));
                    }
                    RectTransformUtility.CalculateRelativeRectTransformBounds(viewRect, trans);
                }
            }
            m_Event.Send(this, Wnd, NoBoxingValue<Vector2>.Apply(value));
        }

        #endregion

        //[NoToLua]
        public void SetEvent(TriggerType id, UIEvent eventName, string param)
        {
            EventData evtData = null;
            switch (id) {
                case TriggerType.None:
                    evtData = m_Event;
                    break;
                case TriggerType.BeginDrag:
                    evtData = m_BeginDrag;
                    break;
                case TriggerType.Drag:
                    evtData = m_Drag;
                    break;
                case TriggerType.EndDrag:
                    evtData = m_EndDrag;
                    break;
            }

            if (evtData != null) {
                evtData.name = eventName;
                evtData.param = param;
            }
        }

        private Camera m_Camera;
        public Camera uiCamera {
            get {
                if (m_Camera == null) {
                    m_Camera = gameObject.FindCameraForLayer();
                }
                return m_Camera;
            }
        }

        private List<UIScrollItem> m_ItemList = new List<UIScrollItem>();

        private void UpdateItemOverlap(UIScrollItem item)
        {
            if (viewRect) {
                var overlap = viewRect.Overlabs((RectTransform)item.transform, uiCamera);
                if (overlap != item.overlap) {
                    item.overlap = overlap;
                    m_ChildOverlap.Send(item.transform, Wnd, NoBoxingValue<bool>.Apply(overlap));
                }
            }
        }

        public void AddItem(UIScrollItem item)
        {
            m_ItemList.Add(item);
            item.overlap = false;
            UpdateItemOverlap(item);
        }

        public void RemoveItem(UIScrollItem item)
        {
            m_ItemList.Remove(item);            
        }
        
        protected override void Awake()
        {
            base.Awake();

            onValueChanged.AddListener(OnScrollValueChanged);
        }

        public override void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            m_BeginDrag.Send(this, Wnd);
        }

        public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            base.OnDrag(eventData);
            m_Drag.Send(this, Wnd);
        }

        public override void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            m_EndDrag.Send(this, Wnd);
        }

        protected override void SetContentAnchoredPosition(Vector2 position)
        {
            var offset = content.anchoredPosition - position;
            if (vertical && Mathf.Abs(offset.y) > 1e-3 || horizontal && Mathf.Abs(offset.x) > 1e-3) {
                base.SetContentAnchoredPosition(position);
            }
        }

        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            m_Camera = null;
        }

        private void SetNormalizedPosition(Vector2 pos)
        {
            normalizedPosition = pos;
        }

        private Vector2 GetNormalizedPosition()
        {
            return normalizedPosition;
        }
        
        public ZTweener Tween(object from, object to, float duration)
        {
            ZTweener tw = null;
            if (to is Vector2) {
                tw = this.Tween(GetNormalizedPosition, SetNormalizedPosition, (Vector2)to, duration);
                if (from is Vector2) {
                    tw.StartFrom((Vector2)from);
                }
            }
            if (tw != null) tw.SetTag(this);
            return tw;
        }

        public ZTweener Tween(float to, float duration)
        {
            ZTweener tw = null;

            if (vertical) {
                var v2To = new Vector2(0, to);
                tw = this.Tween(GetNormalizedPosition, SetNormalizedPosition, v2To, duration);
            } else if (horizontal) {
                var v2To = new Vector2(to, 0);
                tw = this.Tween(GetNormalizedPosition, SetNormalizedPosition, v2To, duration);
            }

            if (tw != null) tw.SetTag(this);
            return tw;
        }

        public ZTweener Tween(float from, float to, float duration)
        {
            if (vertical) {
                verticalNormalizedPosition = from;
            } else if (horizontal) {
                horizontalNormalizedPosition = from;
            }

            return Tween(to, duration);
        }
    }
}

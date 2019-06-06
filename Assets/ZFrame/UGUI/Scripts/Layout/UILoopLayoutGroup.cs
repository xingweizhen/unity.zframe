using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.UGUI
{
    public abstract class UILoopLayoutGroup : HorizontalOrVerticalLayoutGroup, IEventSender, ILoopLayout
    {
        #region 事件通知
        [SerializeField]
        private EventData m_Event = new EventData(TriggerType.None, UIEvent.Send);

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

        protected void OnItemUpdate(GameObject go, int index)
        {
            if (onItemUpdate != null) onItemUpdate.Invoke(go, index);
            m_Event.Send(go.transform, Wnd, NoBoxingValue<int>.Apply(index));
        }

        public event System.Action<GameObject, int> onItemUpdate;

        //[NoToLua]
        public void SetEvent(TriggerType id, UIEvent eventName, string param)
        {
            if (id == TriggerType.None) {
                m_Event.name = eventName;
                m_Event.param = param;
            }
        }
        #endregion

        private UIGroup m_Group;

        public UIGroup group {
            get {
                if (m_Group == null) {
                    m_Group = gameObject.NeedComponent(typeof(UIGroup)) as UIGroup;
                }

                return m_Group;
            }
        }

        [SerializeField]
        protected RectOffset m_RawPading;
        public RectOffset rawPading { get { return m_RawPading; } }

        [SerializeField]
        protected GameObject m_Template;

        [SerializeField]
        protected float m_MinSize = 30f;

        public int startIndex { get; protected set; }
        /// <summary>
        /// 有效列表中第一个元素的结束位置
        /// </summary>
        public float firstPos { get; protected set; }
        /// <summary>
        /// 有效列表中最后一个元素的开始位置
        /// </summary>
        public float lastPos { get; protected set; }

        protected ScrollRect m_Scroll;

        protected bool m_Inited;
        protected int m_TotalItem;
        public int totalItem {
            get { return m_TotalItem; }
            protected set { m_TotalItem = value; }
        }
        protected int m_ViewItem;

        protected List<RectTransform> m_Items = new List<RectTransform>();
        
        protected override void OnEnable()
        {
            base.OnEnable();
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            m_Scroll = GetComponentInParent(typeof(ScrollRect)) as ScrollRect;
            if (m_Scroll == null) {
                LogMgr.W(this, "没有找到<ScrollRect>，建议使用普通的布局脚本。");
            } else {
                m_Scroll.onValueChanged.AddListener(OnScrollValueChanged);
                var pivot = rectTransform.pivot;
                if (m_Scroll.horizontal) {
                    pivot.x = 0;
                    if (m_Scroll.content.pivot.x != 0f) {
                        LogMgr.W(this, "{0} requires horizontal scroll's content has a pivot.x == 0", GetType().Name);
                    }
                }
                if (m_Scroll.vertical) {
                    pivot.y = 1;
                    if (m_Scroll.content.pivot.y != 1f) {
                        LogMgr.W(this, "{0} requires velocity scroll's content has a pivot.y == 1", GetType().Name);
                    }
                }
                rectTransform.pivot = pivot;

                m_Template.SetActive(false);
                m_ViewItem = Mathf.CeilToInt(GetViewLength() / m_MinSize);
                for (var i = m_Items.Count; i < m_ViewItem; ++i) {
                    var item = GoTools.NewChild(gameObject, m_Template);
                    m_Items.Add((RectTransform)item.GetComponent(typeof(RectTransform)));
                }
            }

            m_TotalItem = 0;
            m_Inited = false;
            startIndex = 0;
            firstPos = 0;
            lastPos = 0;
            m_Padding.left = m_RawPading.left;
            m_Padding.right = m_RawPading.right;
            m_Padding.top = m_RawPading.top;
            m_Padding.bottom = m_RawPading.bottom;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_Scroll) {
                m_Scroll.onValueChanged.RemoveListener(OnScrollValueChanged);
            }
        }

        protected Vector2 GetViewSize()
        {
            if (m_Scroll) {
                var viewRect = m_Scroll.viewport ? m_Scroll.viewport : m_Scroll.transform as RectTransform;
                return viewRect.rect.size;
            }

            return Vector2.zero;
        }

        protected abstract float GetScrollValue();
        protected abstract float GetViewLength();
        protected abstract void UpdateFirstAndLastPos();

        protected virtual void OnScrollValueChanged(Vector2 value)
        {

        }

        public void SetTotalItem(int count, bool forceUpdate)
        {
            m_TotalItem = count;
            if (forceUpdate) {
                for (var i = 0; i < Mathf.Min(count, m_Items.Count); ++i) {
                    m_Items[i].gameObject.SetActive(true);
                    OnItemUpdate(m_Items[i].gameObject, i);
                }
                for (var i = count; i < m_Items.Count; ++i) m_Items[i].gameObject.SetActive(false);
                //if (count > m_Items.Count) {
                //    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                //    UpdateFirstAndLastPos();                    
                //}
            }
        }
    }
}

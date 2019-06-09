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
        protected RectOffset m_RawPadding;
        public RectOffset rawPadding { get { return m_RawPadding; } }

        [SerializeField] protected GameObject m_Template;

        [SerializeField] protected float m_MinSize = 30f;

        [SerializeField] protected bool m_Revert = false;

        public int startIndex { get; protected set; }
        /// <summary>
        /// 有效列表中第一个元素的结束位置
        /// </summary>
        public float firstPos { get; protected set; }
        /// <summary>
        /// 有效列表中最后一个元素的开始位置
        /// </summary>
        public float lastPos { get; protected set; }
        
        public float viewport { get; protected set; }

        protected float m_RemainPadding;
        protected float m_HeadPadding, m_TailPadding;
        
        protected ScrollRect m_Scroll;

        protected bool m_Inited;
        protected int m_TotalItem;
        public int totalItem {
            get { return m_TotalItem; }
            protected set { m_TotalItem = value; }
        }
        protected int m_ViewItem;

        protected ObjPool<GameObject> m_ItemPool;
        protected List<RectTransform> m_Items = new List<RectTransform>();

        protected  override void Awake()
        {
            base.Awake();
            
            m_ItemPool = new ObjPool<GameObject>(m_Template,
                go => {
                    group.Add(go);
                    go.Attach(transform, false);
                }, go => {
                    group.Remove(go);
                    go.Attach(m_Template.transform);
                });
        }

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
                viewport = GetViewLength();
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
                UpdateItems();
            }

            m_TotalItem = 0;
            m_Inited = false;
            startIndex = m_Revert ? -1 : 0;
            
            m_Padding.left = m_RawPadding.left;
            m_Padding.right = m_RawPadding.right;
            m_Padding.top = m_RawPadding.top;
            m_Padding.bottom = m_RawPadding.bottom;
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

        protected abstract float GetItemSize(RectTransform item);
        protected abstract void AddHeadPadding(float value);
        protected abstract void AddTailPadding(float value);
        protected abstract void UpdatePadding(float head, float tail);
        protected abstract float GetScrollValue();
        protected abstract float GetViewLength();

        protected void RequireItemView(int index)
        {
            RectTransform item = null;
            if (index == startIndex + m_Items.Count) {
                item = m_Items[0];
                item.SetAsLastSibling();
                m_Items.RemoveAt(0);
                m_Items.Add(item);
                AddHeadPadding(GetItemSize(item));

                OnItemUpdate(item.gameObject, index);
                LayoutRebuilder.ForceRebuildLayoutImmediate(item);
                AddTailPadding(-GetItemSize(item));
            } else if (index < startIndex) {
                var lastIndex = m_Items.Count - 1;
                item = m_Items[lastIndex];
                item.SetAsFirstSibling();
                m_Items.RemoveAt(lastIndex);
                m_Items.Insert(0, item);
                AddTailPadding(GetItemSize(item));

                OnItemUpdate(item.gameObject, index);
                LayoutRebuilder.ForceRebuildLayoutImmediate(item);
                AddHeadPadding(-GetItemSize(item));
            }
        }
        
        protected virtual void OnScrollValueChanged(Vector2 value)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            var newViewport = GetViewLength();
            if (!Mathf.Approximately(viewport, newViewport)) {
                viewport = newViewport;
                var nItem = m_Items.Count;
                m_ViewItem = Mathf.CeilToInt(viewport / m_MinSize);
                UpdateItems();
                for (var i = nItem; i < m_ViewItem; ++i) {
                    m_Items[i].gameObject.SetActive(true);
                    OnItemUpdate(m_Items[i].gameObject, startIndex + i);
                }
                
            }
            float scrollValue = GetScrollValue();

            var changed = false;
            if (scrollValue < firstPos) {
                var pos = firstPos;
                while (scrollValue < pos && startIndex > 0) {
                    pos -= GetItemSize(m_Items[0]);
                    RequireItemView(startIndex - 1);
                    startIndex -= 1;
                    changed = true;
                }
            } else {
                var endValue = scrollValue + newViewport;
                var pos = lastPos;
                while (endValue > pos && startIndex + m_Items.Count < m_TotalItem) {
                    pos += GetItemSize(m_Items[m_Items.Count - 1]);
                    RequireItemView(startIndex + m_Items.Count);
                    startIndex += 1;
                    changed = true;
                }
            }

            if (changed) {
                if (m_RemainPadding > 0) {
                    UpdateRemainPadding(m_TotalItem);
                } else {
                    UpdatePadding(0, 0);
                }
            }
        }

        protected void UpdateItems()
        {
            for (var i = m_Items.Count; i < m_ViewItem; ++i) {
                var item = m_ItemPool.Get();
                m_Items.Add((RectTransform)item.GetComponent(typeof(RectTransform)));
            }

            for (var i = m_Items.Count - 1; i >= m_ViewItem; --i) {
                m_ItemPool.Release(m_Items[i].gameObject);
                m_Items.RemoveAt(i);
            }
        }

        protected void UpdateRemainPadding(int total)
        {
            var remainItem = m_Revert ? startIndex : total - (startIndex + m_Items.Count);
            if (remainItem > 0) {
                m_RemainPadding = remainItem * m_MinSize;
            } else {
                m_RemainPadding = 0;
            }
            
            if (m_Revert) {
                UpdatePadding(m_RemainPadding, 0);
            } else {
                UpdatePadding(0, m_RemainPadding);
            }
        }

        public void SetTotalItem(int count, bool forceUpdate)
        {
            if (startIndex < 0) {
                startIndex = Mathf.Max(0, count - m_Items.Count);
            }

            if (count < m_TotalItem) {
                if (startIndex + m_Items.Count >= count) {
                    startIndex = Mathf.Max(0, count - m_Items.Count);
                    forceUpdate = true;
                }
            }

            if (m_TotalItem != count) {
                UpdateRemainPadding(count);
            }
            m_TotalItem = count;
            if (forceUpdate) {
                for (var i = 0; i < Mathf.Min(m_TotalItem, m_Items.Count); ++i) {
                    m_Items[i].gameObject.SetActive(true);
                    OnItemUpdate(m_Items[i].gameObject, startIndex + i);
                }
                for (var i = m_TotalItem; i < m_Items.Count; ++i) m_Items[i].gameObject.SetActive(false);
            }
        }
    }
}

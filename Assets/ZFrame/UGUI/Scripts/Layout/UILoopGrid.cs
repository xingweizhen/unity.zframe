using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using NoToLua = XLua.BlackListAttribute;

namespace ZFrame.UGUI
{
    public interface ILoopLayout
    {
        event System.Action<GameObject, int> onItemUpdate;

        Transform transform { get; }
        UIGroup group { get; }

        void SetTotalItem(int count, bool forceUpdate);
        void ResetLayout();
    }

    public class UILoopGrid : GridLayoutGroup, IEventSender, ILoopLayout
    {
        [SerializeField]
        private RectOffset m_RawPadding;
        public RectOffset rawPadding { get { return m_RawPadding; } }

        [SerializeField]
        private GameObject m_Template;

        [SerializeField]
        private bool m_AutoStretch = false;

        private UIGroup m_Group;

        public UIGroup group {
            get {
                if (m_Group == null) {
                    m_Group = gameObject.NeedComponent(typeof(UIGroup)) as UIGroup;
                }

                return m_Group;
            }
        }
        private Vector2 curViewSize = Vector2.zero;
        protected int m_LimitLine = -1;
        public int limitLine { get { return m_LimitLine; } }

        public void UpdateLimitLine(Vector2 viewSize)
        {
            if (m_Scroll.horizontal) {
                m_LimitLine = Mathf.CeilToInt(viewSize.x / (cellSize.x + spacing.x));
            } else if (m_Scroll.vertical) {
                m_LimitLine = Mathf.CeilToInt(viewSize.y / (cellSize.y + spacing.y));
            }
        }

        /// <summary>
        /// 开始显示排序号
        /// </summary>
        public int startLine { get; private set; }

        /// <summary>
        /// 最小排序号，因为0和1是一样的显示，所以最小为1
        /// </summary>
        public const int minLine = 1;

        /// <summary>
        /// 最大排序号
        /// </summary>
        public int maxLine { 
            get {
                return Mathf.Max(0, Mathf.CeilToInt((float)totalItem / constraintCount) - limitLine - 1);
            }
        }

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

        private IEventTransfer __Wnd;
        protected IEventTransfer Wnd {
            get {
                if (__Wnd == null) {
                    __Wnd = GetComponentInParent(typeof(IEventTransfer)) as IEventTransfer;
                }
                return __Wnd;
            }
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

        public int rows { get; protected set; }
        public int cols { get; protected set; }

        protected bool m_Inited;
        protected int m_TotalItem;
        public int totalItem {
            get { return m_TotalItem; }
            protected set { m_TotalItem = value; }
        }

        public void SetTotalItem(int count, bool forceUpdate)
        {
            if (!forceUpdate) forceUpdate = m_TotalItem != count;
            m_TotalItem = count;

            if (forceUpdate) {
                m_Inited = false;
                m_ValueDirty = true;
                curViewSize = GetViewSize();
                UpdateLimitLine(curViewSize);
                startLine = Mathf.Clamp(startLine, 0, maxLine);
                UpdateLayout();
                UpdateItems();
                UpdateView(startLine);
            }
        }
        
        void ILoopLayout.ResetLayout() { }

        protected bool m_ValueDirty;
        protected float m_InValue;
        protected float m_OutValue;
        protected float m_StepValue;

        protected ObjPool<GameObject> m_ItemPool;
        protected List<GameObject> m_Items = new List<GameObject>();

        private ScrollRect __Scroll;
        protected ScrollRect m_Scroll {
            get {
                if (__Scroll == null) {
                    __Scroll = GetComponentInParent<ScrollRect>();
                    if (__Scroll) {
                        __Scroll.onValueChanged.AddListener(OnScrollValueChanged);

                        var pivot = rectTransform.pivot;
                        if (__Scroll.horizontal) {
                            pivot.x = 0;
                            if (__Scroll.content.pivot.x != 0f) {
                                LogMgr.W(this, "{0} requires horizontal scroll's content has a pivot.x == 0", GetType().Name);
                            }
                        }

                        if (__Scroll.vertical) {
                            pivot.y = 1;
                            if (__Scroll.content.pivot.y != 1f) {
                                LogMgr.W(this, "{0} requires velocity scroll's content has a pivot.y == 1", GetType().Name);
                            }
                        }

                        rectTransform.pivot = pivot;
                    }
                }
                return __Scroll;
            }
            set {
                if (__Scroll != value) {
                    if (__Scroll) __Scroll.onValueChanged.RemoveListener(OnScrollValueChanged);
                    __Scroll = value;
                }
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

        protected override void Awake()
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

#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
                m_Template.SetActive(false);

            m_ValueDirty = true;
            startLine = 0;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_Scroll = null;
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            __Wnd = null;
            m_Scroll = null;
        }

        protected override void OnRectTransformDimensionsChange()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (m_AutoStretch) {
                var cell = cellSize;
                if (constraint == Constraint.FixedColumnCount) {
                    cell.x = (rectTransform.rect.width - padding.left - padding.right - (constraintCount - 1) * spacing.x) / constraintCount;
                } else if (constraint == Constraint.FixedRowCount) {
                    cell.y = (rectTransform.rect.height - padding.top - padding.bottom - (constraintCount - 1) * spacing.y) / constraintCount;
                }
                cellSize = cell;
            }
            base.OnRectTransformDimensionsChange();
        }

        protected void UpateValues()
        {
            if (m_ValueDirty) {
                var allBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(m_Scroll.content); 
                var myBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(m_Scroll.content, rectTransform);
                var maxOffset = myBounds.max - allBounds.max;
                var minOffset = myBounds.min - allBounds.max;
                var viewSize = GetViewSize();
                if (m_Scroll.horizontal) {
                    m_InValue = -(viewSize.x + minOffset.x + m_RawPadding.left);
                    m_OutValue = -(viewSize.x + maxOffset.x + m_RawPadding.left);
                    m_StepValue = cellSize.x + spacing.x;
                } else if (m_Scroll.vertical) {
                    m_InValue = -viewSize.y - maxOffset.y + m_RawPadding.top;
                    m_OutValue = -viewSize.y - minOffset.y + m_RawPadding.top;
                    m_StepValue = cellSize.y + spacing.y;
                }

                m_ValueDirty = false;
                LogMgr.I("IN = {0}, OUT = {1}, STEP = {2}", m_InValue, m_OutValue, m_StepValue);
            }
        }

        protected void OnScrollValueChanged(Vector2 value)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            UpateValues();

            float scrollValue;
            var anchoredPos = m_Scroll.content.anchoredPosition;
            var anchoredOff = Vector2.zero;
            if (rectTransform != m_Scroll.content)
                anchoredOff = rectTransform.anchoredPosition;

            if (m_Scroll.horizontal) {
                scrollValue = -anchoredPos.x - m_RawPadding.left - anchoredOff.x;
            } else if (m_Scroll.vertical) {
                scrollValue = anchoredPos.y - m_RawPadding.top + anchoredOff.y;
            } else return;

            var viewSize = GetViewSize();
            var viewSizeChanged = curViewSize != viewSize;
            if (viewSizeChanged) {
                curViewSize = viewSize;
                UpdateLimitLine(curViewSize);
                UpdateItems();
            }
            
            var start = Mathf.Clamp(Mathf.FloorToInt(scrollValue / m_StepValue), 0, maxLine);
            if (start != startLine || viewSizeChanged) {
                var prevLine = startLine;
                startLine = start;
                UpdateLayout();
                UpdateView(prevLine);

                LogMgr.I("UPDATE: #{0}->{1}(value={2}|{3}@{4})", 
                    prevLine, startLine, scrollValue, m_StepValue, m_Scroll.content.anchoredPosition);
            }
        }

        protected void UpdateItems()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            
            var nViewItem = (limitLine + 2) * constraintCount;
            for (int i = m_Items.Count; i < nViewItem; ++i) {
                var item = m_ItemPool.Get();
                m_Items.Add(item);
            }

            for (int i = m_Items.Count - 1; i >= nViewItem; --i) {
                m_ItemPool.Release(m_Items[i]);
                m_Items.RemoveAt(i);
            }

            for (int i = 0; i < m_Items.Count; ++i) {
                var visible = i < totalItem;
                // 改变父节点，以触发布局渲染（否则可能出现渲染与位置错位问题）
                m_Items[i].transform.SetParent(visible ? transform : m_Template.transform);
                m_Items[i].SetActive(visible);
            }

        }

        protected void UpdateHorizontalPadding()
        {
            var step = Mathf.RoundToInt(cellSize.x + spacing.x);
            m_Padding.top = m_RawPadding.top;
            m_Padding.bottom = m_RawPadding.bottom;
            var nFront = Mathf.Max(0, startLine - 1);
            m_Padding.left = m_RawPadding.left + nFront * step;

            var endContraint = cols - limitLine - 2 - nFront;
            m_Padding.right = m_RawPadding.right + Mathf.Max(0, endContraint) * step;
        }

        protected void UpdateVerticalPadding()
        {
            var step = Mathf.RoundToInt(cellSize.y + spacing.y);
            m_Padding.left = m_RawPadding.left;
            m_Padding.right = m_RawPadding.right;
            var nFront = Mathf.Max(0, startLine - 1);
            m_Padding.top = m_RawPadding.top + nFront * step;

            var endContraint = rows - limitLine - 2 - nFront;
            m_Padding.bottom = m_RawPadding.bottom + Mathf.Max(0, endContraint) * step;
        }

        protected void UpdateLayout()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            switch (constraint) {
                case Constraint.Flexible: {
                        var size = rectTransform.rect.size;
                        if (startAxis == Axis.Horizontal) {
                            var width = size.x - m_Padding.horizontal + spacing.x;
                            var cellW = cellSize.x + spacing.x;
                            cols = Mathf.FloorToInt(width / cellW);
                            rows = Mathf.CeilToInt(m_TotalItem / (float)cols);
                            constraintCount = cols;
                            UpdateVerticalPadding();
                        } else {
                            var height = size.y - m_Padding.vertical + spacing.y;
                            var cellH = cellSize.y + spacing.y;
                            rows = Mathf.FloorToInt(height / cellH);
                            cols = Mathf.CeilToInt(m_TotalItem / (float)rows);
                            constraintCount = rows;
                            UpdateHorizontalPadding();
                        }
                    } break;
                case Constraint.FixedColumnCount: {
                        cols = constraintCount;
                        rows = Mathf.CeilToInt(m_TotalItem / (float)cols);
                        UpdateVerticalPadding();
                    } break;
                case Constraint.FixedRowCount: {
                        rows = constraintCount;
                        cols = Mathf.CeilToInt(m_TotalItem / (float)rows);
                        UpdateHorizontalPadding();
                    } break;
                default : return;
            }
        }

        protected void UpdateView(int prevLine)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            int startIndex = 0;
            int updateCount = 0;
            var itemIndexStart = Mathf.Max(0, startLine - 1) * constraintCount;
            if (m_Inited) {
                var nViewItem = m_Items.Count;
                var startCount = itemIndexStart - Mathf.Max(0, prevLine - 1) * constraintCount;
                if (startCount > 0 && startCount <= nViewItem) {
                    if (prevLine < maxLine) {
                        // 前一个显示的排列号小于最大排列号才需要调整位置。
                        for (int i = 0; i < startCount; ++i) 
                            m_Items[i].transform.SetAsLastSibling();
                        for (int i = 0; i < startCount; ++i) {
                            var item = m_Items[0];
                            m_Items.RemoveAt(0);
                            m_Items.Add(item);
                        }
                        startIndex = nViewItem - startCount;
                        updateCount = startCount;
                    }
                } else if (startCount < 0 && startCount >= -nViewItem) {
                    if (prevLine > minLine) {
                        // 前一个显示的排列号大于最小排列号才需要调整位置。
                        var lastIndex = nViewItem - 1;
                        for (int i = lastIndex; i >= nViewItem + startCount; --i) 
                            m_Items[i].transform.SetAsFirstSibling();
                        for (int i = lastIndex; i >= nViewItem + startCount; --i) {
                            var item = m_Items[lastIndex];
                            m_Items.RemoveAt(lastIndex);
                            m_Items.Insert(0, item);
                        }
                        updateCount = -startCount;
                    }
                } else {
                    updateCount = nViewItem;
                }
            } else {
                m_Inited = true;
                updateCount = m_Items.Count;
            }

            for (int i = startIndex; i < startIndex + updateCount; ++i) {
                var itemIndex = itemIndexStart + i;
                var cvGrp = m_Items[i].NeedComponent(typeof(CanvasGroup)) as CanvasGroup;
                if (itemIndex < totalItem) {
                    cvGrp.alpha = 1;
                    OnItemUpdate(m_Items[i], itemIndex);
                    //LogMgr.I("Show {0} View @ #{1}", itemIndex, m_Items[i].name);
                } else {
                    cvGrp.alpha = 0;
                }
            }

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.UGUI
{
    public class UILoopVertical : UILoopLayoutGroup
    {
        protected override float GetViewLength()
        {
            return GetViewSize().y;
        }

        protected override float GetScrollValue()
        {
            var anchoredPos = m_Scroll.content.anchoredPosition;
            var anchoredOff = Vector2.zero;
            if (rectTransform != m_Scroll.content)
                anchoredOff = rectTransform.anchoredPosition;

            return anchoredPos.y - anchoredOff.y;
        }

        protected void RequireItemView(int index)
        {
            RectTransform item = null;
            if (index == startIndex + m_Items.Count) {
                item = m_Items[0];
                item.SetAsLastSibling();
                m_Items.RemoveAt(0);
                m_Items.Add(item);
                m_Padding.top += Mathf.RoundToInt(LayoutUtility.GetPreferredHeight(item) + m_Spacing);

                OnItemUpdate(item.gameObject, index);
                LayoutRebuilder.ForceRebuildLayoutImmediate(item);
                if (m_Padding.bottom > m_RawPading.bottom) {                    
                    m_Padding.bottom -= Mathf.RoundToInt(LayoutUtility.GetPreferredHeight(item) + m_Spacing);
                }
            } else if (index < startIndex) {
                var lastIndex = m_Items.Count - 1;
                item = m_Items[lastIndex];
                item.SetAsFirstSibling();
                m_Items.RemoveAt(lastIndex);
                m_Items.Insert(0, item);
                m_Padding.bottom += Mathf.RoundToInt(LayoutUtility.GetPreferredHeight(item) + m_Spacing);

                OnItemUpdate(item.gameObject, index);
                LayoutRebuilder.ForceRebuildLayoutImmediate(item);
                m_Padding.top -= Mathf.RoundToInt(LayoutUtility.GetPreferredHeight(item) + m_Spacing);
            }
        }

        protected override void OnScrollValueChanged(Vector2 value)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            float scrollValue = GetScrollValue();

            if (scrollValue < firstPos) {
                var pos = firstPos;
                while (scrollValue < pos && startIndex > 0) {
                    pos -= LayoutUtility.GetPreferredHeight(m_Items[0]) + m_Spacing;
                    RequireItemView(startIndex - 1);
                    startIndex -= 1;
                }
            } else {
                var endValue = scrollValue + GetViewLength();
                var pos = lastPos;
                while (endValue > pos && startIndex + m_Items.Count < m_TotalItem) {
                    pos += LayoutUtility.GetPreferredHeight(m_Items[m_Items.Count - 1]) + m_Spacing;
                    RequireItemView(startIndex + m_Items.Count);
                    startIndex += 1;
                }
            }
        }

        protected override void UpdateFirstAndLastPos()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            var firstItem = m_Items[0];
            firstPos = -firstItem.anchoredPosition.y - firstItem.rect.yMin;

            var lastItem = m_Items[m_Items.Count - 1];
            lastPos = -lastItem.anchoredPosition.y - lastItem.rect.yMax;
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, true);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, true);
        }

        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, true);
        }

        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, true);
            UpdateFirstAndLastPos();
        }
    }
}

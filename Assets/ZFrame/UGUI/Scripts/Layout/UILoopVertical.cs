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

            return anchoredPos.y + anchoredOff.y;
        }
        protected override float GetItemSize(RectTransform item)
        {
            return LayoutUtility.GetPreferredHeight(item) + m_Spacing;
        }
        
        protected override void AddHeadPadding(float value)
        {
            if (value > 0 || m_HeadPadding > m_RawPadding.top) {
                m_HeadPadding += value;
            }
        }

        protected override void AddTailPadding(float value)
        {
            if (value > 0 || m_TailPadding > m_RawPadding.bottom) {
                m_TailPadding += value;
            }
        }

        protected override void UpdatePadding(float head, float tail)
        {
            m_Padding.top = Mathf.RoundToInt(m_HeadPadding + head);
            m_Padding.bottom = Mathf.RoundToInt(m_TailPadding + tail);
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
            
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (!m_Inited) {
                m_Inited = true;
                if (m_Revert) {
                    m_Scroll.verticalNormalizedPosition = 0;
                }
            }

            var firstItem = m_Items[0];
            firstPos = -firstItem.anchoredPosition.y - firstItem.rect.yMin;

            var lastItem = m_Items[m_Items.Count - 1];
            lastPos = -lastItem.anchoredPosition.y - lastItem.rect.yMax;
        }
    }
}

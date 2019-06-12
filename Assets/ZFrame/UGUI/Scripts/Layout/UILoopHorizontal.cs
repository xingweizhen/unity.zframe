using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.UGUI
{
	public class UILoopHorizontal : UILoopLayoutGroup
	{
		protected override float GetViewLength()
		{
			return GetViewSize().x;
		}

        protected override float GetScrollValue()
        {
            var anchoredPos = m_Scroll.content.anchoredPosition;
            var anchoredOff = Vector2.zero;
            if (rectTransform != m_Scroll.content)
                anchoredOff = rectTransform.anchoredPosition;

            var value = -anchoredPos.x - anchoredOff.x;
            if (m_Revert) {
                value += m_Scroll.content.rect.width + rectTransform.rect.width;
            }
            return value;
        }

		protected override float GetItemSize(RectTransform item)
		{
			return LayoutUtility.GetPreferredWidth(item) + m_Spacing;
		}

		protected override void AddHeadPadding(float value)
		{
			if (value > 0 || m_HeadPadding > m_RawPadding.left) {
				m_HeadPadding += value;
			}
		}

		protected override void AddTailPadding(float value)
		{
			if (value > 0 || m_TailPadding > m_RawPadding.right) {
				m_TailPadding += value;
			}
		}
		
		protected override void UpdatePadding(float head, float tail)
		{
			m_Padding.left = Mathf.RoundToInt(m_HeadPadding + head);
			m_Padding.right = Mathf.RoundToInt(m_TailPadding + tail);
		}
		
		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();
			CalcAlongAxis(0, false);
		}

		public override void CalculateLayoutInputVertical()
		{
			CalcAlongAxis(1, false);
		}

		public override void SetLayoutHorizontal()
		{
			SetChildrenAlongAxis(0, false);
			
#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif
			if (!m_Inited) {
				m_Inited = true;
				if (m_Revert) {
					m_Scroll.horizontalNormalizedPosition = 1;
				}
			}
			
			var firstItem = m_Items[0];
			firstPos = firstItem.anchoredPosition.x + firstItem.rect.xMax;

			var lastItem = m_Items[m_Items.Count - 1];
			lastPos = lastItem.anchoredPosition.x + lastItem.rect.xMin;
		}
		
		public override void SetLayoutVertical()
		{
			SetChildrenAlongAxis(1, false);
		}
	}
}

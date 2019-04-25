using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace ZFrame.UGUI
{
	[ExecuteInEditMode]
	public class SiblingLayout : UIBehaviour, ILayoutElement {
		public RectTransform target;
		public RectOffset pading;

		private RectTransform m_Rect;
		public RectTransform rectTransform { get { if (!m_Rect) m_Rect = GetComponent<RectTransform>(); return m_Rect; } }

		public bool ignoreLayout { get { return false; } }
		public float flexibleHeight { get { return -1; } }
		public float flexibleWidth { get { return -1; } }
		public int layoutPriority { get { return -1; } }
		public float minHeight { get { return -1; } }
		public float minWidth { get { return -1; } }
		public float preferredHeight { get { return -1; } }
		public float preferredWidth { get { return -1; } }

        private void UpdateTargetRect()
        {
            if (target) {
                target.anchorMax = rectTransform.anchorMax;
                target.anchorMin = rectTransform.anchorMin;

                var pivot = rectTransform.pivot;
                var rect = rectTransform.rect;
                float hWidth = rect.width / 2, hHeight = rect.height / 2;
                float tLeft = -(hWidth - pading.left);
                float tRight = hWidth - pading.right;
                float tBottom = -(hHeight - pading.bottom);
                float tTop = hHeight - pading.top;

                target.pivot = new Vector2(
                    Mathf.InverseLerp(tLeft, tRight, 0), 
                    Mathf.InverseLerp(tBottom, tTop, 0));

                target.anchoredPosition3D = rectTransform.anchoredPosition3D 
                    - new Vector3(rect.width * (pivot.x - 0.5f), rect.height * (pivot.y - 0.5f), 0);

                target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tRight - tLeft);
                target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tTop - tBottom);            
            }
        }
        
        protected override void Start()
        {
            UpdateTargetRect();
        }
        
        protected override void OnEnable()
        {
            UpdateTargetRect();
        }

		protected override void OnRectTransformDimensionsChange ()
		{
            UpdateTargetRect();
		}
        
		public void CalculateLayoutInputHorizontal ()
		{
            UpdateTargetRect();
		}
		
		public void CalculateLayoutInputVertical ()
		{
            UpdateTargetRect();
		}
	}
}

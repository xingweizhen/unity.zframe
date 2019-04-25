using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace ZFrame.UGUI
{
    [RequireComponent(typeof(Graphic))]
    public class LabelSizeFitter : UIBehaviour, ILayoutElement
    {
        [SerializeField]
        private Vector2 m_Padding;
        public Vector2 padding { get { return m_Padding; } }

        [HideInInspector] public bool autoWidth = false;
        [HideInInspector] public bool autoHeight = false;
        [HideInInspector, SerializeField] private float m_MinWidth = -1;
        [HideInInspector, SerializeField] private float m_MinHeight = -1;

        public virtual bool ignoreLayout { get { return false; } }

        public virtual float minWidth { get { return m_MinWidth < 0 ? m_MinWidth : m_MinWidth + m_Padding.x; } }
        public virtual float minHeight { get { return m_MinHeight < 0 ? m_MinHeight : m_MinHeight + m_Padding.y; } }
        public virtual float preferredWidth { get { return minWidth; } }
        public virtual float preferredHeight { get { return minHeight; } }
        public virtual float flexibleWidth { get { return -1; } }
        public virtual float flexibleHeight { get { return -1; } }
        public virtual int layoutPriority { get { return 1; } }

        private RectTransform m_Rect;
        public RectTransform rectTransform {
            get {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        #region Unity Lifetime calls

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnTransformParentChanged()
        {
            SetDirty();
        }

        protected override void OnDisable()
        {
            SetDirty();
            base.OnDisable();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            SetDirty();
        }

        protected override void OnBeforeTransformParentChanged()
        {
            SetDirty();
        }

        #endregion

        protected void SetDirty()
        {
            if (!IsActive())
                return;
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }

#endif

        private void SetLayout(RectTransform.Axis axis)
        {
            rectTransform.SetSizeWithCurrentAnchors(axis, LayoutUtility.GetPreferredSize(m_Rect, (int)axis));
        }

        public virtual void CalculateLayoutInputHorizontal()
        {
            if (autoWidth) {
                m_MinWidth = -1;
                SetLayout(RectTransform.Axis.Horizontal);
                m_MinWidth = rectTransform.sizeDelta.x;
                SetDirty();
            }
        }

        public virtual void CalculateLayoutInputVertical()
        {
            if (autoHeight) {
                m_MinHeight = -1;
                SetLayout(RectTransform.Axis.Vertical);
                m_MinHeight = rectTransform.sizeDelta.y;
                SetDirty();
            }
        }

    }
}

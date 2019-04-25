using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace ZFrame.UGUI
{
    [RequireComponent(typeof(RectTransform))]
    public class FlexibleItem : UIBehaviour
    {
        [SerializeField]
        private RectTransform m_Flexible;

        private RectTransform m_Rect;
        public RectTransform rectTransform {
            get {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private void UpdateScale()
        {
            var rect = rectTransform.rect;
            var originRect = m_Flexible.rect;
            var scaleX = rect.width / originRect.width;
            var scaleY = rect.height / originRect.height;
            m_Flexible.localScale = new Vector3(scaleX, scaleY, 1);
        }

        private void UpdateRect()
        {
            var parent = rectTransform.parent as RectTransform;
            if (!parent) return;

            var layoutCtrl = parent.GetComponent(typeof(ILayoutController));
            if (layoutCtrl) return;

            var parentRect = parent.rect;
            if (parentRect.width <= 0 || parentRect.height <= 0) return;

            var selfRect = rectTransform.rect;
            if (selfRect.width <= 0 || selfRect.height <= 0) return;

            var originRect = m_Flexible.rect;

            var selfAspectRadio = selfRect.width / selfRect.height;
            var originAspectRadio = originRect.width / originRect.height;
            if (selfAspectRadio == originAspectRadio) return;

            var aspectRadio = parentRect.width / parentRect.height;
            if (aspectRadio == originAspectRadio) aspectRadio = selfAspectRadio;
            if (aspectRadio > originAspectRadio) {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, selfRect.height * originAspectRadio);
            } else if (aspectRadio < originAspectRadio) {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, selfRect.width / originAspectRadio);
            }
        }

        protected override void OnEnable()
        {
            UpdateRect();
            UpdateScale();
        }

        [ContextMenu("FlexItem")]
        protected override void OnRectTransformDimensionsChange()
        {
            if (!IsActive() || !m_Flexible) return;

            UpdateRect();
            UpdateScale();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            OnRectTransformDimensionsChange();
        }
#endif

    }
}

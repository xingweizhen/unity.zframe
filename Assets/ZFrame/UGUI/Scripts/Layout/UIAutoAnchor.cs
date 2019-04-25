using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZFrame.UGUI
{
    public class UIAutoAnchor : UIBehaviour
    {
        [SerializeField]
        private bool m_KeepInsideScreen = true;

        private Vector2 m_PovitF, m_PovitT, m_Offset;
        private RectTransform m_Target;

        private void Anchor()
        {
            if (enabled) {
                var rectTrans = transform as RectTransform;
                if (rectTrans && m_Target) {
                    var cv = transform.GetComponentInParent(typeof(Canvas)) as Canvas;
                    if (cv == null) return;
                    
                    var pos = cv.AnchorPosition(rectTrans, m_PovitF, m_Target, m_PovitT, m_Offset);
                    rectTrans.anchoredPosition = pos;

                    if (m_KeepInsideScreen) {
                        rectTrans.anchoredPosition = rectTrans.InsideScreenPosition();
                    }
                }
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            Anchor();
        }

        public void SetAnchor(Vector2 selfPovit, RectTransform target, Vector2 tarPivot, Vector2 offset)
        {
            m_PovitF = selfPovit;
            m_Target = target;
            m_PovitT = tarPivot;
            m_Offset = offset;

            Anchor();
        }
    }
}

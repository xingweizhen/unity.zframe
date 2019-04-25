using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZFrame.UGUI
{
    using Tween;

    public class UIButtonScale : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler
    {
        private const float TWEEN_DURA = 0.1f;

        [SerializeField]
        private float m_Scale = 0.9f;

        [SerializeField]
        private bool m_ResumeOnUp;


        #region IPointerDownHandler implementation
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            transform.TweenScaling(Vector3.one, Vector3.one * m_Scale, TWEEN_DURA);
        }
        #endregion

        #region IPointerUpHandler implementation

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            transform.TweenScaling(Vector3.one, TWEEN_DURA);
        }

        #endregion

        #region IBeginDragHandler implementation

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!m_ResumeOnUp) {
                transform.TweenScaling(Vector3.one, TWEEN_DURA);
            }
        }

        #endregion
    }
}

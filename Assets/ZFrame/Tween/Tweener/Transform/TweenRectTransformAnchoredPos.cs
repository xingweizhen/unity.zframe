using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    [TweenMenu("Transform/AnchoredPosition", "RectTransform AnchoredPosition")]
    public class TweenRectTransformAnchoredPos : TweenVector3<RectTransform>
    {
        public override void ResetStatus()
        {
            m_From = target ? target.anchoredPosition3D : Vector3.zero;
            m_To = m_From;
        }

        protected override ZTweener StartTween(bool reset, bool forward)
        {
            return target ? target.TweenAnchorPos(m_From, m_To, duration) : null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenRectTransformAnchoredPos))]
        private class MyEditor : TweenValueEditor
        {

        }
#endif
    }
}

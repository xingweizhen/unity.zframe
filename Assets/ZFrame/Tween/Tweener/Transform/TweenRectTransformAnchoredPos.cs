using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    [TweenMenu("Transform/AnchoredPosition", "RectTransform AnchoredPosition")]
    public class TweenRectTransformAnchoredPos : TweenVector3<RectTransform>
    {
        protected override Vector3 GetCurrentValue() { return target ? target.anchoredPosition3D : Vector3.zero; }

        protected override object StartTween(bool forward)
        {
            if (target) {
                if (reset) target.anchoredPosition3D = m_From;
                return target.TweenAnchorPos(m_To, duration).PlayForward(forward);
            }

            return null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenRectTransformAnchoredPos))]
        private class MyEditor : TweenValueEditor
        {

        }
#endif
    }
}

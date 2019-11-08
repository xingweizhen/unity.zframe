using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    [TweenMenu("Transform/Position", "Transform Position")]
    public class TweenTransformPosition : TweenTransformProperty
    {
        protected override Vector3 GetCurrentValue()
        {
            return target ?
                m_Space == Space.World ? target.position : target.localPosition :
                Vector3.zero;
        }

        protected override object StartTween(bool forward)
        {
            if (target) {
                if (m_Space == Space.World) {
                    if (reset) target.position = m_From;
                    return target.TweenPosition(m_To, duration).PlayForward(forward);
                } else {
                    if (reset) target.localPosition = m_From;
                    return target.TweenLocalPosition(m_To, duration).PlayForward(forward);
                }
            }

            return null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenTransformPosition))]
        private class MyEditor : TweenTransformEditor
        {
            
        }
#endif
    }
}

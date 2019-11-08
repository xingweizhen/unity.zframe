using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    [TweenMenu("Transform/EulerAngles", "Transform EulerAngles")]
    public class TweenTransformEulerAngles : TweenTransformProperty
    {
        protected override Vector3 GetCurrentValue()
        {
            return target ?
                m_Space == Space.World ? target.eulerAngles : target.localEulerAngles :
                Vector3.zero;
        }
        
        protected override object StartTween(bool forward)
        {
            if (target) {
                if (m_Space == Space.World) {
                    if (reset) target.eulerAngles = m_From;
                    return target.TweenEulerAngles(m_To, duration).PlayForward(forward);
                } else {
                    if (reset) target.localEulerAngles = m_From;
                    return target.TweenLocalEulerAngles(m_To, duration).PlayForward(forward);
                }
            }

            return null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenTransformEulerAngles))]
        private class MyEditor : TweenTransformEditor
        {
            
        }
#endif
    }
}

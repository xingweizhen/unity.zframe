using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    [TweenMenu("Transform/EulerAngles", "Transform: Euler Angles")]
    public class TweenTransformEulerAngles : TweenTransformProperty
    {
        public override void ResetStatus()
        {
            m_From = target ? m_Space  == Space.World ? target.eulerAngles : target.localEulerAngles : Vector3.zero;
            m_To = m_From;
        }

        protected override ZTweener StartTween(bool reset, bool forward)
        {
            if (m_Space == Space.World) {
                return target ? target.TweenRotation(m_From, m_To, duration) : null;
            }

            return target ? target.TweenLocalRotation(m_From, m_To, duration) : null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenTransformEulerAngles))]
        private class MyEditor : TweenTransformEditor
        {
            
        }
#endif
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    [TweenMenu("Transform/LocalScale", "Transform LocalScale")]
    public class TweenTransformLocalScale : TweenVector3<Transform>
    {
        protected override Vector3 GetCurrentValue() { return target ? target.localScale : Vector3.zero; }

        protected override object StartTween(bool forward)
        {
            if (target) {
                if (reset) target.localScale = m_From;
                return target.TweenScaling(m_To, duration).PlayForward(forward);
            }

            return null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenTransformLocalScale))]
        private class MyEditor : TweenValueEditor
        {
            
        }
#endif
    }
}

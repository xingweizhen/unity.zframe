using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    [TweenMenu("Transform/LocalScale", "Transform LocalScale")]
    public class TweenTransformLocalScale : TweenVector3<Transform>
    {
        public override void ResetStatus()
        {
            m_From = target ? target.localScale : Vector3.zero;
            m_To = m_From;
        }

        protected override ZTweener StartTween(bool reset, bool forward)
        {
            return target ? target.TweenScaling(m_From, m_To, duration) : null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenTransformLocalScale))]
        private class MyEditor : TweenValueEditor
        {
            
        }
#endif
    }
}

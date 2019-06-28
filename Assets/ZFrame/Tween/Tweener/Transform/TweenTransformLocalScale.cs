using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    public class TweenTransformLocalScale : TweenVector3<Transform>
    {
        public override void ResetStatus()
        {
            m_From = target ? target.localScale : Vector3.zero;
            m_To = m_From;
        }

        protected override ZTweener StartTween(bool reset, bool forward)
        {
            return target ? target.TweenScaling(m_From, m_To, duration)
                    .SetUpdate(UpdateType.Normal, ignoreTimescale).SetTag(this) : null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenTransformLocalScale))]
        private class MyEditor : TweenValueEditor
        {
            public override string TweenName { get { return "Transform: Local Scale"; } }
        }
#endif
    }
}

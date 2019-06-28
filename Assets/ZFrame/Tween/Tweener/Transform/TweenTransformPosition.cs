﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    public class TweenTransformPosition : TweenTransformProperty
    {
        public override void ResetStatus()
        {
            m_From = target ? m_Space  == Space.World ? target.position : target.localPosition : Vector3.zero;
            m_To = m_From;
        }

        protected override ZTweener StartTween(bool reset, bool forward)
        {
            if (m_Space == Space.World) {
                return target ? target.TweenPosition(m_From, m_To, duration)
                    .SetUpdate(UpdateType.Normal, ignoreTimescale).SetTag(this) : null;
            }

            return target ? target.TweenLocalPosition(m_From, m_To, duration)
                    .SetUpdate(UpdateType.Normal, ignoreTimescale).SetTag(this) : null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenTransformPosition))]
        private class MyEditor : TweenTransformEditor
        {
            public override string TweenName { get { return "Transform: Position"; } }
        }
#endif
    }
}

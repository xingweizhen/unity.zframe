using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    public sealed class TweenCanvasAlpha : TweenFloat<CanvasGroup>
    {
        public override void ResetStatus()
        {
            m_From = target ? target.alpha : 1;
            m_To = m_From;
        }

        protected override ZTweener StartTween(bool reset, bool forward)
        {
            return target ? target.TweenAlpha(m_From, m_To, duration)
                .SetUpdate(UpdateType.Normal, ignoreTimescale).SetTag(this) : null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenCanvasAlpha))]
        private class MyEditor : TweenValueEditor
        {
            public override string TweenName { get { return "UGUI: Canvas Alpha"; } }
        }
#endif
    }
}


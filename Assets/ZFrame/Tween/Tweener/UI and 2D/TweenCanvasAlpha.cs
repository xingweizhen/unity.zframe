using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    [TweenMenu("UI and 2D/Alpha(CanvasGroup)", "CanvasGroup Alpha")]
    public sealed class TweenCanvasAlpha : TweenFloat<CanvasGroup>
    {
        public override void ResetStatus()
        {
            m_From = target ? target.alpha : 1;
            m_To = 1 - m_From;
        }

        protected override object StartTween(bool reset, bool forward)
        {
            return target ? target.TweenAlpha(m_From, m_To, duration) : null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenCanvasAlpha))]
        private class MyEditor : TweenValueEditor
        {
            
        }
#endif
    }
}


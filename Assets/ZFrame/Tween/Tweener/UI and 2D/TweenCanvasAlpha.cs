using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    [TweenMenu("UI and 2D/Alpha(CanvasGroup)", "CanvasGroup Alpha")]
    public sealed class TweenCanvasAlpha : TweenFloat<CanvasGroup>
    {
        protected override float GetCurrentValue() { return target ? target.alpha : 1; }

        protected override object StartTween(bool forward)
        {
            if (target) {
                if (reset) target.alpha = m_From;
                return target.TweenAlpha(m_To, duration).PlayForward(forward);
            }
            return null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenCanvasAlpha))]
        private class MyEditor : TweenValueEditor
        {
            
        }
#endif
    }
}


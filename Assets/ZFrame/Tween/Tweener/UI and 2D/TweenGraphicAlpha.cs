using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.Tween
{
    [TweenMenu("UI and 2D/Alpha(Graphic)", "Graphic Alpha")]
    public sealed class TweenGraphicAlpha : TweenFloat<Graphic>
    {
        protected override float GetCurrentValue() { return target ? target.color.a : 1; }
        
        protected override object StartTween(bool forward)
        {
            if (target) {
                if (reset) {
                    var c = target.color;
                    c.a = m_From; 
                    target.color = c;
                }
                return target.TweenAlpha(m_To, duration).PlayForward(forward);
            }
            return null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenGraphicAlpha))]
        private class MyEditor : TweenValueEditor
        {
            
        }
#endif
    }
}

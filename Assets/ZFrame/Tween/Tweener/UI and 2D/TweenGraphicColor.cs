using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.Tween
{
    [TweenMenu("UI and 2D/Color(Graphic)", "Graphic Color")]
    public sealed class TweenGraphicColor : TweenColor<Graphic>
    {
        protected override Color GetCurrentValue() { return target ? target.color : Color.white; }
      
        protected override object StartTween(bool forward)
        {
            if (target) {
                if (reset) target.color = m_From;
                return target.TweenColor(m_To, duration).PlayForward(forward);
            }
            return null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenGraphicColor))]
        private class MyEditor : TweenValueEditor
        {
            
        }
#endif
    }
}

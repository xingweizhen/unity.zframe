using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.Tween
{
    [TweenMenu("UI and 2D/Graphic Color", "UGUI: Graphic Color")]
    public sealed class TweenGraphicColor : TweenColor<Graphic>
    {
        public override void ResetStatus()
        {
            m_From = target ? target.color : Color.white;
            m_To = m_From;
        }

        protected override ZTweener StartTween(bool reset, bool forward)
        {
            return target ? target.TweenColor(m_From, m_To, duration) : null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenGraphicColor))]
        private class MyEditor : TweenValueEditor
        {
            
        }
#endif
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.Tween
{
    [TweenMenu("UI and 2D/Alpha(Graphic)", "UGUI: Graphic Alpha")]
    public sealed class TweenGraphicAlpha : TweenFloat<Graphic>
    {
        public override void ResetStatus()
        {
            m_From = target ? target.color.a : 1;
            m_To = 1 - m_From;
        }

        protected override ZTweener StartTween(bool reset, bool forward)
        {
            return target ? target.TweenAlpha(m_From, m_To, duration) : null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenGraphicAlpha))]
        private class MyEditor : TweenValueEditor
        {
            
        }
#endif
    }
}

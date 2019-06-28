using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.Tween
{
    public sealed class TweenGraphicColor : TweenColor<Graphic>
    {
        public override void ResetStatus()
        {
            m_From = target ? target.color : Color.white;
            m_To = m_From;
        }

        protected override ZTweener StartTween(bool reset, bool forward)
        {
            return target ? target.TweenColor(m_From, m_To, duration)
                .SetUpdate(UpdateType.Normal, ignoreTimescale).SetTag(this) : null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenGraphicColor))]
        private class MyEditor : TweenValueEditor
        {
            public override string TweenName { get { return "UGUI: Graphic Color"; } }
        }
#endif
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    public sealed class TweenRectTransformSizeDelta : TweenVector2<RectTransform>
    {
        public override void ResetStatus()
        {
            m_From = target ? target.sizeDelta : Vector2.zero;
            m_To = m_From;
        }

        protected override ZTweener StartTween(bool reset, bool forward)
        {
            return target ? target.TweenSize(m_From, m_To, duration)
                .SetUpdate(UpdateType.Normal, ignoreTimescale).SetTag(this) : null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenRectTransformSizeDelta))]
        private class MyEditor : TweenValueEditor
        {
            public override string TweenName { get { return "RectTransform: SizeDelta"; } }
        }
#endif
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    [TweenMenu("Transform/SizeDelta", "RectTransform SizeDelta")]
    public sealed class TweenRectTransformSizeDelta : TweenVector2<RectTransform>
    {
        public override void ResetStatus()
        {
            m_From = target ? target.sizeDelta : Vector2.zero;
            m_To = m_From;
        }

        protected override object StartTween(bool reset, bool forward)
        {
            return target ? target.TweenSize(m_From, m_To, duration) : null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenRectTransformSizeDelta))]
        private class MyEditor : TweenValueEditor
        {
            
        }
#endif
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    [TweenMenu("Transform/SizeDelta", "RectTransform SizeDelta")]
    public sealed class TweenRectTransformSizeDelta : TweenVector2<RectTransform>
    {
        protected override Vector2 GetCurrentValue() {  return target ? target.sizeDelta : Vector2.zero; }

        protected override object StartTween(bool forward)
        {
            if (target) {
                if (reset) target.sizeDelta = m_From;
                return target.TweenSize(m_To, duration).PlayForward(forward);
            }

            return null;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenRectTransformSizeDelta))]
        private class MyEditor : TweenValueEditor
        {
            
        }
#endif
    }
}

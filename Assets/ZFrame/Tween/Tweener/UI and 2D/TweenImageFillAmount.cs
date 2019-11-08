using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.Tween
{
    [TweenMenu("UI and 2D/FillAmount(Image)", "Image FillAmount")]
    public class TweenImageFillAmount : TweenFloat<Image>
    {
        protected override float GetCurrentValue() { return target ? target.fillAmount : 1; }

        protected override object StartTween(bool forward)
        {
            if (target) {
                if (reset) target.fillAmount = m_From;
                return target.TweenFill(m_To, duration).PlayForward(forward);
            }

            return null;
        }
#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenImageFillAmount))]
        private class MyEditor : TweenValueEditor
        {

        }
#endif
    }
}

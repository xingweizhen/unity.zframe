using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.Tween
{
    [TweenMenu("UI and 2D/FillAmount(Image)", "Image FillAmount")]
    public class TweenImageFillAmount : TweenFloat<Image>
    {
        public override void ResetStatus()
        {
            m_From = target ? target.fillAmount : 1;
            m_To = 1 - m_From;
        }

        protected override ZTweener StartTween(bool reset, bool forward)
        {
            return target ? target.TweenFill(m_From, m_To, duration) : null;
        }
#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(TweenImageFillAmount))]
        private class MyEditor : TweenValueEditor
        {

        }
#endif
    }
}

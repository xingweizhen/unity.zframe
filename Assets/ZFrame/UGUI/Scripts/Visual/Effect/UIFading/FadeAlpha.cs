using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    using Tween;
    public class FadeAlpha : FadeTemplate<float>
    {
        public override void Apply()
        {
            CanvasGroup cv = target.GetComponent<CanvasGroup>();
            if (cv) {
                m_Source = cv.alpha;
            }

            Graphic graphic = target.GetComponent<Graphic>();
            if (graphic) {
                m_Source = graphic.color.a;
            }
            m_Destina = m_Source;            
        }

        protected override void Restart()
        {
            CanvasGroup cv = target.GetComponent<CanvasGroup>();
            if (cv) {
                cv.alpha = (float)source;
                return;
            }

            Graphic graphic = target.GetComponent<Graphic>();
            if (graphic) {
                var color = graphic.color;
                color.a = (float)source;
                graphic.color = color;
                return;
            }
        }

        protected override ZTweener AnimateFade(bool forward)
        {
            var tweenTar = forward ? destina : source;

            CanvasGroup cv = target.GetComponent<CanvasGroup>();
            if (cv) {
                return cv.TweenAlpha((float)tweenTar, duration);
            }

            Graphic graphic = target.GetComponent<Graphic>();
            if (graphic) {
                return graphic.TweenAlpha((float)tweenTar, duration);
            }

            LogMgr.W("FadeAlpha失败：没有找到<CanvasGroup>或者<Graphic>");
            return null;
        }
    }
}

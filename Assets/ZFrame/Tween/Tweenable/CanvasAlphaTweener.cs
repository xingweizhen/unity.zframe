using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.Tween
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasAlphaTweener : BaseTweener, ITweenable<float>
    {
        public override object Tween(object from, object to, float duration)
        {
            var cvGrp = gameObject.GetComponent<CanvasGroup>();
            var alpha = (float)to;
            var tw = cvGrp.TweenAlpha(alpha, duration);
			tw.SetTag(this);
            if (from != null) {
                cvGrp.alpha = (float)from;
				tw.StartFrom(from);
            }
			return tw;
        }

        public object Tween(float to, float duration)
        {
            var cvGrp = gameObject.GetComponent<CanvasGroup>();
            return cvGrp.TweenAlpha(to, duration).SetTag(this);
        }

        public object Tween(float from, float to, float duration)
        {
            var cvGrp = gameObject.GetComponent<CanvasGroup>();
            return cvGrp.TweenAlpha(from, to, duration).SetTag(this);
        }
    }
}

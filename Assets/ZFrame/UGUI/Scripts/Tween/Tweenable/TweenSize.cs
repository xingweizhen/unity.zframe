using UnityEngine;
using System.Collections;

namespace ZFrame.Tween
{
    public class TweenSize : BaseTweener, ITweenable<Vector2>
    {
        public override ZTweener Tween(object from, object to, float duration)
        {
            var trans = transform as RectTransform;
            if (trans == null) return null;

            var tw = trans.TweenSize((Vector2)to, duration);

            tw.SetTag(this);
            if (from != null) {
                var v2 = (Vector2)from;
                trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, v2.x);
                trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, v2.y);
                tw.StartFrom(from);
            }
            return tw;
        }

        public ZTweener Tween(Vector2 to, float duration)
        {

            var trans = transform as RectTransform;
            if (trans == null) return null;
            return trans.TweenSize(to, duration).SetTag(this);
        }

        public ZTweener Tween(Vector2 from, Vector2 to, float duration)
        {

            var trans = transform as RectTransform;
            if (trans == null) return null;

            return trans.TweenSize(from, to, duration).SetTag(this);
        }
    }
}

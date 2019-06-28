using UnityEngine;
using System.Collections;

namespace ZFrame.Tween
{
    [RequireComponent(typeof(RectTransform))]
    public class RectSizeTweener : BaseTweener, ITweenable<Vector2>
    {
        public override ZTweener Tween(object from, object to, float duration)
        {
            if (to is Vector2) {
                if (from is Vector2) {
                    return Tween((Vector2)from, (Vector2)to, duration);
                } else {
                    return Tween((Vector2)to, duration);
                }
            }

            return null;
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

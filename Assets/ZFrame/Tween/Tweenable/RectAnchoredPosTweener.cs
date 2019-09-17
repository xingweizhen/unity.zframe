using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    [RequireComponent(typeof(RectTransform))]
    public class RectAnchoredPosTweener : BaseTweener, ITweenable<Vector3>
    {
        public override object Tween(object from, object to, float duration)
        {
            if (to is Vector3) {
                if (from is Vector3) {
                    return Tween((Vector3)from, (Vector3)to, duration);
                } else {
                    return Tween((Vector3)to, duration);
                }
            }

            return null;
        }

        public object Tween(Vector3 to, float duration)
        {
            var trans = transform as RectTransform;
            if (trans == null) return null;

            return trans.TweenAnchorPos(to, duration).SetTag(this);
        }

        public object Tween(Vector3 from, Vector3 to, float duration)
        {
            var trans = transform as RectTransform;
            if (trans == null) return null;

            return trans.TweenAnchorPos(from, to, duration).SetTag(this);
        }
    }
}

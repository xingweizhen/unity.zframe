using UnityEngine;
using System.Collections;
//using DG.Tweening;

namespace ZFrame.Tween
{
    public class TweenScaling : BaseTweener, ITweenable<Vector3>
    {
        public override ZTweener Tween(object from, object to, float duration)
        {
            var trans = transform;
            var tw = trans.TweenScaling((Vector3)to, duration);

            tw.SetTag(this);
            if (from != null) {
                trans.localScale = (Vector3)from;
                tw.StartFrom(from);
            }
            return tw;
        }

        public ZTweener Tween(Vector3 to, float duration)
        {
            return transform.TweenScaling(to, duration).SetTag(this);
        }

        public ZTweener Tween(Vector3 from, Vector3 to, float duration)
        {
            return transform.TweenScaling(from, to, duration).SetTag(this);
        }
    }
}
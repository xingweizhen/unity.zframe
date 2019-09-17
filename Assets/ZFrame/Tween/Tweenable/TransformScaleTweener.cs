using UnityEngine;
using System.Collections;
//using DG.Tweening;

namespace ZFrame.Tween
{
    public class TransformScaleTweener : BaseTweener, ITweenable<Vector3>
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
            return transform.TweenScaling(to, duration).SetTag(this);
        }

        public object Tween(Vector3 from, Vector3 to, float duration)
        {
            return transform.TweenScaling(from, to, duration).SetTag(this);
        }
    }
}
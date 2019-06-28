using UnityEngine;
using System.Collections;
//using DG.Tweening;

namespace ZFrame.Tween
{
    public class TransformPositionTweener : BaseTweener, ITweenable<Vector3>
    {
        public Space space = Space.Self;

        public override ZTweener Tween(object from, object to, float duration)
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

        public ZTweener Tween(Vector3 to, float duration)
        {
            switch (space) {
                case Space.Self:
                    return transform.TweenLocalPosition(to, duration).SetTag(this);
                case Space.World:
                    return transform.TweenPosition(to, duration).SetTag(this);
            }
            return null;
        }

        public ZTweener Tween(Vector3 from, Vector3 to, float duration)
        {
            switch (space) {
                case Space.Self:
                    return transform.TweenLocalPosition(from, to, duration).SetTag(this);
                case Space.World:
                    return transform.TweenPosition(from, to, duration).SetTag(this);
            }
            return null;
        }
    }
}

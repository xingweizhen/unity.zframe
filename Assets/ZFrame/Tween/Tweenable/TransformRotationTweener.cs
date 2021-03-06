﻿using UnityEngine;
using System.Collections;

namespace ZFrame.Tween
{
    public class TransformRotationTweener : BaseTweener, ITweenable<Vector3>
    {
        public Space space = Space.Self;

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
            switch (space) {
                case Space.Self:
                    return transform.TweenLocalEulerAngles(to, duration).SetTag(this);
                case Space.World:
                    return transform.TweenEulerAngles(to, duration).SetTag(this);
            }

            return null;
        }

        public object Tween(Vector3 from, Vector3 to, float duration)
        {
            switch (space) {
                case Space.Self:
                    return transform.TweenLocalEulerAngles(from, to, duration).SetTag(this);
                case Space.World:
                    return transform.TweenEulerAngles(from, to, duration).SetTag(this);
            }

            return null;
        }
    }
}

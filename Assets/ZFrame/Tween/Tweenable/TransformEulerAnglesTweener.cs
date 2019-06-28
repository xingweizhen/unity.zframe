using UnityEngine;
using System.Collections;

namespace ZFrame.Tween
{
    public class TransformEulerAnglesTweener : BaseTweener, ITweenable<Vector3>
    {
        public Space space = Space.Self;

        private Vector3 LocalGetter()
        {
            return transform.localEulerAngles;
        }

        private void LocalSetter(Vector3 value)
        {
            transform.localEulerAngles = value;
        }

        private Vector3 WorldGetter()
        {
            return transform.eulerAngles;
        }

        private void WorldSetter(Vector3 value)
        {
            transform.eulerAngles = value;
        }

        public override ZTweener Tween(object from, object to, float duration)
        {
            ZTweener tw = null;
            var trans = transform;
            switch (space) {
                case Space.Self:
                    tw = this.Tween(LocalGetter, LocalSetter, (Vector3)to, duration);
                    break;
                case Space.World:
                    tw = this.Tween(WorldGetter, WorldSetter, (Vector3)to, duration);
                    break;
            }
            if (tw != null) {
                if (from != null) {
                    trans.localRotation = Quaternion.Euler((Vector3)from);
                    tw.StartFrom(from);
                }

                tw.SetTag(this);
            }

            return tw;
        }

        public ZTweener Tween(Vector3 to, float duration)
        {
            switch (space) {
                case Space.Self:
                    return this.Tween(LocalGetter, LocalSetter, to, duration).SetTag(this);
                case Space.World:
                    return this.Tween(WorldGetter, WorldSetter, to, duration).SetTag(this);
            }
            return null;
        }

        public ZTweener Tween(Vector3 from, Vector3 to, float duration)
        {
            switch (space) {
                case Space.Self: transform.localEulerAngles = from; break;
                case Space.World: transform.eulerAngles = from; break;
            }
            return Tween(to, duration);
        }
    }
}

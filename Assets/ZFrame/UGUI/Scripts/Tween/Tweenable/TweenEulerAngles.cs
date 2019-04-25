using UnityEngine;
using System.Collections;

namespace ZFrame.Tween
{
    public class TweenEulerAngles : BaseTweener, ITweenable<Vector3>
    {
        Transform mTrans;
        public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

        private Vector3 Getter()
        {
            return cachedTransform.localEulerAngles;
        }

        private void Setter(Vector3 value)
        {
            cachedTransform.localEulerAngles = value;
        }

        public override ZTweener Tween(object from, object to, float duration)
        {
            ZTweener tw = null;
            var trans = transform;
            tw = this.Tween(Getter, Setter, (Vector3)to, duration);
            if (from != null) {
                trans.localRotation = Quaternion.Euler((Vector3)from);
                tw.StartFrom(from);
            }

            tw.SetTag(this);
            return tw;
        }

        public ZTweener Tween(Vector3 to, float duration)
        {
            return this.Tween(Getter, Setter, to, duration).SetTag(this);
        }

        public ZTweener Tween(Vector3 from, Vector3 to, float duration)
        {
            cachedTransform.localRotation = Quaternion.Euler(from);
            return this.Tween(Getter, Setter, to, duration).SetTag(this);
        }
    }
}

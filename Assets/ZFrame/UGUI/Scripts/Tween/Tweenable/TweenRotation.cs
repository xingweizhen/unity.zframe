using UnityEngine;
using System.Collections;

namespace ZFrame.Tween
{
	public class TweenRotation : BaseTweener, ITweenable<Vector3>
	{
		public override ZTweener Tween (object from, object to, float duration)
		{
            ZTweener tw = null;
            var trans = transform;
			tw = trans.TweenLocalRotation((Vector3)to, duration);
			if (from != null) {
				trans.localRotation = Quaternion.Euler((Vector3)from);
				tw.StartFrom(from);
			}

            tw.SetTag(this);
            return tw;
		}

        public ZTweener Tween(Vector3 to, float duration)
        {
            return transform.TweenLocalRotation(to, duration).SetTag(this);
        }

        public ZTweener Tween(Vector3 from, Vector3 to, float duration)
        {
            return transform.TweenLocalRotation(from, to, duration).SetTag(this);
        }
    }
}

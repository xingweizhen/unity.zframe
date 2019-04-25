using UnityEngine;
using System.Collections;
//using DG.Tweening;

namespace ZFrame.Tween
{
	public class TweenPositionW : BaseTweener, ITweenable<Vector3>
    {
        public override ZTweener Tween(object from, object to, float duration)
        {
            var trans = transform;
			Vector3? v3From = null, v3To =  null;
            v3To = (Vector3)to;

            ZTweener tw = null;
			if (v3To != null) {
                tw = trans.TweenPosition((Vector3)v3To, duration);

                if (from is Vector3) {
					v3From = (Vector3)from;
				} else if (from is Vector2) {
					v3From = (Vector2)from;
				}
				if (v3From != null) {
					tw.StartFrom((Vector3)v3From);
				}
			}

            if (tw != null) tw.SetTag(this);
			return tw;
        }

        public ZTweener Tween(Vector3 to, float duration)
        {
            return transform.TweenPosition(to, duration).SetTag(this);
        }

        public ZTweener Tween(Vector3 from, Vector3 to, float duration)
        {
            return transform.TweenPosition(from, to, duration).SetTag(this);
        }
    }
}

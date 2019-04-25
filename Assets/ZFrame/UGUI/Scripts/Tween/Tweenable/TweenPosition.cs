using UnityEngine;
using System.Collections;
//using DG.Tweening;

namespace ZFrame.Tween
{
	public class TweenPosition : BaseTweener, ITweenable<Vector3>
    {
        public override ZTweener Tween(object from, object to, float duration)
        {
            var trans = transform;
            var rect = trans as RectTransform;
			Vector3? v3From = null, v3To =  null;
			if (to is Vector3) {
				v3To = (Vector3)to;
			} else if (to is Vector2) {
				v3To = (Vector2)to;
			}

            ZTweener tw = null;
			if (v3To != null) {
				if (rect) {
					tw = rect.TweenAnchorPos((Vector3)v3To, duration);
				} else {
					tw = trans.TweenLocalPosition((Vector3)v3To, duration);
				}

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
            ZTweener tw = null;
            var trans = transform;
            var rect = trans as RectTransform;
            if (rect) {
                tw = rect.TweenAnchorPos(to, duration);
            } else {
                tw = trans.TweenLocalPosition(to, duration);
            }

            if (tw != null) tw.SetTag(this);
            return tw;
        }

        public ZTweener Tween(Vector3 from, Vector3 to, float duration)
        {
            var trans = transform;
            var rect = trans as RectTransform;
            if (rect) {
                rect.anchoredPosition = from;
            } else {
                trans.localPosition = from;
            }
            return Tween(to, duration);
        }
    }
}

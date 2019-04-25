using UnityEngine;
using System.Collections;
namespace ZFrame.UGUI
{
    using Tween;

    public class FadeScaling : FadeTemplate<Vector3> 
	{
        public override void Apply()
        {
            var trans = target.GetComponent<Transform>();
            m_Source = trans.localScale;
            m_Destina = m_Source;
        }

        protected override void Restart()
        {
            var trans = target.GetComponent<Transform>();
            if (trans) {
                trans.localScale = (Vector3)source;
            }
        }

		protected override ZTweener AnimateFade (bool forward)
        {
            var tweenTar = forward ? destina : source;

            var trans = target.GetComponent<Transform>();
			return trans.TweenScaling((Vector3)tweenTar, duration);
		}
	}
}

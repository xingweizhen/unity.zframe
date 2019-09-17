using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI
{ 
    using Tween;

    public class FadeRotation : FadeTemplate<Vector3> 
	{
		public RotateMode rotateMode = RotateMode.Fast;

        public override void Apply()
        {
            var trans = target.GetComponent<Transform>();
            m_Source = trans.localRotation.eulerAngles;
            m_Destina = m_Source;
        }

        protected override void Restart()
        {
            var trans = target.GetComponent<Transform>();
            if (trans) {
                trans.localRotation = Quaternion.Euler((Vector3)source);
            }
        }

		protected override object AnimateFade (bool forward)
        {
            var tweenTar = forward ? destina : source;

            var trans = target.GetComponent<Transform>();
            return trans.TweenLocalRotation((Vector3)tweenTar, duration, rotateMode);
		}
	}
}

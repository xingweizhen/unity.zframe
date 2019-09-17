using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI
{
    using Tween;
    public class FadePositionW : FadeTemplate<Vector3>
    {
        public override void Apply()
        {
            var trans = target.GetComponent<Transform>();            
            m_Source = trans.position;
            m_Destina = m_Source;
        }

        protected override void Restart()
        {
            var trans = target.GetComponent<Transform>();
            trans.position = (Vector3)source;
        }

        protected override object AnimateFade(bool forward)
        {
            var tweenTar = forward ? destina : source;

            var trans = target.GetComponent<Transform>();
            return trans.TweenPosition((Vector3)tweenTar, duration);
        }
    }
}

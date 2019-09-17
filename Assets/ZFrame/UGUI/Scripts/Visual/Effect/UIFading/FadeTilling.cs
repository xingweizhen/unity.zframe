using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI
{
    using Tween;

    public class FadeTilling : FadeTemplate<Vector2>
    {
        public override void Apply()
        {
            var rdr = target.GetComponent<Renderer>();
            if (rdr && rdr.sharedMaterial) {
                m_Source = rdr.sharedMaterial.mainTextureScale;
                m_Destina = m_Source;
                return;
            }

            LogMgr.W("FadeTintColor：没有找到<Renderer>或<Material>");
        }

        protected override void Restart()
        {
            var rdr = target.GetComponent<Renderer>();
            if (rdr && rdr.material) {
                rdr.material.mainTextureScale = (Vector2)source;
            }
        }

        protected override object AnimateFade(bool forward)
        {
            var tweenTar = forward ? destina : source;

            var rdr = target.GetComponent<Renderer>();
            if (rdr && rdr.material) {
                return rdr.material.TweenTilling((Vector2)tweenTar, duration);
            }

            LogMgr.W("FadeTintColor：没有找到<Renderer>或<Material>");
            return null;
        }
    }
}

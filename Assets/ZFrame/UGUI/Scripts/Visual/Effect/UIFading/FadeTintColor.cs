using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI
{
    using Tween;

    public class FadeTintColor : FadeTemplate<Color>
    {
        [SerializeField]
        private string m_Tint = "_TintColor";
        
        public override void Apply()
        {
            var rdr = target.GetComponent<Renderer>();
            if (rdr && rdr.sharedMaterial) {
                m_Source = rdr.sharedMaterial.GetColor(m_Tint);
                m_Destina = m_Source;
                return;
            }

            LogMgr.W("FadeTintColor：没有找到<Renderer>或<Material>");
        }

        protected override void Restart()
        {
            var rdr = target.GetComponent<Renderer>();
            if (rdr && rdr.material) {
                rdr.material.SetColor(m_Tint, (Color)source);
            }
        }

        protected override ZTweener AnimateFade(bool forward)
        {
            var tweenTar = forward ? destina : source;

            var rdr = target.GetComponent<Renderer>();
            if (rdr && rdr.material) {
                return rdr.material.TweenColor((Color)tweenTar, m_Tint, duration);
            }

            LogMgr.W("FadeTintColor：没有找到<Renderer>或<Material>");
            return null;
        }
    }
}

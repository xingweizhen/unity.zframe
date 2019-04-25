using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
	using Tween;

    public class FadeColor : FadeTemplate<Color>
    {
        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                m_Source = Color.white;
                m_Destina = Color.white;
            }
#endif
        }

        public override void Apply()
        {
            Graphic graphic = target.GetComponent<Graphic>();
            if (graphic) {
                m_Source = graphic.color;
                m_Destina = m_Source;
                return;
            }

            LogMgr.W("FadeColor失败：没有找到<Graphic>");
        }

        protected override void Restart()
        {
            Graphic graphic = target.GetComponent<Graphic>();
            if (graphic) {
                graphic.color = (Color)source;
            }
        }

        protected override ZTweener AnimateFade(bool forward)
        {
            var tweenTar = forward ? destina : source;

            Graphic graphic = target.GetComponent<Graphic>();
            if (graphic) {
                return graphic.TweenColor((Color)tweenTar, duration);
            }

            LogMgr.W("FadeColor失败：没有找到<Graphic>");
            return null;
        }
    }
}
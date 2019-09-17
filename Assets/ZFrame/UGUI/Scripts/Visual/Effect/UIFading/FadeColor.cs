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
                graphic.color = m_Source;
            }
        }

        protected override object AnimateFade(bool forward)
        {
            var tweenTar = forward ? m_Destina : m_Source;

            Graphic graphic = target.GetComponent<Graphic>();
            if (graphic) {
                return graphic.TweenColor(tweenTar, duration);
            }

            LogMgr.W("FadeColor失败：没有找到<Graphic>");
            return null;
        }
    }
}
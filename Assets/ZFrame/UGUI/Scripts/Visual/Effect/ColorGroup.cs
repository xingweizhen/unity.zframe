using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
	using Tween;

    public sealed class ColorGroup : MonoBehavior, ITweenable, ITweenable<Color>
    {
        [SerializeField]
        private Color m_Color = Color.white;
        public Color color
        {
            get { return m_Color; }
            set
            {
                m_Color = value;
                if (m_Graphics != null) {
                    for (int i = 0; i < m_Graphics.Length; ++i) {
                        m_Graphics[i].color = m_Color;
                    }
                }
            }
        }

        [Description("Graphics")]
        private Graphic[] m_Graphics;

        private void OnEnable()
        {
            m_Graphics = GetComponentsInChildren<Graphic>();
        }

        private Color ColorGetter() { return m_Color; }
        private void ColorSetter(Color value) { color = value; }

        public object Tween(object from, object to, float duration)
        {
            object tw = null;
            if (to is Color) {
                tw = this.TweenAny(ColorGetter, ColorSetter, (Color)from, (Color)to, 0f);
                if (from is Color) {
                    color = (Color)from;
					tw.StartFrom(color);
                }
            }

            if (tw != null) tw.SetTag(this);
            return tw;
        }

        public object Tween(Color to, float duration)
        {
            var tw = this.TweenAny(ColorGetter, ColorSetter, m_Color, to, duration);
            if (tw != null) tw.SetTag(this);
            return tw;
        }

        public object Tween(Color from, Color to, float duration)
        {
            var tw = this.TweenAny(ColorGetter, ColorSetter, from, to, duration);
            if (tw != null) tw.SetTag(this);
            return tw;
        }
    }
}

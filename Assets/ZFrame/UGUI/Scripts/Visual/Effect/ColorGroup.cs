using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
	using Tween;

    public sealed class ColorGroup : MonoBehavior, ITweenable
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

        public ZTweener Tween(object from, object to, float duration)
        {
            ZTweener tw = null;
            if (to is Color) {
                tw = this.Tween(ColorGetter, ColorSetter, (Color)to, 0f);
                if (from is Color) {
                    color = (Color)from;
					tw.StartFrom(color);
                }
            }

            if (tw != null) tw.SetTag(this);
            return tw;
        }
    }
}

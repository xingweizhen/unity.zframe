using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    using Tween;

    public class FadeHeight : FadeTemplate<float>
    {
        public override object source {
            get {
                var rect = target.GetComponent<RectTransform>();
                return rect ? new Vector2(rect.sizeDelta.x, m_Source) : default(Vector2);
            }
        }

        public override object destina {
            get {
                var rect = target.GetComponent<RectTransform>();
                return rect ? new Vector2(rect.sizeDelta.x, m_Destina) : default(Vector2);
            }
        }

        public override void Apply()
        {
            var rect = target.GetComponent<RectTransform>();
            if (rect) {
                m_Source = rect.sizeDelta.y;
                m_Destina = m_Source;
                return;
            }

            LogMgr.W("FadeHeight失败：没有找到<RectTransform>");
        }

        protected override void Restart()
        {
            var rect = target.GetComponent<RectTransform>();
            if (rect) {
                rect.sizeDelta = (Vector2)source;
            }
        }

        protected override ZTweener AnimateFade(bool forward)
        {
            var tweenTar = forward ? destina : source;

            var rect = target.GetComponent<RectTransform>();
            if (rect) {
                return rect.TweenSize((Vector2)tweenTar, duration);
            }

            LogMgr.W("FadeHeight失败：没有找到<RectTransform>");
            return null;
        }
    }
}
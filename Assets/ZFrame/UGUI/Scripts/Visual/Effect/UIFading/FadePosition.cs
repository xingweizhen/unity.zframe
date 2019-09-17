using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI
{
	using Tween;

    public class FadePosition : FadeTemplate<Vector3>
    {
        public override void Apply()
        {
            var trans = target.GetComponent<Transform>();
            var rectTrans = trans as RectTransform;
            if (rectTrans) {
                m_Source = rectTrans.anchoredPosition3D;                
            } else if (trans) {
                m_Source = trans.localPosition;
            }
            m_Destina = m_Source;
        }

        protected override void Restart()
        {
            var trans = target.GetComponent<Transform>();
            var rectTrans = trans as RectTransform;
            if (rectTrans) {
                if (m_bRestartUseOriginalPos)
                    m_Source = rectTrans.anchoredPosition3D;
                else
                    rectTrans.anchoredPosition3D = (Vector3)source;
            } else if (trans) {
                trans.localPosition = (Vector3)source;
            }
        }

        protected override object AnimateFade(bool forward)
        {
            var tweenTar = forward ? destina : source;

            var trans = target.GetComponent<Transform>();
			var rectTrans = trans as RectTransform;
			if (rectTrans) {
                return rectTrans.TweenAnchorPos((Vector3)tweenTar, duration);
			} else if (trans) {
                return trans.TweenLocalPosition((Vector3)tweenTar, duration);
			}

            LogMgr.W("FadePosition失败：没有找到<Transform>或者<RectTransform>");
            return null;
        }
    }
}

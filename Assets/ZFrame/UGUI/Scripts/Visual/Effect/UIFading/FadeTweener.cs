using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI
{
    using Tween;

    public abstract class FadeTweener<T> : FadeTemplate<T>
    {
        protected override void Restart ()
        {

        }

        public override void Apply ()
        {

        }

        protected override ZTweener AnimateFade(bool forward)
        {
            object tweenSrc, tweenDst;
            if (forward) {
                tweenSrc = source;
                tweenDst = destina;
            } else {
                tweenSrc = destina;
                tweenDst = source;
            }
            var tween = GetComponent(typeof(ITweenable)) as ITweenable;
            if (tween != null) {
                var tw = tween.Tween(tweenSrc, tweenDst, duration);
                if (tw == null) {
                    LogMgr.W("Fade {0}失败：<ITweenable>不支持参数<{1}>", typeof(T).Name, tweenSrc.GetType().Name);
                }
                return tw;
            }

            LogMgr.W("Fade {0}失败：没有找到<ITweenable>", typeof(T).Name);
            return null;
        }
    }
}

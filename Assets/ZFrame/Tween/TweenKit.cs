using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.Tween
{
    public interface ITweenKit
    {
        #region Tweener Methods
        bool IsTweening(object self);
        object SetTag(object self, object tag);
        object SetTimeScale(object self, float timeScale);
        object SetUpdate(object self, UpdateType updateType, bool ignoreTimeScale);
        object DelayFor(object self, float time);
        object LoopFor(object self, int loops, LoopType loopType);
        object EaseBy(object self, Ease ease);
        object StartFrom(object self, object from);
        object EndAt(object self, object at);
        object StartWith(object self, CallbackOnUpdate onStart);
        object UpdateWith(object self, CallbackOnUpdate onUpdate);
        object CompleteWith(object self, CallbackOnComplete onComplete);
        object Stop(object self, bool complete = false);
        #endregion

        void Init();
        int Finish(object tarOrTag, bool complete = false);
        object TweenPosition(Transform self, Vector3 from, Vector3 to, float duration, Space space = Space.Self);
        object TweenRotation(Transform self, Vector3 from, Vector3 to, float duration, Space space = Space.Self, RotateMode mode = RotateMode.Fast);
        object TweenScaling(Transform self, Vector3 from, Vector3 to, float duration);
        object TweenAnchoredPosition(RectTransform self, Vector3 from, Vector3 to, float duration);
        object TweenSizeDelta(RectTransform self, Vector2 from, Vector2 to, float duration);
        object TweenAlpha(CanvasGroup self, float from, float to, float duration);
        object TweenAlpha(Graphic self, float from, float to, float duration);
        object TweenColor(Graphic self, Color from, Color to, float duration);
        object TweenFillAmount(Image self, float from, float to, float duration);
        object TweenString(Text self, string from, string to, float duration);
        object TweenMaterialProperty(Material self, Vector2 from, Vector2 to, float duration);
        object TweenAnything<S, T, V>(T self, TweenGetAndSet<S, T, V> gs, float duration) where S : TweenGetAndSet<S, T, V> where T : Object;
    }

    internal class TweenKit : ITweenKit
    {

        bool ITweenKit.IsTweening(object self)
        {
            return ((ZTweener)self).IsTweening();
        }

        object ITweenKit.SetTag(object self, object tag)
        {
            return ((ZTweener)self).SetTag(tag);
        }

        object ITweenKit.SetTimeScale(object self, float timeScale)
        {
            throw new System.NotImplementedException();
        }

        object ITweenKit.SetUpdate(object self, UpdateType updateType, bool ignoreTimeScale)
        {
            return ((ZTweener)self).SetUpdate(updateType, ignoreTimeScale);
        }

        object ITweenKit.DelayFor(object self, float time)
        {
            return ((ZTweener)self).DelayFor(time);
        }

        object ITweenKit.LoopFor(object self, int loops, LoopType loopType)
        {
            return ((ZTweener)self).LoopFor(loops, loopType);
        }

        object ITweenKit.EaseBy(object self, Ease ease)
        {
            return ((ZTweener)self).EaseBy(ease);
        }

        object ITweenKit.StartFrom(object self, object from)
        {
            return ((ZTweener)self).StartFrom(from);
        }

        object ITweenKit.EndAt(object self, object at)
        {
            return ((ZTweener)self).EndAt(at);
        }

        object ITweenKit.StartWith(object self, CallbackOnUpdate onStart)
        {
            return ((ZTweener)self).StartWith(onStart);
        }

        object ITweenKit.UpdateWith(object self, CallbackOnUpdate onUpdate)
        {
            return ((ZTweener)self).UpdateWith(onUpdate);
        }

        object ITweenKit.CompleteWith(object self, CallbackOnComplete onComplete)
        {
            return ((ZTweener)self).CompleteWith(onComplete);
        }

        object ITweenKit.Stop(object self, bool complete)
        {
            return ((ZTweener)self).Stop(complete);
        }

        void ITweenKit.Init()
        {
            
        }

        int ITweenKit.Finish(object tarOrTag, bool complete)
        {
            return ZTweenMgr.Instance.Stop(tarOrTag, complete);
        }

        object ITweenKit.TweenPosition(Transform self, Vector3 from, Vector3 to, float duration, Space space)
        {
            var gs = TransformPositionGetAndSet.Get(from, to);
            gs.space = space;
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter() { duration = duration, });
        }

        object ITweenKit.TweenRotation(Transform self, Vector3 from, Vector3 to, float duration, Space space, RotateMode mode)
        {
            var gs = TransformRotationGetAndSet.Get(Quaternion.Euler(from), Quaternion.Euler(to));
            gs.space = space;
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter() { duration = duration, });
        }

        object ITweenKit.TweenScaling(Transform self, Vector3 from, Vector3 to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, TransformScaleGetAndSet.Get(from, to), new TweenParameter() {
                duration = duration,
            });
        }

        object ITweenKit.TweenAnchoredPosition(RectTransform self, Vector3 from, Vector3 to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, RectTransformAnchoredPosGetAndSet.Get(from, to), new TweenParameter() {
                duration = duration,
            });
        }

        object ITweenKit.TweenSizeDelta(RectTransform self, Vector2 from, Vector2 to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, RectTransformSizeDeltaGetAndSet.Get(from, to), new TweenParameter() {
                duration = duration,
            });
        }

        object ITweenKit.TweenAlpha(CanvasGroup self, float from, float to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, CanvasGroupAlphaGetAndSet.Get(from, to), new TweenParameter() {
                duration = duration,
            });
        }

        object ITweenKit.TweenAlpha(Graphic self, float from, float to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, UIGraphicAlphaGetAndSet.Get(from, to), new TweenParameter() {
                duration = duration,
            });
        }

        object ITweenKit.TweenColor(Graphic self, Color from, Color to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, UIGraphicColorGetAndSet.Get(from, to), new TweenParameter() {
                duration = duration,
            });
        }

        object ITweenKit.TweenFillAmount(Image self, float from, float to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, UIImageFillAmountGetAndSet.Get(from, to), new TweenParameter() {
                duration = duration,
            });
        }

        object ITweenKit.TweenString(Text self, string from, string to, float duration)
        {
            throw new System.NotImplementedException();
        }

        object ITweenKit.TweenMaterialProperty(Material self, Vector2 from, Vector2 to, float duration)
        {
            throw new System.NotImplementedException();
        }

        object ITweenKit.TweenAnything<S, T, V>(T self, TweenGetAndSet<S, T, V> gs, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter() { duration = duration });
        }

    }
}

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
        object PlayForward(object self, bool forward);
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
        object TweenAlpha(CanvasGroup self, float from, float to, float duration);
        object TweenAlpha(Graphic self, float from, float to, float duration);
        object TweenRange(Image self, float from, float to, float duration);
        object TweenRange(Renderer self, float from, float to, float duration, int propertyId);
        object TweenSizeDelta(RectTransform self, Vector2 from, Vector2 to, float duration);
        object TweenPosition(Transform self, Vector3 from, Vector3 to, float duration, Space space = Space.Self);
        object TweenEulerAngles(Transform self, Vector3 from, Vector3 to, float duration, Space space = Space.Self);
        object TweenRotation(Transform self, Vector3 from, Vector3 to, float duration, Space space = Space.Self, RotateMode mode = RotateMode.Fast);
        object TweenScaling(Transform self, Vector3 from, Vector3 to, float duration);
        object TweenAnchoredPosition(RectTransform self, Vector3 from, Vector3 to, float duration);
        object TweenVector(Renderer self, Vector4 from, Vector4 to, float duration, int propertyId);
        object TweenColor(Graphic self, Color from, Color to, float duration);
        object TweenColor(Renderer self, Color from, Color to, float duration, int propertyId);
        object TweenString(Text self, string from, string to, float duration);
        object TweenAny(object self, TweenGetValue<float> getter, TweenSetValue<float> setter, float from, float to, float duration);
        object TweenAny(object self, TweenGetValue<Vector2> getter, TweenSetValue<Vector2> setter, Vector2 from, Vector2 to, float duration);
        object TweenAny(object self, TweenGetValue<Vector3> getter, TweenSetValue<Vector3> setter, Vector3 from, Vector3 to, float duration);
        object TweenAny(object self, TweenGetValue<Vector4> getter, TweenSetValue<Vector4> setter, Vector4 from, Vector4 to, float duration);
        object TweenAny(object self, TweenGetValue<Color> getter, TweenSetValue<Color> setter, Color from, Color to, float duration);
    }
}

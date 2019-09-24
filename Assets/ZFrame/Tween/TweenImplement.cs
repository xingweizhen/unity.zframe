using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.Tween
{
    public delegate T TweenGetValue<out T>();
    public delegate void TweenSetValue<in T>(T value);

#if !ZFRAME_USE_THIRD_PART_TWEEN
    [System.Serializable]
    public struct TweenParameter
    {
        public float delay;
        public float duration;
        public Ease ease;
        public int loops;
        public LoopType loopType;
        public UpdateType updateType;
        public bool ignoreTimescale;

        public TweenParameter(float duration)
        {
            this = Default;
            this.duration = duration;
        }

        public static readonly TweenParameter Default = new TweenParameter() {
            delay = 0, duration = 1,
            ease = Ease.Linear,
            loops = 1, loopType = LoopType.Restart,
            updateType = UpdateType.Normal, ignoreTimescale = false,
        };
    }

    public abstract class TweenGetAndSet
    {
        //private static Dictionary<System.Type, System.Delegate> s_Type2LerpUnclamped = new Dictionary<System.Type, System.Delegate> {
        //    { typeof(float), new System.Func<float, float, float, float>(Mathf.LerpUnclamped) },
        //    { typeof(Vector2), new System.Func<Vector2, Vector2, float, Vector2>(Vector2.LerpUnclamped) },
        //    { typeof(Vector3), new System.Func<Vector3, Vector3, float, Vector3>(Vector3.LerpUnclamped) },
        //    { typeof(Quaternion), new System.Func<Quaternion, Quaternion, float, Quaternion>(Quaternion.LerpUnclamped) },
        //};

        public abstract void Evaluate(object target, float t);
    }

    public abstract class TweenGetAndSet<S, T, V> : TweenGetAndSet where S : TweenGetAndSet<S, T, V>, new()
    {
        protected TweenGetAndSet() { }

        public V from, to;

        // public abstract V GetValue();

        // public abstract void SetValue(V value);

        public static S Get(V from, V to)
        {
            return new S { from = from, to = to };
        }
    }

    public sealed class TransformPositionGetAndSet : TweenGetAndSet<TransformPositionGetAndSet, Transform, Vector3>
    {
        public Space space;

        public override void Evaluate(object target, float t)
        {
            //var value = from + (to - from) * t;
            var value = Vector3.LerpUnclamped(from, to, t);
            switch (space) {
                case Space.Self:
                    ((Transform)target).localPosition = value;
                    break;
                case Space.World:
                    ((Transform)target).position = value;
                    break;
            }            
        }
    }

    public sealed class TransformEulerAnglesGetAndSet : TweenGetAndSet<TransformEulerAnglesGetAndSet, Transform, Vector3>
    {
        public Space space;

        public override void Evaluate(object target, float t)
        {
            //var value = from + (to - from) * t;
            var value = Vector3.LerpUnclamped(from, to, t);
            switch (space) {
                case Space.Self:
                    ((Transform)target).localEulerAngles = value;
                    break;
                case Space.World:
                    ((Transform)target).eulerAngles = value;
                    break;
            }
        }
    }

    public sealed class TransformRotationGetAndSet : TweenGetAndSet<TransformRotationGetAndSet, Transform, Quaternion>
    {
        public Space space;
        public RotateMode mode;

        public override void Evaluate(object target, float t)
        {
            var value = Quaternion.LerpUnclamped(from, to, t);
            switch (space) {
                case Space.Self:
                    ((Transform)target).localRotation = value;
                    break;
                case Space.World:
                    ((Transform)target).rotation = value;
                    break;
            }
        }
    }

    public sealed class TransformScaleGetAndSet : TweenGetAndSet<TransformScaleGetAndSet, Transform, Vector3>
    {
        public override void Evaluate(object target, float t)
        {
            var value = Vector3.LerpUnclamped(from, to, t);
            ((Transform)target).localScale = value;
        }
    }

    public sealed class RectTransformAnchoredPosGetAndSet : TweenGetAndSet<RectTransformAnchoredPosGetAndSet, RectTransform, Vector3>
    {
        public override void Evaluate(object target, float t)
        {
            var value = Vector3.LerpUnclamped(from, to, t);
            ((RectTransform)target).anchoredPosition3D = value;
        }
    }

    public sealed class RectTransformSizeDeltaGetAndSet : TweenGetAndSet<RectTransformSizeDeltaGetAndSet, RectTransform, Vector2>
    {
        public override void Evaluate(object target, float t)
        {
            var value = Vector2.LerpUnclamped(from, to, t);
            ((RectTransform)target).sizeDelta = value;
        }
    }

    public sealed class CanvasGroupAlphaGetAndSet : TweenGetAndSet<CanvasGroupAlphaGetAndSet, CanvasGroup, float>
    {
        public override void Evaluate(object target, float t)
        {
            t = from + (to - from) * t;
            ((CanvasGroup)target).alpha = t;
        }
    }

    public sealed class UIGraphicAlphaGetAndSet : TweenGetAndSet<UIGraphicAlphaGetAndSet, Graphic, float>
    {
        public override void Evaluate(object target, float t)
        {
            var value = Mathf.LerpUnclamped(from, to, t);
            var color = ((Graphic)target).color;
            color.a = value;
            ((Graphic)target).color = color;
        }
    }

    public sealed class UIGraphicColorGetAndSet : TweenGetAndSet<UIGraphicColorGetAndSet, Graphic, Color>
    {
        public override void Evaluate(object target, float t)
        {
            var value = Color.LerpUnclamped(from, to, t);
            ((Graphic)target).color = value;
        }
    }

    public sealed class UIImageFillAmountGetAndSet : TweenGetAndSet<UIImageFillAmountGetAndSet, Image, float>
    {
        public override void Evaluate(object target, float t)
        {
            var value = Mathf.LerpUnclamped(from, to, t);
            ((Image)target).fillAmount = value;
        }
    }

    public abstract class MaterialPropertyGetAndSet<S, V> : TweenGetAndSet<S, Renderer, V> where S : TweenGetAndSet<S, Renderer, V>, new()
    {
        protected static MaterialPropertyBlock s_Block = new MaterialPropertyBlock();
        protected int m_PropId;
        public void SetPropertyName(string propertyName)
        {
            m_PropId = Shader.PropertyToID(propertyName);
        }
    }

    public sealed class MaterialPropertyGetAndSetFloat : MaterialPropertyGetAndSet<MaterialPropertyGetAndSetFloat, float>
    {
        public override void Evaluate(object target, float t)
        {
            var value = Mathf.LerpUnclamped(from, to, t);
            var rdr = (Renderer)target;
            rdr.GetPropertyBlock(s_Block);
            s_Block.SetFloat(m_PropId, value);
            rdr.SetPropertyBlock(s_Block);
        }
    }

    public sealed class MaterialPropertyGetAndSetVector : MaterialPropertyGetAndSet<MaterialPropertyGetAndSetVector, Vector4>
    {
        public override void Evaluate(object target, float t)
        {
            var value = Vector4.LerpUnclamped(from, to, t);
            var rdr = (Renderer)target;
            rdr.GetPropertyBlock(s_Block);
            s_Block.SetVector(m_PropId, value);
            rdr.SetPropertyBlock(s_Block);
        }
    }

    public sealed class MaterialPropertyGetAndSetColor : MaterialPropertyGetAndSet<MaterialPropertyGetAndSetColor, Color>
    {
        public override void Evaluate(object target, float t)
        {
            var value = Color.LerpUnclamped(from, to, t);
            var rdr = (Renderer)target;
            rdr.GetPropertyBlock(s_Block);
            s_Block.SetColor(m_PropId, value);
            rdr.SetPropertyBlock(s_Block);
        }
    }

    public abstract class UserDefineGetAndSet<S, V> : TweenGetAndSet<S, object, V> where S : TweenGetAndSet<S, object, V>, new()
    {
        public TweenSetValue<V> setter;
        public TweenGetValue<V> getter;
    }

    public sealed class UserDefineGetAndSetFloat : UserDefineGetAndSet<UserDefineGetAndSetFloat, float>
    {
        public override void Evaluate(object target, float t)
        {
            setter.Invoke(Mathf.LerpUnclamped(from, to, t));
        }
    }

    public sealed class UserDefineGetAndSetVector2 : UserDefineGetAndSet<UserDefineGetAndSetVector2, Vector2>
    {
        public override void Evaluate(object target, float t)
        {
            setter.Invoke(Vector2.LerpUnclamped(from, to, t));
        }
    }

    public sealed class UserDefineGetAndSetVector3 : UserDefineGetAndSet<UserDefineGetAndSetVector3, Vector3>
    {
        public override void Evaluate(object target, float t)
        {
            setter.Invoke(Vector3.LerpUnclamped(from, to, t));
        }
    }

    public sealed class UserDefineGetAndSetVector4 : UserDefineGetAndSet<UserDefineGetAndSetVector4, Vector4>
    {
        public override void Evaluate(object target, float t)
        {
            setter.Invoke(Vector4.LerpUnclamped(from, to, t));
        }
    }

    public sealed class UserDefineGetAndSetColor : UserDefineGetAndSet<UserDefineGetAndSetColor, Color>
    {
        public override void Evaluate(object target, float t)
        {
            setter.Invoke(Color.LerpUnclamped(from, to, t));
        }
    }

    public partial class ZTweener
    {
        public bool backward;

        private TweenGetAndSet m_Accessor;
        private TweenParameter m_Parameter;
        private float m_Time;        
        private bool m_Started;

        private CallbackOnUpdate m_OnStart, m_OnUpdate;
        private CallbackOnComplete m_OnComplete;

        public void Init(object target, TweenGetAndSet gs, TweenParameter param)
        {
            this.target = target;
            m_Accessor = gs;
            m_Parameter = param;            
            m_Started = false;
        }

        public void Recycle()
        {
            target = null;
            tag = null;
            m_Accessor = null;
            m_OnStart = null;
            m_OnUpdate = null;
            m_OnComplete = null;
        }

        public bool UpdateValue(UpdateType updateType)
        {
            if (m_Parameter.updateType != updateType) return false;

            if (!m_Started) {
                m_Time = m_Parameter.ignoreTimescale ? Time.unscaledTime : Time.time;
                m_Time += m_Parameter.delay;

                m_Started = true;
                if (m_OnStart != null) m_OnStart.Invoke(this);
            }

            var time = m_Parameter.ignoreTimescale ? Time.unscaledTime : Time.time;
            var pass = time - m_Time;
            if (pass < 0) return false;

            pass /= m_Parameter.duration;
            var loop = (int)pass;
            if (loop > 0) {
                pass = backward ? 0 : 1;
                m_Time = time;

                switch (m_Parameter.loopType) {
                    case LoopType.Yoyo: backward = !backward;break;
                    case LoopType.Incremental: /* TODO */ break;
                }

                if (m_Parameter.loops > 0) {
                    m_Parameter.loops -= loop;
                    if (m_Parameter.loops < 0) m_Parameter.loops = 0;
                }
            } else {
                pass -= loop;
                if (backward) pass = 1 - pass;
            }

            pass = EaseAlgorithm[(int)m_Parameter.ease](0, 1, pass);
            m_Accessor.Evaluate(target, pass);
            if (m_OnUpdate != null) m_OnUpdate.Invoke(this);

            if (m_Parameter.loops == 0) {
                if (m_OnComplete != null) m_OnComplete.Invoke(this);
                return true;
            }

            return false;
        }
    }

    internal class TweenKit : ITweenKit
    {
        void ITweenKit.Init()
        {

        }

        bool ITweenKit.IsTweening(object self)
        {
            return ((ZTweener)self).IsTweening();
        }

        int ITweenKit.Finish(object tarOrTag, bool complete)
        {
            return ZTweenMgr.Instance.Stop(tarOrTag, complete);
        }

        object ITweenKit.PlayForward(object self, bool forward)
        {
            ((ZTweener)self).backward = !forward;
            return self;
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

        object ITweenKit.TweenPosition(Transform self, Vector3 from, Vector3 to, float duration, Space space)
        {
            var gs = TransformPositionGetAndSet.Get(from, to);
            gs.space = space;
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter(duration));
        }

        object ITweenKit.TweenEulerAngles(Transform self, Vector3 from, Vector3 to, float duration, Space space)
        {
            var gs = TransformEulerAnglesGetAndSet.Get(from, to);
            gs.space = space;
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter(duration));
        }

        object ITweenKit.TweenRotation(Transform self, Vector3 from, Vector3 to, float duration, Space space, RotateMode mode)
        {
            var gs = TransformRotationGetAndSet.Get(Quaternion.Euler(from), Quaternion.Euler(to));
            gs.space = space;
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter(duration));
        }

        object ITweenKit.TweenScaling(Transform self, Vector3 from, Vector3 to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, TransformScaleGetAndSet.Get(from, to), new TweenParameter(duration));
        }

        object ITweenKit.TweenAnchoredPosition(RectTransform self, Vector3 from, Vector3 to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, RectTransformAnchoredPosGetAndSet.Get(from, to), new TweenParameter(duration));
        }

        object ITweenKit.TweenSizeDelta(RectTransform self, Vector2 from, Vector2 to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, RectTransformSizeDeltaGetAndSet.Get(from, to), new TweenParameter(duration));
        }

        object ITweenKit.TweenAlpha(CanvasGroup self, float from, float to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, CanvasGroupAlphaGetAndSet.Get(from, to), new TweenParameter(duration));
        }

        object ITweenKit.TweenAlpha(Graphic self, float from, float to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, UIGraphicAlphaGetAndSet.Get(from, to), new TweenParameter(duration));
        }

        object ITweenKit.TweenColor(Graphic self, Color from, Color to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, UIGraphicColorGetAndSet.Get(from, to), new TweenParameter(duration));
        }

        object ITweenKit.TweenRange(Image self, float from, float to, float duration)
        {
            return ZTweenMgr.Instance.Begin(self, UIImageFillAmountGetAndSet.Get(from, to), new TweenParameter(duration));
        }

        object ITweenKit.TweenString(Text self, string from, string to, float duration)
        {
            throw new System.NotImplementedException();
        }

        object ITweenKit.TweenAny(object self, TweenGetValue<float> getter, TweenSetValue<float> setter, float from, float to, float duration)
        {
            var gs = UserDefineGetAndSetFloat.Get(from, to);
            gs.setter = setter;
            gs.getter = getter;
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter(duration));
        }

        object ITweenKit.TweenAny(object self, TweenGetValue<Vector2> getter, TweenSetValue<Vector2> setter, Vector2 from, Vector2 to, float duration)
        {
            var gs = UserDefineGetAndSetVector2.Get(from, to);
            gs.setter = setter;
            gs.getter = getter;
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter(duration));
        }

        object ITweenKit.TweenAny(object self, TweenGetValue<Vector3> getter, TweenSetValue<Vector3> setter, Vector3 from, Vector3 to, float duration)
        {
            var gs = UserDefineGetAndSetVector3.Get(from, to);
            gs.setter = setter;
            gs.getter = getter;
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter(duration));
        }

        object ITweenKit.TweenAny(object self, TweenGetValue<Vector4> getter, TweenSetValue<Vector4> setter, Vector4 from, Vector4 to, float duration)
        {
            var gs = UserDefineGetAndSetVector4.Get(from, to);
            gs.setter = setter;
            gs.getter = getter;
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter(duration));
        }

        object ITweenKit.TweenAny(object self, TweenGetValue<Color> getter, TweenSetValue<Color> setter, Color from, Color to, float duration)
        {
            var gs = UserDefineGetAndSetColor.Get(from, to);
            gs.setter = setter;
            gs.getter = getter;
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter(duration));
        }

        object ITweenKit.TweenRange(Renderer self, float from, float to, float duration, string propertyName)
        {
            var gs = MaterialPropertyGetAndSetFloat.Get(from, to);
            gs.SetPropertyName(propertyName);
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter(duration));
        }

        object ITweenKit.TweenVector(Renderer self, Vector4 from, Vector4 to, float duration, string propertyName)
        {
            var gs = MaterialPropertyGetAndSetVector.Get(from, to);
            gs.SetPropertyName(propertyName);
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter(duration));
        }

        object ITweenKit.TweenColor(Renderer self, Color from, Color to, float duration, string propertyName)
        {
            var gs = MaterialPropertyGetAndSetColor.Get(from, to);
            gs.SetPropertyName(propertyName);
            return ZTweenMgr.Instance.Begin(self, gs, new TweenParameter(duration));
        }
    }
#endif
}
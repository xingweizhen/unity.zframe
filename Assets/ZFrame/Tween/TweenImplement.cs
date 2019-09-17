using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
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

    public abstract class TweenGetAndSet<S, T, V> : TweenGetAndSet where S :  TweenGetAndSet<S, T, V> where T : Object
    {
        protected TweenGetAndSet() { }

        public V from, to;

        // public abstract V GetValue();

        // public abstract void SetValue(V value);

        public static S Get(V from, V to)
        {
            var obj = (S)System.Activator.CreateInstance(typeof(S));
            obj.from = from;
            obj.to = to;
            return obj;
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

    public sealed class UIGraphicAlphaGetAndSet : TweenGetAndSet<UIGraphicAlphaGetAndSet, UnityEngine.UI.Graphic, float>
    {
        public override void Evaluate(object target, float t)
        {
            var value = Mathf.LerpUnclamped(from, to, t);
            var color = ((UnityEngine.UI.Graphic)target).color;
            color.a = value;
            ((UnityEngine.UI.Graphic)target).color = color;
        }
    }

    public sealed class UIGraphicColorGetAndSet : TweenGetAndSet<UIGraphicColorGetAndSet, UnityEngine.UI.Graphic, Color>
    {
        public override void Evaluate(object target, float t)
        {
            var value = Color.LerpUnclamped(from, to, t);
            ((UnityEngine.UI.Graphic)target).color = value;
        }
    }

    public sealed class UIImageFillAmountGetAndSet : TweenGetAndSet<UIImageFillAmountGetAndSet, UnityEngine.UI.Image, float>
    {
        public override void Evaluate(object target, float t)
        {
            var value = Mathf.LerpUnclamped(from, to, t);
            ((UnityEngine.UI.Image)target).fillAmount = value;
        }
    }

    public sealed class UserDefineGetAndSet<V> : TweenGetAndSet<UserDefineGetAndSet<V>, Object, V>
    {
        public System.Action<V> setter;
        public System.Func<V> getter;

        public override void Evaluate(object target, float t)
        {
            
        }
    }

    public partial class ZTweener
    {
        private TweenGetAndSet m_Accessor;
        private TweenParameter m_Parameter;
        private float m_Time;
        private bool m_Invert;
        private bool m_Started;

        private CallbackOnUpdate m_OnStart, m_OnUpdate;
        private CallbackOnComplete m_OnComplete;

        public void Init(object target, TweenGetAndSet gs, TweenParameter param)
        {
            this.target = target;
            m_Accessor = gs;
            m_Parameter = param;
            m_Time = param.ignoreTimescale ? Time.unscaledTime : Time.time;
            m_Time += param.delay;
            m_Started = false;
        }

        public void Recycle()
        {
            m_OnStart = null;
            m_OnUpdate = null;
            m_OnComplete = null;
        }

        public bool UpdateValue(UpdateType updateType)
        {
            if (m_Parameter.updateType != updateType) return false;

            var time = m_Parameter.ignoreTimescale ? Time.unscaledTime : Time.time;
            var pass = time - m_Time;
            if (pass < 0) return false;

            pass /= m_Parameter.duration;
            var loop = (int)pass;
            if (loop > 0) {
                pass = m_Invert ? 0 : 1;
                m_Time = time;

                switch (m_Parameter.loopType) {
                    case LoopType.Yoyo: m_Invert = !m_Invert;break;
                    case LoopType.Incremental: /* TODO */ break;
                }

                if (m_Parameter.loops > 0) {
                    m_Parameter.loops -= loop;
                    if (m_Parameter.loops < 0) m_Parameter.loops = 0;
                }
            } else {
                pass -= loop;
                if (m_Invert) pass = 1 - pass;
            }

            if (!m_Started) {
                m_Started = true;
                if (m_OnStart != null) m_OnStart.Invoke(this);
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
}
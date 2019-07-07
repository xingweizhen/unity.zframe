using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    [System.Serializable]
    public class TweenParameter
    {
        public float delay = 0;
        public float duration = 1;
        public Ease ease = Ease.Linear;
        public int loops = 1;
        public LoopType loopType = LoopType.Restart;
        public UpdateType updateType = UpdateType.Normal;
        public bool ignoreTimescale = false;
    }

    public interface ITweenGetAndSet<T, V> where T : Object
    {
        V GetValue(T obj);
        void SetValue(T obj, V value);
    }

    public abstract class TweenGetAndSet<S, T, V> : ITweenGetAndSet<T, V> where S : TweenGetAndSet<S, T, V> where T : Object
    {
        public static S Instance { get; protected set; }
        public abstract V GetValue(T obj);
        public abstract void SetValue(T obj, V value);
    }

    public abstract class Tweener<T, V> : ZTweener where T : Object
    {
        public T target;
        public V from, to;
        public abstract ITweenGetAndSet<T, V> accessor { get; }
    }

    public class TweenCanvasGroupAlpha : Tweener<CanvasGroup, float>
    {
        internal class GetAndSet : TweenGetAndSet<GetAndSet, CanvasGroup, float>
        {
            static GetAndSet() { Instance = new GetAndSet(); }
            private GetAndSet() { }

            public override float GetValue(CanvasGroup obj)
            {
                return obj.alpha;
            }

            public override void SetValue(CanvasGroup obj, float value)
            {
                obj.alpha = value;
            }
        }
        
        public override ITweenGetAndSet<CanvasGroup, float> accessor {
            get {
                return GetAndSet.Instance;
            }
        }
    }

    public class TweenCallback
    {

    }
}
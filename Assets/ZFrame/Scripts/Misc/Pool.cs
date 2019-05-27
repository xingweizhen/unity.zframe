using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_5
using Assert = UnityEngine.Assertions.Assert;
#else
using Assert = ZFrame.Assertions.Assert;
#endif
namespace ZFrame
{
    public abstract class PoolBase<T>
    {
        protected abstract T New();

        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly System.Action<T> m_ActionOnGet;
        private readonly System.Action<T> m_ActionOnRelease;
        private readonly int m_Limit;

        public int countAll { get; private set; }
        public int countActive { get { return countAll - countInactive; } }
        public int countInactive { get {  lock (m_Stack) return m_Stack.Count; } }

        public PoolBase(System.Action<T> actionOnGet, System.Action<T> actionOnRelease, int limit = 0)
        {
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
            m_Limit = limit;
        }

        public T Get()
        {
            lock (m_Stack) {
                T element;
                if (m_Stack.Count == 0) {
                    element = New();
                    countAll++;
                    LogMgr.I("Pool++ {0}", this);
                } else {
                    element = m_Stack.Pop();
                    //LogMgr.D("Pool== {0}", info);
                }
                if (m_ActionOnGet != null)
                    m_ActionOnGet(element);
                return element;
            }
        }

        public void Release(T element)
        {
            lock (m_Stack) {
                // Pool is full.
                if (m_Limit > 0) {
                    if (m_Stack.Count == m_Limit) {
                        countAll--;
                        return;
                    }
                    Assert.IsTrue(m_Stack.Count < m_Limit);
                }

                if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element)) {
                    Debug.LogErrorFormat(
                        "Internal error. Trying to destroy {0} that is already released to pool.", element);
                }
                if (m_ActionOnRelease != null)
                    m_ActionOnRelease(element);
                m_Stack.Push(element);
            }
        }

        public void Clear()
        {
            lock (m_Stack) m_Stack.Clear();
            countAll = 0;
        }

        public override string ToString()
        {
            return string.Format("<{0}>: {1}/{2}", typeof(T), countActive, countAll);
        }
    }

    public class Pool<T> : PoolBase<T> where T : new()
    {
        public Pool(System.Action<T> actionOnGet, System.Action<T> actionOnRelease, int limit = 0)
            : base(actionOnGet, actionOnRelease, limit)
        {

        }

        protected override T New()
        {
            return new T();
        }
    }

    public class ObjPool<T> : PoolBase<T> where T : Object
    {
        private T m_Obj;
        public ObjPool(T Obj, System.Action<T> actionOnGet, System.Action<T> actionOnRelease, int limit = 0)
            : base(actionOnGet, actionOnRelease, limit)
        {
            m_Obj = Obj;
        }

        protected override T New()
        {
            return Object.Instantiate(m_Obj);
        }

        public override string ToString()
        {
            return string.Format("<{0}>: {1}/{2}", m_Obj, countActive, countAll);
        }
    }

    public static class ListPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly Pool<List<T>> s_ListPool = new Pool<List<T>>(null, l => l.Clear());

        public static List<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }

        public static void Clear()
        {
            s_ListPool.Clear();
        }

        public static string info {
            get { return s_ListPool.ToString(); }
        }
    }

    public class Poolable<T> where T : Poolable<T>, new()
    {
        protected static readonly Pool<T> _Pool = new Pool<T>(t => t.OnGet(), t=> t.OnRelease());

        public static T Get()
        {
            return _Pool.Get();
        }

        public static void Release(T element)
        {
            _Pool.Release(element);
        }
        
        protected virtual void OnGet() {}
        protected virtual void OnRelease() {}

        public static string info {
            get { return _Pool.ToString(); }
        }
    }
}

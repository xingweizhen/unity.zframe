using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.Asset
{    
    /// <summary>
    /// 非游戏主线程下回收Unity对象
    /// </summary>
    public class ObjectRecycler : MonoSingleton<ObjectRecycler>
    {
        public delegate void DelegateRecycle(Object obj, float delay = 0);

        private struct RecycleTask
        {
            private DelegateRecycle m_Method;
            private Object m_Obj;
            private float m_Delay;
            public RecycleTask(Object obj, float delay, DelegateRecycle method)
            {
                m_Obj = obj;
                m_Delay = delay;
                m_Method = method;
            }
            public void Recyle()
            {
                if (m_Method != null) {
                    m_Method.Invoke(m_Obj, m_Delay);
                } else {
                    Destroy(m_Obj, m_Delay);
                }
            }
        }

        private Queue<RecycleTask> m_Que = new Queue<RecycleTask>();

        private void Update()
        {
            while (m_Que.Count > 0) {
                m_Que.Dequeue().Recyle();
            }
        }

        public static void Recycle(Object obj, float delay, DelegateRecycle method)
        {
            Instance.m_Que.Enqueue(new RecycleTask(obj, delay, method));
        }
    }
}


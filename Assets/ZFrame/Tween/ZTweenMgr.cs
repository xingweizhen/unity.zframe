using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    internal class ZTweenMgr : MonoBehaviour
    {
        private static ZTweenMgr m_Inst;
        public static ZTweenMgr Instance {
            get {
                if (m_Inst == null) {
                    m_Inst = new GameObject("[ZTweenMgr]").AddComponent(typeof(ZTweenMgr)) as ZTweenMgr;
                }
                return m_Inst;
            }
        }
        
        private Stack<ZTweener> m_Pool = new Stack<ZTweener>();
        private List<ZTweener> m_Tweens = new List<ZTweener>();

        public ZTweener Begin(object target, TweenGetAndSet gs, TweenParameter parameter)
        {
            var tweener = m_Pool.Count > 0 ? m_Pool.Pop() : new ZTweener();            
            tweener.Init(target, gs, parameter);
            m_Tweens.Add(tweener);
            return tweener;
        }

        private void UpdateValue(float t, UpdateType updateType)
        {
            for (int i = m_Tweens.Count - 1; i >= 0; --i) { 
                if (m_Tweens[i].UpdateValue(updateType)) {
                    m_Pool.Push(m_Tweens[i]);
                    m_Tweens.RemoveAt(i);
                }
            }
        }

        private void Update()
        {
            UpdateValue(Time.time, UpdateType.Normal);
        }

        private void LateUpdate()
        {
            UpdateValue(Time.time, UpdateType.Late);
        }

        private void FixedUpdate()
        {
            UpdateValue(Time.time, UpdateType.Fixed);
        }
    }
}

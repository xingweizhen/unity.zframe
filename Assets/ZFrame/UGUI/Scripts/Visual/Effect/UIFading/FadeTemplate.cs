using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI
{
    using Tween;

    public abstract class FadeTemplate<T> : Fading
    {
        [SerializeField]protected T m_Source;
        [SerializeField]protected T m_Destina;

        [Header("勾选使用当前状态代替Source")][SerializeField] protected bool m_bRestartUseOriginalPos;

        public void SetSource(T src) { m_Source = src; }
        public void SetDestina(T dst) { m_Destina = dst; }

        public virtual object source { get { return m_Source; } }
        public virtual object destina { get { return m_Destina; } }

        protected override void SetRestart(bool forward)
        {
            m_Tweener.StartFrom(forward ? m_Source : m_Destina);
        }
    }    
}
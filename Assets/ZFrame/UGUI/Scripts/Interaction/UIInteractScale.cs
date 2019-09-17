using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI
{
    using Tween;
    public class UIInteractScale : InteractFade
    {
        private const float m_Origin = 1f;

        [SerializeField]
        protected GameObject m_Target;
        [SerializeField]
        protected float m_Scale = 0.9f;
        [SerializeField]
        protected float m_Duration = 0.1f;

        public override float duration { get { return m_Duration; } }
        public override GameObject target { get { return m_Target ? m_Target : gameObject ; } }

        protected override void Restart()
        {
            transform.localScale = Vector3.one * m_Origin;
        }

        protected override void SetRestart(bool forward)
        {
            var tweenTar = forward ? m_Scale : m_Origin;
            m_Tweener.StartFrom(Vector3.one * tweenTar);
        }

        protected override object AnimateFade(bool forward)
        {
            var tweenTar = forward ? m_Scale : m_Origin;

            var trans = target.GetComponent<Transform>();
            return trans.TweenScaling(Vector3.one * tweenTar, duration);
        }
    }
}

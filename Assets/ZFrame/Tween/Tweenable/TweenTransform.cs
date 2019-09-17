using UnityEngine;
using System.Collections;

namespace ZFrame.Tween
{
    public class TransformFollowTweener : BaseTweener
    {
        private Vector3 m_From;
        private Transform m_To;
        private float m_Time, m_Dura;

        private void Setter(float t)
        {
            m_Time = t;
            transform.position = Vector3.Lerp(m_From, m_To.position, m_Time / m_Dura);
        }

        private float Getter()
        {
            return m_Time;
        }

        public override object Tween(object from, object to, float duration)
        {
            object tw = null;
            m_To = to as Transform;
            if (m_To) {
                tw = this.Tween(Getter, Setter, duration, duration);
                m_Time = 0f;
                m_Dura = duration;
                if (from is Vector3) {
                    m_From = (Vector3)from;
                    transform.position = m_From;
                } else {
                    m_From = transform.position;
                }
            }

            if (tw != null) tw.SetTag(this);

            return tw;
        }
    }
}

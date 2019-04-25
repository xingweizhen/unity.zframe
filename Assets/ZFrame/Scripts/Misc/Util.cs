using UnityEngine;
using System.Collections;

namespace ZFrame.Util
{
    [System.Serializable]
    public class TimeCounter
    {
        [SerializeField]
        private float m_TimeLimit;
        private float m_TimeCount;
        public TimeCounter(float limit)
        {
            m_TimeLimit = limit;
            m_TimeCount = 0;
        }

        public bool Count(float time)
        {
            m_TimeCount += time;
            if (m_TimeCount >= m_TimeLimit) {
                m_TimeCount = 0;
                return true;
            }

            return false;
        }
    }
}

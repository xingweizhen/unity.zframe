using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.Tween
{
    public class TweenShaking : MonoBehavior
    {
        [System.Serializable]
        private struct ShakeParam
        {
            public float delay;
            public float offset;
            public float duration;

            public override string ToString()
            {
                return string.Format(
                    "[Delay={0}; Offset={1}, Dura={2}]", delay, offset, duration);
            }
        }

        private int CompareShakeParam(ShakeParam x, ShakeParam y)
        {
            return Mathf.RoundToInt((y.offset - x.offset) * 10000);
        }

        [Description]
        private List<ShakeParam> m_Shakes = new List<ShakeParam>();

        private float m_DeltaTime { get { return Time.deltaTime; } }

        [Description]
        private Vector3 originPosition;

        public void ScheduleShake(float delay, float offset, float duration)
        {
            if (!this.enabled) {
                this.enabled = true;
            }

            var param = new ShakeParam() {
                delay = delay,
                offset = offset,
                duration = duration };

            m_Shakes.Add(param);
            m_Shakes.Sort(CompareShakeParam);
        }

        private void Update()
        {
            if (m_DeltaTime > 0 && m_Shakes.Count > 0) {
                ShakeParam param = m_Shakes[0];

                // shakeDelay可以等于0
                if (param.delay <= 0 && param.duration > 0) {
                    transform.localPosition = originPosition + Random.onUnitSphere * param.offset;
                }

                for (int i = 0; i < m_Shakes.Count; ) {
                    var shake = m_Shakes[i];
                    if (shake.delay <= 0) {
                        shake.duration -= m_DeltaTime;                        
                    } else {
                        shake.delay -= m_DeltaTime;
                    }
                    if (shake.duration > 0) {
                        m_Shakes[i++] = shake;
                    } else {
                        m_Shakes.RemoveAt(i);
                    }                    
                }
            } else {
                transform.localPosition = originPosition;
                this.enabled = false;
            }
        }
             
        private void OnEnable()
        {
            originPosition = transform.localPosition;
            m_Shakes.Clear();
        }
                
        private void OnTransformParentChanged()
        {
            this.enabled = false;
        }
    }
}

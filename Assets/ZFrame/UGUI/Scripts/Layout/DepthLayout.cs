using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace ZFrame.UGUI
{
    /// <summary>
    /// 对象的z轴数值和其离锚点的距离正比（基于系数）。
    /// </summary>
    public class DepthLayout : UIBehaviour
    {
        public Transform anchor;
        public float factor;

        private RectTransform m_Rect;
        public RectTransform rectTransform { get { if (!m_Rect) m_Rect = GetComponent<RectTransform>(); return m_Rect; } }

        // Update is called once per frame
        private void Update()
        {
            var v3 = rectTransform.position;
            v3.z = anchor.position.z;
            var offset = (v3 - anchor.position).magnitude;
            v3.z = anchor.position.z + factor * offset;
            rectTransform.position = v3;
        }
    }
}

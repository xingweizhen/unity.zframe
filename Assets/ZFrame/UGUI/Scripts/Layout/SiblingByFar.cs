using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ZFrame.UGUI
{
    /// <summary>
    /// 基于离摄像机的距离对子物体进行兄弟排序
    /// </summary>
    public class SiblingByFar : UIBehaviour, ILayoutElement
    {
        public Transform center;

        private List<Transform> m_Children = new List<Transform>();
        private long m_SortingSignature;

        Transform mTrans;
        public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

        public float flexibleHeight { get { return -1; } }

        public float flexibleWidth { get { return -1; } }

        public int layoutPriority { get { return 0; } }

        public float minHeight { get { return -1; } }

        public float minWidth { get { return -1; } }

        public float preferredHeight { get { return -1; } }

        public float preferredWidth { get { return -1; } }

        public void CalculateLayoutInputHorizontal()
        {
            CalculateLayoutInput();
        }

        public void CalculateLayoutInputVertical()
        {
            CalculateLayoutInput();
        }

        protected override void OnEnable()
        {
            CalculateLayoutInput();
        }

        private void CalculateLayoutInput()
        {
            m_Children.Clear();
            
            for (int i = 0; i < cachedTransform.childCount; ++i) {
                m_Children.Add(cachedTransform.GetChild(i));
            }
        }

        private int SortByFar(Transform a, Transform b)
        {
            var origin = center.position;
            var x = Vector3.Distance(origin, a.position);
            var y = Vector3.Distance(origin, b.position);
            var ret = (int)((x - y) * 10000);
            return -ret;
        }

        private void Update()
        {
            m_Children.Sort(SortByFar);
            m_SortingSignature = DOSibling(m_Children, m_SortingSignature);
        }

        public static long DOSibling(List<Transform> collection, long sortingSignature)
        {
            long signature = 0;
            for (int i = 0; i < collection.Count; ++i) {
                var graphic = collection[i];
                signature += graphic.GetHashCode() * (i + 1);
            }
            if (sortingSignature != signature) {
                sortingSignature = signature;
                for (int i = 0; i < collection.Count; ++i) {
                    var rect = collection[i];
                    rect.SetSiblingIndex(i);
                }
            }
            return sortingSignature;
        }
    }
}

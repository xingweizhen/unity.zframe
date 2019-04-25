using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZFrame
{
    public class SortingOrderModify : MonoBehaviour
    {
        [SerializeField] private bool m_IncludeInactive;

        [SerializeField] private string m_SortingLayer;

        [FormerlySerializedAs("m_OrderInLayer")] [SerializeField] private int m_SortingOrder;
             
        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(m_SortingLayer)) Modify();
        }

        private void Modify()
        {
            var list = ListPool<Component>.Get();
            this.GetComponentsInChildren(typeof(Renderer), list, m_IncludeInactive);
            for (var i = 0; i < list.Count; ++i) {
                var rdr = (Renderer)list[i];
                if (!string.IsNullOrEmpty(m_SortingLayer)) {
                    rdr.sortingLayerName = m_SortingLayer;
                }

                rdr.sortingOrder = m_SortingOrder;
            }

            ListPool<Component>.Release(list);
        }

        public void Modify(string sortingLayer, int sortingOrder, bool includeInactive)
        {
            m_SortingLayer = sortingLayer;
            m_SortingOrder = sortingOrder;
            m_IncludeInactive = includeInactive;
            Modify();  
        }
    }
}

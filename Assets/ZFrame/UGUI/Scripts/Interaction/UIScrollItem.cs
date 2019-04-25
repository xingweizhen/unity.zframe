using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.UGUI
{
#if UNITY_EDITOR
    public class UIScrollItem : MonoBehavior
#else
    public class UIScrollItem : MonoBehaviour
#endif
    {
        [SerializeField]
        private UIScrollView m_Scroll;

        [System.NonSerialized, Description("是否在滚动区域")]
        public bool overlap;

        private void OnEnable()
        {
            if (m_Scroll) m_Scroll.AddItem(this);
        }

        private void OnDisable()
        {
            if (m_Scroll) m_Scroll.RemoveItem(this);
        }
    }
}

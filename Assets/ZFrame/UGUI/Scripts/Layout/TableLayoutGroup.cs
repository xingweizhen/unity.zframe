using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace ZFrame.UGUI
{
    public class TableLayoutGroup : UIBehaviour, ILayoutElement, ILayoutController
    {
        public float minWidth { get { return 0; } }
        public float preferredWidth { get { return 0; } }
        public float flexibleWidth { get { return 0; } }

        public float minHeight { get { return 0; } }
        public float preferredHeight { get { return 0; } }
        public float flexibleHeight { get { return 0; } }

        public int layoutPriority { get { return 0; } }

        public void CalculateLayoutInputHorizontal()
        {
            throw new System.NotImplementedException();
        }

        public void CalculateLayoutInputVertical()
        {
            throw new System.NotImplementedException();
        }

        public void SetLayoutHorizontal()
        {
            throw new System.NotImplementedException();
        }

        public void SetLayoutVertical()
        {
            throw new System.NotImplementedException();
        }
    }
}

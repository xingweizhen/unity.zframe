using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    public class UIGridLayoutGroup : GridLayoutGroup
    {
        protected override void OnRectTransformDimensionsChange()
        {
            var cell = cellSize;
            if (startAxis == Axis.Horizontal) {
                cell.y = rectTransform.rect.height - padding.top - padding.bottom;
            } else {
                cell.x = rectTransform.rect.width - padding.left - padding.right;
            }
            cellSize = cell;
            base.OnRectTransformDimensionsChange();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            OnRectTransformDimensionsChange();
        }
#endif
    }
}

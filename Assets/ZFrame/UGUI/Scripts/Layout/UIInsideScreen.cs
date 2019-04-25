using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ZFrame.UGUI
{
    public class UIInsideScreen : UIBehaviour
    {
        protected override void OnRectTransformDimensionsChange()
        {
            if (enabled) {
                var rectTrans = transform as RectTransform;
                if (rectTrans) {
                    var pos = rectTrans.InsideScreenPosition();
                    rectTrans.anchoredPosition = pos;
                }
            }
        }
    }
}
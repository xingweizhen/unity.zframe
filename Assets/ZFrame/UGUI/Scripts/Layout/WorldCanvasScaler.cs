using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.UGUI
{
    public class WorldCanvasScaler : CanvasScaler
    {
        private const float kLogBase = 2;

        protected override void HandleWorldCanvas()
        {
            base.HandleScaleWithScreenSize();
        }
    }
}

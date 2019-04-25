using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    /// <summary>
    /// 不可见的UI控件，仅仅用来阻挡UI射线
    /// </summary>
    public sealed class UIBlock : Graphic, ICanvasRaycastFilter
    {
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            return true;
        }

        protected override void Start()
        {
#if UNITY_EDITOR
            UGUITools.AutoUIRoot(this);
#endif

            base.Start();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

        public override void SetMaterialDirty() { }
        public override void SetVerticesDirty() { }
    }
}

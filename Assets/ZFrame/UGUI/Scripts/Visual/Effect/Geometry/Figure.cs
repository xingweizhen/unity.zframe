using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    /// <summary>
    /// 画几何图形基类
    /// </summary>
    [RequireComponent(typeof(Image)), DisallowMultipleComponent]
    public abstract class Figure : BaseMeshEffect, ILayoutSelfController
    {
        protected abstract void UpdateImage();

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
#endif

        public virtual void SetDirty()
        {
            if (!IsActive()) return;
            UpdateImage();
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            UpdateImage();
        }
        
        public virtual void SetLayoutHorizontal()
        {
            
        }

        public virtual void SetLayoutVertical()
        {
            
        }
    }
}

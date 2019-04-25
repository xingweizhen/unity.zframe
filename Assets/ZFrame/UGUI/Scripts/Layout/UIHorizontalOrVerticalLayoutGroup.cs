using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    using Tween;
    
    public abstract class UIHorizontalOrVerticalLayoutGroup : HorizontalOrVerticalLayoutGroup
    {
        private const float smoothDuration = 0.5f;

        public bool autoFitSize = true;
        public Ease smoothLaying = Ease.Unset;

        protected void SetChildrenAlongAxisEx(int axis, bool isVertical)
        {
            List<Vector2> list = new List<Vector2>();
            for (int i = 0; i < rectChildren.Count; ++i) {
                list.Add(rectChildren[i].anchoredPosition);
            }

            var fitSize = axis == 0 ? preferredWidth : preferredHeight;
            float addedSize = fitSize - rectTransform.rect.width;
            if (autoFitSize) {
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, fitSize);
            }

            SetChildrenAlongAxis(axis, isVertical);

            if (Application.isPlaying) {
                if (smoothLaying != Ease.Unset) {
                    this.enabled = false;
                    var offset = axis == 0 ? addedSize * rectTransform.pivot.x : -addedSize * rectTransform.pivot.y;
                    for (int i = 0; i < rectChildren.Count; ++i) {
                        var rect = rectChildren[i];
                        var v2 = list[i];
                        var depthLayout = rect.GetComponent<DepthLayout>();
                        if (depthLayout && depthLayout.enabled) {
                            var startV2 = axis == 0 ? 
                                new Vector2(v2.x + offset, v2.y) : new Vector2(v2.x, v2.y + offset);
                            rect.TweenAnchorPos(rect.anchoredPosition, smoothDuration)
                                .StartFrom(startV2)
                                .EaseBy(smoothLaying);
                        } else {
                            var endV3 = rect.anchoredPosition3D;
                            var startV3 = axis == 0 ? 
                                new Vector3(v2.x + offset, v2.y, endV3.z) : new Vector3(v2.x, v2.y + offset, endV3.z);
                            endV3.z = 0;
                            rect.TweenAnchorPos(endV3, smoothDuration)
                                .StartFrom(startV3)
                                .EaseBy(smoothLaying);
                        }
                    }
                }
            }
        }
        
        protected override void OnDisable()
        {
            for (int i = 0; i < rectChildren.Count; ++i) {
                ZTween.Stop(rectChildren[i]);
            }
            base.OnDisable();
        }
    }
}

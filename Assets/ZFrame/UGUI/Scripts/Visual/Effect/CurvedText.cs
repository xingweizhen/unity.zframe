/// Credit Breyer
/// Sourced from - http://forum.unity3d.com/threads/scripts-useful-4-6-scripts-collection.264161/#post-1777407

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    [RequireComponent(typeof(Text), typeof(RectTransform)), DisallowMultipleComponent]
    public class CurvedText : BaseMeshEffect
    {
        public AnimationCurve curveForText = AnimationCurve.Linear(0, 0, 1, 10);
        public float curveMultiplier = 1;
        
		private RectTransform rectTrans { get { return graphic.rectTransform; } }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (curveForText[0].time != 0) {
                var tmpRect = curveForText[0];
                tmpRect.time = 0;
                curveForText.MoveKey(0, tmpRect);
            }

            if (curveForText[curveForText.length - 1].time != rectTrans.rect.width)
                OnRectTransformDimensionsChange();
        }
#endif
        protected override void Awake()
        {
            base.Awake();
            OnRectTransformDimensionsChange();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            OnRectTransformDimensionsChange();
        }

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!IsActive())
				return;

			var verts = ListPool<UIVertex>.Get();
			vh.GetUIVertexStream(verts);

			if (verts.Count > 0) {
				for (int index = 0; index < verts.Count; index++) {
					var uiVertex = verts[index];
					uiVertex.position.y += curveForText.Evaluate(rectTrans.rect.width * rectTrans.pivot.x + uiVertex.position.x) * curveMultiplier;
					verts[index] = uiVertex;
				}

				vh.Clear();
				vh.AddUIVertexTriangleStream(verts);
			}

			ListPool<UIVertex>.Release(verts);
		}

        protected override void OnRectTransformDimensionsChange()
        {
            var tmpRect = curveForText[curveForText.length - 1];
            tmpRect.time = rectTrans.rect.width;
            curveForText.MoveKey(curveForText.length - 1, tmpRect);
        }
    }
}

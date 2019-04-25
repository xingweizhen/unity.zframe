using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    [RequireComponent(typeof(Graphic)), DisallowMultipleComponent]
    public class Outline8 : Shadow
    {

        protected void Modify(List<UIVertex> verts)
        {
            Text foundtext = GetComponent<Text>();

            float best_fit_adjustment = 1f;

            if (foundtext && foundtext.resizeTextForBestFit) {
                best_fit_adjustment = (float)foundtext.cachedTextGenerator.fontSizeUsedForBestFit / (foundtext.resizeTextMaxSize - 1); //max size seems to be exclusive 
            }

            float distanceX = this.effectDistance.x * best_fit_adjustment;
            float distanceY = this.effectDistance.y * best_fit_adjustment;

            var start = 0;
            var end = verts.Count;
            ApplyShadow(verts, effectColor, start, verts.Count, distanceX, distanceY);

            start = end;
            end = verts.Count;
            ApplyShadow(verts, effectColor, start, verts.Count, distanceX, 0);

            start = end;
            end = verts.Count;
            ApplyShadow(verts, effectColor, start, verts.Count, distanceX, -distanceY);

            start = end;
            end = verts.Count;
            ApplyShadow(verts, effectColor, start, verts.Count, 0, -distanceY);

            start = end;
            end = verts.Count;
            ApplyShadow(verts, effectColor, start, verts.Count, -distanceX, -distanceY);


            start = end;
            end = verts.Count;
            ApplyShadow(verts, effectColor, start, verts.Count, -distanceX, 0);

            start = end;
            end = verts.Count;
            ApplyShadow(verts, effectColor, start, verts.Count, -distanceX, distanceY);

            start = end;
            end = verts.Count;
            ApplyShadow(verts, effectColor, start, verts.Count, 0, distanceY);
        }

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!IsActive()) return;

			var verts = ListPool<UIVertex>.Get();
			vh.GetUIVertexStream(verts);

			if (verts.Count > 0) {
				Modify(verts);

				vh.Clear();
				vh.AddUIVertexTriangleStream(verts);
			}

			ListPool<UIVertex>.Release(verts);
		}
    }
}

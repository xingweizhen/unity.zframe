using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
	public partial class Gradient : BaseMeshEffect
    {
        /// <summary>
        /// Simple
        ///  1      2|3
        /// 
        ///  0|5    4
        /// </summary>
        /// <summary>
        /// Sliced
        ///  13     50|51
        /// 
        ///  0|5	40
        /// </summary>

        private Image m_Image  {  get  { return graphic as Image; } }
        
        private void FindCorners(List<UIVertex> verts, out Vector3 bottomLeft, out Vector3 topRight)
        {
            var rectTrans = GetComponent<RectTransform>();
            bottomLeft = rectTrans.rect.min;
            topRight = rectTrans.rect.max;
        }

		private void GradientVertical(List<UIVertex> verts, Vector3 bottomLeft, Vector3 topRight)
		{
			UIVertex vert;
			var total = topRight.y - bottomLeft.y;
			for (int i = 0; i < verts.Count; i++) {
				vert = verts[i];
				if (!overwriteAllColor && vert.color != m_Image.color)
					continue;
				vert.color *= Color.Lerp(vertex2, vertex1, (vert.position.y - bottomLeft.y) / total);
				verts[i] = vert;
			}
		}

		private void GradientHorizontal(List<UIVertex> verts, Vector3 bottomLeft, Vector3 topRight)
		{
			UIVertex vert;
			var total = (topRight.x - bottomLeft.x);
			for (int i = 0; i < verts.Count; i++) {
				vert = verts[i];
				if (!overwriteAllColor && vert.color != m_Image.color)
					continue;
				vert.color *= Color.Lerp(vertex1, vertex2, (vert.position.x - bottomLeft.x) / total);
				verts[i] = vert;
			}
		}

		private void GradientDiagonalLeftToRight(List<UIVertex> verts, Vector3 bottomLeft, Vector3 topRight)
		{
			UIVertex vert;
			for (int i = 0; i < verts.Count; i++) {
				vert = verts[i];
				if (!overwriteAllColor && vert.color != m_Image.color)
					continue;
				var dFrom = Vector3.Distance(vert.position, bottomLeft);
				var dTo = Vector3.Distance(vert.position, topRight);
				vert.color *= Color.Lerp(vertex1, vertex2, dFrom / (dFrom + dTo));
				verts[i] = vert;
			}
		}

		private void GradientDiagonalRightToLeft(List<UIVertex> verts, Vector3 bottomLeft, Vector3 topRight)
		{
			UIVertex vert;
			for (int i = 0; i < verts.Count; i++) {
				vert = verts[i];
				if (!overwriteAllColor && vert.color != m_Image.color)
					continue;
				var dFrom = Vector3.Distance(vert.position, topRight);
				var dTo = Vector3.Distance(vert.position, bottomLeft);
				vert.color *= Color.Lerp(vertex1, vertex2, dFrom / (dFrom + dTo));
				verts[i] = vert;
			}
		}

        protected void ModifyImage(List<UIVertex> verts)
        {
            Vector3 bottomLeft, topRight;
            FindCorners(verts, out bottomLeft, out topRight);
            switch (gradientDir) {
				case GradientDir.Vertical: GradientVertical(verts, bottomLeft, topRight); break;
				case GradientDir.Horizontal: GradientHorizontal(verts, bottomLeft, topRight); break;
				case GradientDir.DiagonalLeftToRight: GradientDiagonalLeftToRight(verts, bottomLeft, topRight); break;
				case GradientDir.DiagonalRightToLeft: GradientDiagonalRightToLeft(verts, bottomLeft, topRight); break;
                default: break;
            }
        }
    }
}
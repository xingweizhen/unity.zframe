using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
	public class Triangle : Figure
    {
        [Range(0, 360)]
        public float angleF = 0;        
        [Range(0, 360)]
        public float angleT = 90;
        [Range(0, 1)]
        public float edgeAB = 1;
        [Range(0, 1)]
        public float edgeAC = 1;
        public Color colorA = Color.white;
        public Color colorB = Color.white;
        public Color colorC = Color.white;

        protected void Modify(List<UIVertex> verts)
        {
            for (int i = 3; i < verts.Count;) {
                verts.RemoveAt(i);
            }

            var size = graphic.rectTransform.rect.size / 2;
            var vt = verts[0];
            vt.position = Vector3.zero;
            vt.color *= colorA;
            verts[0] = vt;

            vt = verts[1];
            vt.position = Quaternion.Euler(0, 0, angleF) * new Vector3(size.y * edgeAB, 0);
            vt.color *= colorB;
            verts[1] = vt;

            vt = verts[2];
            vt.position = Quaternion.Euler(0, 0, angleT) * new Vector3(size.y * edgeAC, 0);
            vt.color *= colorC;
            verts[2] = vt;
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

		protected override void UpdateImage ()
		{
            var rect = graphic.rectTransform;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}

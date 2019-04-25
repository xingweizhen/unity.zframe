using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ZFrame.UGUI
{
    public class Segment : Figure
    {
        public float length = 100f;
        public float width = 1f;
        public int segment = 1;

        private void CopyVertexes(List<UIVertex> sourceList, List<UIVertex> destnationList, float offset)
        {
            for (int i = 0; i < sourceList.Count; ++i) {
                var vert = sourceList[i];
                var pos = vert.position;
                pos.x += offset;
                vert.position = pos;
                destnationList.Add(vert);
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;

            var verts = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(verts);

            if (verts.Count > 0) {
                var newVerts = ListPool<UIVertex>.Get();
                newVerts.AddRange(verts);

                var offset = length / segment;
                for (int i = 1; i < segment; ++i) {
                    CopyVertexes(verts, newVerts, i * offset);
                }

                vh.Clear();
                vh.AddUIVertexTriangleStream(newVerts);

                ListPool<UIVertex>.Release(newVerts);
            }
            ListPool<UIVertex>.Release(verts);
        }
        
        protected override void UpdateImage()
        {
            var rect = GetComponent<RectTransform>();
            rect.pivot = new Vector2(0, 0.5f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, length / segment);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, width);
        }
    }
}


using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public class ImageClip : BaseMeshEffect
    {
        [SerializeField]
        private Vector2Int m_Pos;

        [SerializeField]
        private Vector2Int m_Size;

        public void UpdateCrop(int x, int y, int w, int h)
        {
            m_Pos = new Vector2Int(x, y);
            m_Size = new Vector2Int(w, h);

            var image = GetComponent<Image>();
            if (image) {
                image.SetVerticesDirty();    
            }
        }

        private void SetVertUV(List<UIVertex> verts, int index, Vector2 uv)
        {
            var vert = verts[index];
            vert.uv0 = uv;
            verts[index] = vert;
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;

            var image = GetComponent<Image>();
            if (image.type != Image.Type.Simple) {

                return;
            }

            var verts = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(verts);

            if (verts.Count != 6) return;

            var spSize = image.overrideSprite.rect.size;
            var _uv = verts[2].uv0 - verts[0].uv0;
            float uvw = _uv.x, uvh = _uv.y;

            var l = m_Pos.x / spSize.x;
            var t = (spSize.y - m_Pos.y - m_Size.y) / spSize.y;
            var r = (spSize.x - m_Pos.x - m_Size.x) / spSize.x;
            var b = m_Pos.y / spSize.y;

            var left = verts[1].uv0.x + uvw * l;
            var top = verts[1].uv0.y - uvh * t;
            var right = verts[4].uv0.x - uvw * r;
            var bottom = verts[4].uv0.y + uvh * b;

            var tl = new Vector2(left, top);
            var tr = new Vector2(right, top);
            var br = new Vector2(right, bottom);
            var bl = new Vector2(left, bottom);

            SetVertUV(verts, 0, bl);
            SetVertUV(verts, 1, tl);
            SetVertUV(verts, 2, tr);
            SetVertUV(verts, 3, tr);
            SetVertUV(verts, 4, br);
            SetVertUV(verts, 5, bl);

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
            ListPool<UIVertex>.Release(verts);
        }
    }
}

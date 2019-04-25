using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.UGUI
{
    [RequireComponent(typeof(Image))]
    public sealed class ImageRepeat : BaseMeshEffect
    {
        public enum AdjustMode { Width, Height }
        [SerializeField]
        private AdjustMode m_Mode = AdjustMode.Width;

        [SerializeField]
        private Vector2 m_Size;
        public Vector2 size {
            get { return m_Size; }
            set {
                if (m_Size != value) {
                    m_Size = value;
                    var img = (Image)GetComponent(typeof(Image));
                    img.SetVerticesDirty();
                }
            }
        }

        [SerializeField]
        private Vector2 m_Spacing;

        [SerializeField]
        private Vector2 m_Alignment = new Vector2(0.5f, 0.5f);

        protected override void OnEnable()
        {
            base.OnEnable();
            var img = (Image)GetComponent(typeof(Image));
            img.type = Image.Type.Simple;
        }

        private void AdjustSize()
        {
            var img = (Image)GetComponent(typeof(Image));
            var rawSize = img.overrideSprite.textureRect.size;

            if (m_Mode == AdjustMode.Width) {
                m_Size.y = m_Size.x * (rawSize.y / rawSize.x);
            } else if (m_Mode == AdjustMode.Height) {
                m_Size.x = m_Size.y * (rawSize.x / rawSize.y);
            }
        }

        /*
         1      2|3
        
        0|5      4
        */
        private void InitBaseMesh(List<UIVertex> verts, out int col, out int row)
        {
            var rectTransform = (RectTransform)GetComponent(typeof(RectTransform));
            var povit = rectTransform.pivot;
            var tiledSize = rectTransform.rect.size;
            row = (int)((tiledSize.y + m_Spacing.y) / (m_Size.y + m_Spacing.y));
            col = (int)((tiledSize.x + m_Spacing.x) / (m_Size.x + m_Spacing.x));

            var totalSize = new Vector2(
                (m_Size.x + m_Spacing.x) * col - m_Spacing.x,
                (m_Size.y + m_Spacing.y) * row - m_Spacing.y);

            var start = new Vector3(-totalSize.x * povit.x, -totalSize.y * povit.y);

            var offsetSize = totalSize - tiledSize;
            offsetSize.x *= (povit.x - m_Alignment.x);
            offsetSize.y *= (povit.y - m_Alignment.y);
            start += new Vector3(offsetSize.x, offsetSize.y);

            var vert = verts[0];
            vert.position = start;
            verts[0] = vert;

            vert = verts[5];
            vert.position = start;
            verts[5] = vert;

            vert = verts[1];
            vert.position = start + new Vector3(0, m_Size.y);
            verts[1] = vert;

            vert = verts[2];
            vert.position = start + new Vector3(m_Size.x, m_Size.y);
            verts[2] = vert;

            vert = verts[3];
            vert.position = verts[2].position;
            verts[3] = vert;

            vert = verts[4];
            vert.position = start + new Vector3(m_Size.x, 0);
            verts[4] = vert;
        }

        private void GenerateMesh(List<UIVertex> verts, int col, int row)
        {
            // 从初始的6个顶点复制
            for (int i = 0; i < 6; ++i) {
                var v = verts[i];
                v.position += new Vector3(
                    (m_Size.x + m_Spacing.x) * col,
                    (m_Size.y + m_Spacing.y) * row, 0);
                verts.Add(v);
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;

            var verts = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(verts);

            if (verts.Count > 0) {
                AdjustSize();
                
                int col, row;
                InitBaseMesh(verts, out col, out row);
                
                vh.Clear();
                if (col > 0 && row > 0) {
                    for (int i = 0; i < col; ++i) {
                        for (int j = 0; j < row; ++j) {
                            if (i != 0 || j != 0) {
                                GenerateMesh(verts, i, j);
                            }
                        }
                    }
                    vh.AddUIVertexTriangleStream(verts);
                }
            }

            ListPool<UIVertex>.Release(verts);
        }
        
    }
}


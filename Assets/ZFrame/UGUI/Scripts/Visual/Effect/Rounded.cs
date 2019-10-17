using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.UGUI
{
    public class Rounded : BaseMeshEffect
    {
        private static Material m_RoundedMat;

        [SerializeField] int m_Radius = 8;

        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            if (graphic && graphic.canvas) {
                graphic.canvas.additionalShaderChannels = 
                    AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_RoundedMat == null) {
                m_RoundedMat = new Material(Shader.Find("UI/Rounded")) {
                    name = "Rounded"
                };
            }
            graphic.material = m_RoundedMat;
        }

        ///
        /// Simple
        ///  1      2|3
        /// 
        ///  0|5    4
        /// 
        /// Sliced
        ///  13     50|51
        /// 
        ///  0|5	40
        /// 


        public override void ModifyMesh(VertexHelper vh)
        {
            if (!enabled) return;

            var verts = UGUITools.TempVertList;

            vh.GetUIVertexStream(verts);
            var size = ((RectTransform)transform).rect.size;

            for (int i = 0; i < verts.Count; i++) {
                var vert = verts[i];
                vert.uv1 = size;
                vert.uv2 = new Vector2(m_Radius, 0);
                verts[i] = vert;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
        }
    }
}

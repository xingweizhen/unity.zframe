/// Credit zge
/// Sourced from - http://forum.unity3d.com/threads/draw-circles-or-primitives-on-the-new-ui-canvas.272488/#post-2293224

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Sprites;

namespace ZFrame.UGUI
{
    public class UICircle : MaskableGraphic
    {
        private const int MAX_DEGREE = 360;
        
        [SerializeField]
        private Sprite m_Sprite;

        [Range(0, MAX_DEGREE)]
        public float startDegree = 0;
        [FormerlySerializedAs("fillPercent")]
        [Range(0, MAX_DEGREE)]
        public int fillDegree = MAX_DEGREE;
        public bool fill = true;
        public float thickness = 5;
        [Range(0, 360)]
        public int segments = 60;
        public bool flip = false;
        
        public override Texture mainTexture {
            get {
                if (m_Sprite == null) {
                    if (material != null && material.mainTexture != null) {
                        return material.mainTexture;
                    }
                    return s_WhiteTexture;
                }

                return m_Sprite.texture;
            }
        }
        
        protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
        {
            UIVertex[] vbo = new UIVertex[4];
            for (int i = 0; i < vertices.Length; i++) {
                var vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = vertices[i];
                vert.uv0 = uvs[i];
                vbo[i] = vert;
            }
            return vbo;
        }


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var size = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height);
            var radius = size / 2f;
            var thick = Mathf.Clamp(this.thickness, 0, radius);
            float outer = -radius;
            float inner = outer + thick;
            Vector2 center = Vector2.zero + (new Vector2(0.5f, 0.5f) - rectTransform.pivot) * size;

            vh.Clear();

            Vector2 prevX = center;
            Vector2 prevY = center;

            Vector2 zero, right, one, up;
            if (m_Sprite != null && m_Sprite.packed) {
                var uv = DataUtility.GetOuterUV(m_Sprite);
                zero = new Vector2(uv.x, uv.y);
                right = new Vector2(uv.z, uv.y);
                one = new Vector2(uv.z, uv.w);
                up = new Vector2(uv.x, uv.w);
            } else {
                zero = Vector2.zero; right = Vector2.right; one = Vector2.one; up = Vector2.up; 
            }
            Vector2[] POS = new Vector2[4];            
            Vector2[] UV = flip ? 
                new Vector2[] { zero, right, one, up, } : 
                new Vector2[] { up, one, right, zero, } ;

            float f = this.fillDegree / (float)MAX_DEGREE;
            float degrees = 360f / segments * f;
            var fa = segments / 2;
            int from = -fa, to = fa + 1;
            if (segments % 2 == 1) {
                to += 1;
            }

            for (int i = from; i < to; i++) {
                float rad = Mathf.Deg2Rad * (i * degrees + startDegree);
                float c = Mathf.Cos(rad);
                float s = Mathf.Sin(rad);

                POS[0] = prevX;
                POS[1] = new Vector2(outer * c, outer * s) + center;

                if (fill) {
                    POS[2] = center;
                    POS[3] = center;
                } else {
                    POS[2] = new Vector2(inner * c, inner * s) + center;
                    POS[3] = prevY;
                }

                prevX = POS[1];
                prevY = POS[2];

                if (i == -fa) continue;
                
                vh.AddUIVertexQuad(SetVbo(POS, UV));

            }
        }
    }
}
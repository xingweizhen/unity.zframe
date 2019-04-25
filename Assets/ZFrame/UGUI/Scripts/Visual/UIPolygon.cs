/// Credit CiaccoDavide
/// Sourced from - http://ciaccodavi.de/unity/UIPolygon
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    public class UIPolygon : MaskableGraphic
    {
        [SerializeField]
        Texture m_Texture;
        public bool fill = true;
        public float thickness = 5;
        [Range(3, 360)]
        public int sides = 3;
        [Range(0, 360)]
        public float rotation = 0;
        [Range(0, 1)]
        public float[] VerticesDistances = new float[3];
        public bool flip;
        private float size = 0;

        public override Texture mainTexture {
            get {
                return m_Texture == null ? s_WhiteTexture : m_Texture;
            }
        }
        public Texture texture {
            get {
                return m_Texture;
            }
            set {
                if (m_Texture == value) return;
                m_Texture = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }
        public void DrawPolygon(int _sides)
        {
            sides = _sides;
            VerticesDistances = new float[_sides + 1];
            for (int i = 0; i < _sides; i++) VerticesDistances[i] = 1; ;
            rotation = 0;
        }
        public void DrawPolygon(int _sides, float[] _VerticesDistances)
        {
            sides = _sides;
            VerticesDistances = _VerticesDistances;
            rotation = 0;
        }
        public void DrawPolygon(int _sides, float[] _VerticesDistances, float _rotation)
        {
            sides = _sides;
            VerticesDistances = _VerticesDistances;
            rotation = _rotation;
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
            size = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height);
            var thick = (float)Mathf.Clamp(thickness, 0, size / 2);
            var center = Vector2.zero;

            vh.Clear();

            Vector2 prevX = center;
            Vector2 prevY = center;

            Vector2[] POS = new Vector2[4];
            Vector2[] UV = flip ?
                new Vector2[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up, } :
                new Vector2[] { Vector2.up, Vector2.one, Vector2.right, Vector2.zero, };

            float degrees = 360f / sides;
            int vertices = sides + 1;
            if (VerticesDistances.Length != vertices) {
                VerticesDistances = new float[vertices];
                for (int i = 0; i < vertices - 1; i++) VerticesDistances[i] = 1;
            }
            // last vertex is also the first!
            VerticesDistances[vertices - 1] = VerticesDistances[0];
            for (int i = 0; i < vertices; i++) {
                float outer = -rectTransform.pivot.x * size * VerticesDistances[i];
                float inner = -rectTransform.pivot.x * size * VerticesDistances[i] + thick;
                float rad = Mathf.Deg2Rad * (i * degrees + rotation);
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
                vh.AddUIVertexQuad(SetVbo(POS, UV));
            }
        }
    }
}
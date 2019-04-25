using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    /// <summary>
    /// 用一张Simple类型的Image画个弧形
    /// </summary>
    public class Arc : Figure
    {
        /*
         1      2|3
        
        0|5      4
        */
        
        [SerializeField, Range(0, 360)]
        private float m_Angle = 360;
        [SerializeField]
        public float m_Width = 1;
        [SerializeField]
        public int m_Segment = 10;
        
        public float angle {
            get { return m_Angle; }
            set { if (value != m_Angle) { m_Angle = value; UpdateImage(); } }
        }

        public float width {
            get { return m_Width; }
            set { if (value != m_Width) { m_Width = value; UpdateImage(); } }
        }

        public int segment {
            get { return m_Segment; }
            set { if (value != m_Segment) { m_Segment = value; UpdateImage(); } }
        }
        
        private void MakeEllipse(List<UIVertex> verts, VertexHelper vh)
        {
            Vector3 min = verts[0].position, max = verts[2].position;
            var center = (min + max) / 2;
            var extents = (max - min) / 2;
            var Inner = new Vector3(extents.x - m_Width, extents.y - m_Width);
            if (Inner.x < 0) Inner.x = 0; if (Inner.y < 0) Inner.y = 0;

            var newVerts = ListPool<UIVertex>.Get();
            var totalRadins = angle * Mathf.Deg2Rad;
            var radians = totalRadins / m_Segment;
            for (int i = 0; i < m_Segment; ++i) {
                var p = i * radians - totalRadins / 2;
                float cos0 = Mathf.Cos(p), sin0 = Mathf.Sin(p);
                float cos1 = Mathf.Cos(p + radians), sin1 = Mathf.Sin(p + radians);
                
                var vert0 = verts[0];
                vert0.position = center + new Vector3(Inner.x * cos1, Inner.y * sin1);
                newVerts.Add(vert0);

                var vert1 = verts[1];
                vert1.position = center + new Vector3(extents.x * cos1, extents.y * sin1);
                newVerts.Add(vert1);

                var vert2 = verts[2];
                vert2.position = center + new Vector3(extents.x * cos0, extents.y * sin0);
                newVerts.Add(vert2);

                var vert3 = verts[3];
                vert3.position = vert2.position;
                newVerts.Add(vert3);

                var vert4 = verts[4];
                vert4.position = center + new Vector3(Inner.x * cos0, Inner.y * sin0);
                newVerts.Add(vert4);

                var vert5 = verts[5];
                vert5.position = vert0.position;
                newVerts.Add(vert5);
            }
            vh.Clear();
            vh.AddUIVertexTriangleStream(newVerts);

            ListPool<UIVertex>.Release(newVerts);
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;
            if (segment < 3) return;

            var verts = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(verts);
            if (verts.Count < 6) { vh.Clear(); return;}
            
            MakeEllipse(verts, vh);
        }

        protected override void UpdateImage()
        {
            if (!IsActive()) return;
            if (segment < 3) return;

            var img = GetComponent<Image>();
            img.type = Image.Type.Simple;
        }
    }
}

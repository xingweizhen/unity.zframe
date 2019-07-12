using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace ZFrame.UGUI
{
    /// <summary>
    /// UGUI描边
    /// </summary>
    public class Stroke : BaseMeshEffect
    {
        private static List<UIVertex> m_VetexList = new List<UIVertex>();
        private static Material m_StrokeMat;

        [SerializeField] private Color m_Color = new Color(0, 0, 0, 0.5f);
        [SerializeField, Range(0.1f, 6f)] private float m_Width = 1f;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!enabled) return;

            vh.GetUIVertexStream(m_VetexList);

            this._ProcessVertices();

            vh.Clear();
            vh.AddUIVertexTriangleStream(m_VetexList);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_StrokeMat == null) {
                m_StrokeMat = new Material(Shader.Find("UI/Stroke")) {
                    name = "Stroke"
                };
            }
            graphic.material = m_StrokeMat;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            graphic.material = null;
        }


        private void _ProcessVertices()
        {
            var c3 = transform.rotation * new Vector3(m_Color.r, m_Color.g, m_Color.b);
            var strokeColor = new Color(c3.x, c3.y, c3.z, m_Color.a * graphic.color.a);

            for (int i = 0, count = m_VetexList.Count - 3; i <= count; i += 3) {
                var v1 = m_VetexList[i];
                var v2 = m_VetexList[i + 1];
                var v3 = m_VetexList[i + 2];
                // 计算原顶点坐标中心点
                //
                var minX = _Min(v1.position.x, v2.position.x, v3.position.x);
                var minY = _Min(v1.position.y, v2.position.y, v3.position.y);
                var maxX = _Max(v1.position.x, v2.position.x, v3.position.x);
                var maxY = _Max(v1.position.y, v2.position.y, v3.position.y);
                var posCenter = new Vector2(minX + maxX, minY + maxY) * 0.5f;
                // 计算原始顶点坐标和UV的方向
                //
                Vector2 triX, triY, uvX, uvY;
                Vector2 pos1 = v1.position;
                Vector2 pos2 = v2.position;
                Vector2 pos3 = v3.position;
                if (Mathf.Abs(Vector2.Dot((pos2 - pos1).normalized, Vector2.right))
                    > Mathf.Abs(Vector2.Dot((pos3 - pos2).normalized, Vector2.right))) {
                    triX = pos2 - pos1;
                    triY = pos3 - pos2;
                    uvX = v2.uv0 - v1.uv0;
                    uvY = v3.uv0 - v2.uv0;
                } else {
                    triX = pos3 - pos2;
                    triY = pos2 - pos1;
                    uvX = v3.uv0 - v2.uv0;
                    uvY = v2.uv0 - v1.uv0;
                }

                // 计算原始UV框
                //
                var uvMin = _Min(v1.uv0, v2.uv0, v3.uv0);
                var uvMax = _Max(v1.uv0, v2.uv0, v3.uv0);
                var uvOrigin = new Vector4(uvMin.x, uvMin.y, uvMax.x, uvMax.y);
                // 为每个顶点设置新的Position和UV，并传入原始UV框
                //
                v1 = _SetNewPosAndUV(v1, posCenter, m_Width, strokeColor, triX, triY, uvX, uvY, uvOrigin);
                v2 = _SetNewPosAndUV(v2, posCenter, m_Width, strokeColor, triX, triY, uvX, uvY, uvOrigin);
                v3 = _SetNewPosAndUV(v3, posCenter, m_Width, strokeColor, triX, triY, uvX, uvY, uvOrigin);
                // 应用设置后的UIVertex
                //
                m_VetexList[i] = v1;
                m_VetexList[i + 1] = v2;
                m_VetexList[i + 2] = v3;
            }
        }

        private static UIVertex _SetNewPosAndUV(
            UIVertex pVertex, Vector2 pPosCenter, float strokeWidth, Color strokeColor,
            Vector2 pTriangleX, Vector2 pTriangleY,
            Vector2 pUVX, Vector2 pUVY, Vector4 pUVOrigin)
        {
            // Position
            var pos = pVertex.position;
            var posXOffset = pos.x > pPosCenter.x ? strokeWidth : -strokeWidth;
            var posYOffset = pos.y > pPosCenter.y ? strokeWidth : -strokeWidth;
            pos.x += posXOffset;
            pos.y += posYOffset;
            pVertex.position = pos;
            // UV
            var uv = pVertex.uv0;
            uv += pUVX / pTriangleX.magnitude * posXOffset * (Vector2.Dot(pTriangleX, Vector2.right) > 0 ? 1 : -1);
            uv += pUVY / pTriangleY.magnitude * posYOffset * (Vector2.Dot(pTriangleY, Vector2.up) > 0 ? 1 : -1);
            pVertex.uv0 = uv;
            // 原始UV框
            //pVertex.tangent = pUVOrigin; // 切线在缩放的情况下，xy值会错误。所以放弃使用
            pVertex.uv1 = new Vector2(pUVOrigin.x, pUVOrigin.y);
            pVertex.uv2 = new Vector2(pUVOrigin.z, pUVOrigin.w);            
            pVertex.normal = new Vector3(strokeColor.r, strokeColor.g, strokeColor.b);
            pVertex.uv3 = new Vector2(strokeColor.a, strokeWidth);

            return pVertex;
        }


        private static float _Min(float pA, float pB, float pC)
        {
            return Mathf.Min(Mathf.Min(pA, pB), pC);
        }


        private static float _Max(float pA, float pB, float pC)
        {
            return Mathf.Max(Mathf.Max(pA, pB), pC);
        }


        private static Vector2 _Min(Vector2 pA, Vector2 pB, Vector2 pC)
        {
            return new Vector2(_Min(pA.x, pB.x, pC.x), _Min(pA.y, pB.y, pC.y));
        }


        private static Vector2 _Max(Vector2 pA, Vector2 pB, Vector2 pC)
        {
            return new Vector2(_Max(pA.x, pB.x, pC.x), _Max(pA.y, pB.y, pC.y));
        }
    }
}
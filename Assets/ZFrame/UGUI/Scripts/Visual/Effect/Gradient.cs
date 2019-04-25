/// Credit Breyer
/// Sourced from - http://forum.unity3d.com/threads/scripts-useful-4-6-scripts-collection.264161/#post-1780095

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{    
    [AddComponentMenu("UI/Effects/Gradient", 17)]
    [RequireComponent(typeof(RectTransform), typeof(Graphic))]
	public partial class Gradient : BaseMeshEffect
    {
		private const int N = 6;
		/// <summary>
		///  0|5    1
		/// 
		/// 
		///  4      2|3
		/// </summary>
        
        public GradientMode gradientMode = GradientMode.Local;
        public GradientDir gradientDir = GradientDir.Vertical;
        public bool overwriteAllColor = false;
        public Color vertex1 = Color.white;
        public Color vertex2 = Color.black;
		private Text m_Text { get { return graphic as Text; } }

        protected void Modify(List<UIVertex> vertexList)
        {
            int count = vertexList.Count;
            UIVertex uiVertex = vertexList[0];
            if (gradientMode == GradientMode.Global) {
                if (gradientDir == GradientDir.DiagonalLeftToRight || gradientDir == GradientDir.DiagonalRightToLeft) {
#if UNITY_EDITOR
                    Debug.LogWarning("Diagonal dir is not supported in Global mode");
#endif
                    gradientDir = GradientDir.Vertical;
                }
                float bottomY = gradientDir == GradientDir.Vertical ? vertexList[vertexList.Count - 3].position.y : vertexList[vertexList.Count - 3].position.x;
                float topY = gradientDir == GradientDir.Vertical ? vertexList[0].position.y : vertexList[0].position.x;

                float uiElementHeight = topY - bottomY;

                for (int i = 0; i < count; i++) {
                    uiVertex = vertexList[i];
					if (!overwriteAllColor && uiVertex.color != m_Text.color)
                        continue;
					uiVertex.color *= Color.Lerp(vertex2, vertex1, ((gradientDir == GradientDir.Vertical ? uiVertex.position.y : uiVertex.position.x) - bottomY) / uiElementHeight);
                    vertexList[i] = uiVertex;
                }
            } else {
                for (int i = 0; i < count; i++) {
                    uiVertex = vertexList[i];
					if (!overwriteAllColor && !CompareCarefully(uiVertex.color, m_Text.color))
                        continue;
                    
					switch (gradientDir) {
						case GradientDir.Vertical:
							uiVertex.color *= (i % N == 0 || i % N == 1 || i % N == 5) ? vertex1 : vertex2;
							break;
						case GradientDir.Horizontal:
							uiVertex.color *= (i % N == 0 || i % N == 4 || i % N == 5) ? vertex1 : vertex2;
							break;
						case GradientDir.DiagonalLeftToRight:
							uiVertex.color *= (i % N == 0 || i % N == 5) ? vertex1 : ((i % N == 2 || i % N == 3) ? vertex2 : Color.Lerp(vertex2, vertex1, 0.5f));
							break;
						case GradientDir.DiagonalRightToLeft:
							uiVertex.color *= (i % N == 1) ? vertex1 : (i % N == 4 ? vertex2 : Color.Lerp(vertex2, vertex1, 0.5f));
							break;
					}
                    vertexList[i] = uiVertex;
                }
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;

            var verts = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(verts);

            if (verts.Count == 0) return;

			if (m_Text) {
				Modify(verts);
			} else {
				ModifyImage(verts);
			}

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
            ListPool<UIVertex>.Release(verts);
        }

        public void SetColor(Color c1, Color c2)
        {
            bool hasChanged = false;
            if (vertex1 != c1) {
                vertex1 = c1;
                hasChanged = true;
            }
            if (vertex2 != c2) {
                vertex2 = c2;
                hasChanged = true;
            }
            if (hasChanged) graphic.SetVerticesDirty();
        }

        private bool CompareCarefully(Color col1, Color col2)
        {
            if (Mathf.Abs(col1.r - col2.r) < 0.003f && Mathf.Abs(col1.g - col2.g) < 0.003f && Mathf.Abs(col1.b - col2.b) < 0.003f && Mathf.Abs(col1.a - col2.a) < 0.003f)
                return true;
            return false;
        }
    }

    public enum GradientMode
    {
        Global,
        Local
    }

    public enum GradientDir
    {
        Vertical,
        Horizontal,
        DiagonalLeftToRight,
        DiagonalRightToLeft
        //Free
    }
    //enum color mode Additive, Multiply, Overwrite

}
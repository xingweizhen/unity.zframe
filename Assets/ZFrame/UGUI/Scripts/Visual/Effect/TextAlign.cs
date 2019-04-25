using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    /// <summary>
    /// 文本三角面强制对齐
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class TextAlign : BaseMeshEffect
    {
        public enum Anchor { Top, Bottom, }
        [SerializeField]
        private Anchor m_Anchor;
        [SerializeField]
        private uint m_StartIndex;
        [SerializeField]
        private uint m_EndIndex;

        private int ModifyVertices(List<UIVertex> verts, string text, int startIndex)
        {
            var endIndex = text.Length;
            for (int i = startIndex; i < text.Length; ++i) {
                if (text[i] == '\n') {
                    endIndex = i;
                    break;
                }
            }

            if (verts.Count > (endIndex - 1) * 6) {
                var yLmt = 0f;
                var vertOff = 0;
                if (m_Anchor == Anchor.Top) {
                    yLmt = verts[startIndex * 6].position.y;
                    for (int i = startIndex + 1; i < endIndex; ++i) {
                        var iVert = i * 6 + vertOff;
                        if (yLmt < verts[iVert].position.y) {
                            yLmt = verts[iVert].position.y;
                        }
                    }
                } else {
                    vertOff = 4;
                    yLmt = verts[startIndex * 6 + 4].position.y;
                    for (int i = startIndex + 1; i < endIndex; ++i) {
                        var iVert = i * 6 + vertOff;
                        if (yLmt > verts[iVert].position.y) {
                            yLmt = verts[iVert].position.y;
                        }
                    }
                }

                for (int i = startIndex; i < endIndex; ++i) {
                    if (i < m_StartIndex || (m_EndIndex > 0 && i > m_EndIndex)) continue;
                    var iVert = i * 6;
                    var offset = yLmt - verts[iVert + vertOff].position.y;
                    for (int j = iVert; j < iVert + 6; ++j) {
                        UIVertex v = verts[j];
                        v.position = new Vector3(v.position.x, v.position.y + offset, v.position.z);
                        verts[j] = v;
                    }
                }
            }

            return endIndex + 1;
        }

        private void ModifyVertices(List<UIVertex> verts)
        {
            var text = graphic as Text;
            
            for(int index = 0; index < text.text.Length; ) {
                index = ModifyVertices(verts, text.text, index);
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;

            var verts = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(verts);

            if (verts.Count > 0) {
                ModifyVertices(verts);

                vh.Clear();
                vh.AddUIVertexTriangleStream(verts);
            }

            ListPool<UIVertex>.Release(verts);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif
    }
}

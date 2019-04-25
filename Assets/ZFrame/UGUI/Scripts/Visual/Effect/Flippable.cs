/// Credit ChoMPHi
/// Sourced from - http://forum.unity3d.com/threads/script-flippable-for-ui-graphics.291711/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    [RequireComponent(typeof(RectTransform), typeof(Graphic)), DisallowMultipleComponent]
	public class Flippable : BaseMeshEffect
    {
        [SerializeField]
        private bool m_Horizontal = false;
        [SerializeField]
        private bool m_Veritical = false;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ZFrame.UGUI.Flippable"/> should be flipped horizontally.
        /// </summary>
        /// <value><c>true</c> if horizontal; otherwise, <c>false</c>.</value>
        public bool horizontal {
            get { return m_Horizontal; }
            set {
                if (m_Horizontal != value) {
                    m_Horizontal = value;
                    graphic.SetVerticesDirty();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ZFrame.UGUI.Flippable"/> should be flipped vertically.
        /// </summary>
        /// <value><c>true</c> if vertical; otherwise, <c>false</c>.</value>
        public bool vertical {
            get { return m_Veritical; }
            set {
                if (m_Veritical != value) {
                    m_Veritical = value;
                    graphic.SetVerticesDirty();
                }
            }
        }

        public void ModifyVertices(List<UIVertex> verts)
        {
            RectTransform rt = graphic.rectTransform;

            for (int i = 0; i < verts.Count; ++i) {
                UIVertex v = verts[i];

                // Modify positions
                v.position = new Vector3(
                    (this.m_Horizontal ? (v.position.x + (rt.rect.center.x - v.position.x) * 2) : v.position.x),
                    (this.m_Veritical ? (v.position.y + (rt.rect.center.y - v.position.y) * 2) : v.position.y),
                    v.position.z
                );

                // Apply
                verts[i] = v;
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
    }
}

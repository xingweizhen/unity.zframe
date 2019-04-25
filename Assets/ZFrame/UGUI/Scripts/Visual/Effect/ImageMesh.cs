using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    public class ImageMesh : BaseMeshEffect
    {
        public Mesh sourceMesh;
        
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;

            vh.Clear();
            var tris = sourceMesh.triangles;
            for (int i = 0; i < tris.Length; ++i) {
                
            }
            
            vh.FillMesh(sourceMesh);
            
        }
    }
}

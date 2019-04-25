using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : Singleton<MeshCombiner>
{
    [System.Obsolete("单例类，不需要直接调用构造函数")]
    public MeshCombiner() { }

    public const string COMBINED = "Combined";

    private List<CombineInstance> m_CombineInsts = new List<CombineInstance>();
    private List<Material> m_Mats = new List<Material>();
    private float m_StartTime;

    private void Reset()
    {
        m_CombineInsts.Clear();
        m_Mats.Clear();
    }

    public void Begin()
    {
        Reset();

        m_StartTime = Time.realtimeSinceStartup;
    }
    
    public void AddMesh(Mesh mesh, Material[] mats, Matrix4x4 transform)
    {
        for (int sub = 0; sub < mesh.subMeshCount; sub++) {
            CombineInstance ci = new CombineInstance {
                mesh = mesh,
                subMeshIndex = sub,
                transform = transform,
            };
            m_CombineInsts.Add(ci);
        }

        if (mats != null) m_Mats.AddRange(mats);
    }

    public Mesh Finish(bool mergeSubMeshes, bool useMatrices, string meshName = null)
    {
        Mesh mesh = null;
        if (m_CombineInsts.Count > 0) {
            mesh = new Mesh();
            mesh.name = string.IsNullOrEmpty(meshName) ?
                string.Format("{0} ({1})", mesh.GetInstanceID(), COMBINED) :
                meshName;
            mesh.CombineMeshes(m_CombineInsts.ToArray(), mergeSubMeshes, useMatrices);
            
            LogMgr.I("合并模型完成，消耗: {0} ms", (Time.realtimeSinceStartup - m_StartTime) * 1000);
        }

        Reset();
        return mesh;
    }
}

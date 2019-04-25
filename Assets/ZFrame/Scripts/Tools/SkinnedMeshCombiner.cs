using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshCombiner : Singleton<SkinnedMeshCombiner>
{
    [System.Obsolete("单例类，不需要直接调用构造函数")]
    public SkinnedMeshCombiner() { }

    public const string COMBINED = "Combined";

    private List<CombineInstance> m_CombineInsts = new List<CombineInstance>();
    private List<Material> m_Mats = new List<Material>();
    private List<Transform> m_Bones = new List<Transform>();
    private List<Vector2[]> m_CachedUV1s = new List<Vector2[]>();
    private List<Vector2[]> m_CachedUV2s = new List<Vector2[]>();
    private SkinnedMeshRenderer m_Smr;
    private float m_StartTime;

    public List<Material> skinnedMats {
        get { return m_Mats; }
    }

    private void Reset()
    {
        m_CombineInsts.Clear();
        m_Mats.Clear();
        m_Bones.Clear();
    }

    public void Begin(SkinnedMeshRenderer smr)
    {
        Reset();
        m_Smr = smr;

        m_StartTime = Time.realtimeSinceStartup;
    }

    public void AddBones(string[] boneNames)
    {
        foreach (var bone in boneNames) {
            var t = m_Smr.transform.FindByName(bone);
            if (t) m_Bones.Add(t);
        }
    }

    public void AddBones(Transform[] boneArray)
    {
        // 添加骨骼，注意顺序
        foreach (var b in boneArray) {
            var t = m_Smr.transform.FindByName(b.name); 
            if (t) m_Bones.Add(t);
        }
    }

    public void AddMesh(Mesh mesh, Material mat, Transform[] boneArray)
    {
        for (int sub = 0; sub < mesh.subMeshCount; sub++) {
            CombineInstance ci = new CombineInstance {
                mesh = mesh,
                subMeshIndex = sub
            };
            m_CombineInsts.Add(ci);
        }

        if (mat != null) m_Mats.Add(mat);

        if (boneArray != null) AddBones(boneArray);
    }

    public void AddUV(Rect[] uvs, Rect[] uv2s)
    {
        m_CachedUV1s.Clear();
        m_CachedUV2s.Clear();
        for (int i = 0; i < m_CombineInsts.Count; i++) {
            var uva = m_CombineInsts[i].mesh.uv;

            var uvb = new Vector2[uva.Length];
            for (int k = 0; k < uva.Length; k++) {
                uvb[k] = new Vector2(uva[k].x * uvs[i].width + uvs[i].x, uva[k].y * uvs[i].height + uvs[i].y);
            }
            m_CachedUV1s.Add(m_CombineInsts[i].mesh.uv);
            m_CombineInsts[i].mesh.uv = uvb;

            if (uv2s != null) {
                uvb = new Vector2[uva.Length];
                for (int k = 0; k < uva.Length; k++) {
                    uvb[k] = new Vector2(uva[k].x * uv2s[i].width + uv2s[i].x, uva[k].y * uv2s[i].height + uv2s[i].y);
                }
                m_CachedUV2s.Add(m_CombineInsts[i].mesh.uv2);
                m_CombineInsts[i].mesh.uv2 = uvb;
            }
        }
    }
    
    public void Finish(Material combinedMat, Material[] mats, string meshName)
    {
        if (m_CombineInsts.Count > 0) {
            var mesh = m_Smr.sharedMesh ? m_Smr.sharedMesh : new Mesh();
            mesh.name = string.IsNullOrEmpty(meshName) ?
                string.Format("{0} ({1})", mesh.GetInstanceID(), COMBINED) :
                meshName;

            mesh.CombineMeshes(m_CombineInsts.ToArray(), mats != null || combinedMat, false);
            mesh.RecalculateNormals();
            m_Smr.sharedMesh = mesh;

            LogMgr.I("合并角色模型完成，消耗: {0} ms", (Time.realtimeSinceStartup - m_StartTime) * 1000);
        }

        m_Smr.bones = m_Bones.ToArray();
        if (combinedMat) {
            for (int i = 0; i < m_CombineInsts.Count; i++) {
                if (i < m_CachedUV1s.Count) {
                    m_CombineInsts[i].mesh.uv = m_CachedUV1s[i];
                }
                if (i < m_CachedUV2s.Count) {
                    m_CombineInsts[i].mesh.uv2 = m_CachedUV2s[i];
                }                
            }
            m_CachedUV1s.Clear(); m_CachedUV2s.Clear();
        } else {
            if (m_Mats.Count > 0) m_Smr.materials = m_Mats.ToArray();
            //m_Smr.materials = mats ?? m_Mats.ToArray();
        }

        Reset();
        m_Smr = null;
    }
}

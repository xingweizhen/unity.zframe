using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPropertyTool : Singleton<MaterialPropertyTool>
{
    public readonly MaterialPropertyBlock shared = new MaterialPropertyBlock();

    private Renderer m_Rdr;
    private MaterialPropertyBlock _Begin(Renderer renderer)
    {
        m_Rdr = renderer;

        shared.Clear();
        m_Rdr.GetPropertyBlock(shared);
        return shared;
    }

    public static MaterialPropertyBlock Begin(Renderer renderer)
    {
        if (renderer != null) {
            return Instance._Begin(renderer);
        }

        return Instance.shared;
    }

    public static void Finish()
    {
        if (Instance.m_Rdr != null) {
            Instance.m_Rdr.SetPropertyBlock(Instance.shared);
        }
        Instance.shared.Clear();
        Instance.m_Rdr = null;
    }

    public static void Clear()
    {
        Instance.shared.Clear();
        Instance.m_Rdr = null;
    }
}

public struct MaterialPropertyScope : System.IDisposable
{
    private static ZFrame.Pool<MaterialPropertyBlock> s_Pool = new ZFrame.Pool<MaterialPropertyBlock>(null, mpb => mpb.Clear());
    public static MaterialPropertyBlock Get() { return s_Pool.Get(); }
    public static void Release(MaterialPropertyBlock block) { s_Pool.Release(block); }

    public MaterialPropertyBlock block { get; private set; }
    private Renderer m_Rdr;
    private int m_MatIndex;

    public MaterialPropertyScope(Renderer rdr, int materialIndex = -1)
    {
        m_Rdr = rdr;
        m_MatIndex = materialIndex;
        block = Get();
    }

    public void Dispose()
    {
        if (m_Rdr) {
#if UNITY_2018_3_OR_NEWER
            if (m_MatIndex < 0) {
                m_Rdr.SetPropertyBlock(block);
            } else {
                m_Rdr.SetPropertyBlock(block, m_MatIndex);
            }
#else
            m_Rdr.SetPropertyBlock(block);
#endif
        }
        Release(block);
    }
}
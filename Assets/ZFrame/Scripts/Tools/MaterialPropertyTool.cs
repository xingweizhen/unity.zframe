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
    public bool apply;
    public MaterialPropertyScope(Renderer renderer)
    {
        apply = true;
        MaterialPropertyTool.Begin(renderer);
    }

    void System.IDisposable.Dispose()
    {
        if (apply) {
            MaterialPropertyTool.Finish();
        } else {
            MaterialPropertyTool.Clear();
        }
    }
}

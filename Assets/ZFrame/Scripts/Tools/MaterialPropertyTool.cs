using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPropertyTool : Singleton<MaterialPropertyTool>
{
    private MaterialPropertyBlock m_Props = new MaterialPropertyBlock();
    private readonly MaterialPropertyBlock m_Temp = new MaterialPropertyBlock();

    private Renderer m_Rdr;
    private MaterialPropertyBlock _Begin(Renderer renderer)
    {
        m_Rdr = renderer;

        m_Props.Clear();
        m_Rdr.GetPropertyBlock(m_Props);
        return m_Props;
    }

    private void _Finish()
    {
        if (m_Rdr != null) {
            m_Rdr.SetPropertyBlock(m_Props);
        }
        m_Props.Clear();
        m_Rdr = null;
    }

    public static MaterialPropertyBlock Begin(Renderer renderer)
    {
        if (renderer != null) {
            return Instance._Begin(renderer);
        }

        return Instance.m_Props;
    }

    public static MaterialPropertyBlock Temp(Renderer renderer)
    {
        var temp = Instance.m_Temp;
        temp.Clear();
        renderer.GetPropertyBlock(temp);
        return temp;
    }

    public static void Finish()
    {
        Instance._Finish();
        Instance.m_Temp.Clear();
    }

    public static void Clear()
    {
        Instance.m_Props.Clear();
        Instance.m_Rdr = null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RendererTools
{
#if UNITY_2018_3_OR_NEWER
#else
    public static bool HasPropertyBlock(this Renderer self)
    {
        var block = MaterialPropertyScope.Get();
        self.GetPropertyBlock(block);
        return !block.isEmpty;
    }

    public static void GetSharedMaterials(this Renderer self, List<Material> list)
    {
        list.AddRange(self.sharedMaterials);
    }
#endif
}

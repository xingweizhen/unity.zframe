using UnityEngine;
using System.Collections;

public static class LayerTools {
    
    public static int AddCullingMask(this int self, string layerName)
    {
        return self |= (1 << LayerMask.NameToLayer(layerName));
    }

    public static int DelCullingMask(this int self, string layerName)
    {
        return self &= ~(1 << LayerMask.NameToLayer(layerName));
    }

    public static bool HasCullingMask(this int self, string layerName)
    {
        return (self & (1 << LayerMask.NameToLayer(layerName))) != 0;
    }

}

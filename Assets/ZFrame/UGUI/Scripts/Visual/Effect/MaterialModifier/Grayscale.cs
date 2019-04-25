using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI
{
    [AddComponentMenu("UI/Effects/Grayscale-UI", 18)]
    public class Grayscale : BaseMaterialEffect
    {
        public override Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!IsActive())
                return baseMaterial;

            return UGUITools.ToggleGrayscale(baseMaterial, true);
        }        
    }
}

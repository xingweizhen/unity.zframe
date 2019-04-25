using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI
{    
    public class ImageBlur : BaseMaterialEffect
    {
        [SerializeField]
        protected float m_Distance = 0.015f;
        
        public override Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!IsActive())
                return baseMaterial;
            
			return UGUITools.blurMat;
        }
    }
}

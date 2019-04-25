using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace ZFrame.UGUI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Graphic)), DisallowMultipleComponent]
    public abstract  class BaseMaterialEffect : UIBehaviour, IMaterialModifier
    {
        private Graphic m_Graphic;
        protected Graphic graphic { get { if (!m_Graphic) m_Graphic = GetComponent<Graphic>(); return m_Graphic; } }

        public abstract Material GetModifiedMaterial(Material baseMaterial);

        protected override void OnEnable()
        {
            base.OnEnable();
            graphic.SetMaterialDirty();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            graphic.SetMaterialDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            graphic.SetMaterialDirty();
        }
#endif
    }
}

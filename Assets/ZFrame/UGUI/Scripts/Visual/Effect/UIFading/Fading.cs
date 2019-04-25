using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace ZFrame.UGUI
{
	using Tween;

    [ExecuteInEditMode]
    public abstract class Fading : InteractFade, IFading
    {
        [SerializeField, HideInInspector]
        private FadeGroup m_Group;
        [SerializeField, HideInInspector]
        private GameObject m_Target;
        [SerializeField, HideInInspector]
        private float m_Duration;
        [SerializeField, HideInInspector, FormerlySerializedAs("easeType")]
        protected Ease m_Ease = Ease.Linear;
        [SerializeField, HideInInspector]
        private float m_Delay;
        [SerializeField, HideInInspector]
        private int m_Loops;
        [SerializeField, HideInInspector]
        private bool m_IgnoreTimescale;
        [SerializeField, HideInInspector]
        private LoopType m_LoopType;

        public FadeGroup group { get { return m_Group; } }
		public override GameObject target { get { if (!m_Target) m_Target = gameObject; return m_Target; } }
		public override float duration { get { return m_Duration; } }
		public override Ease easeType { get { return m_Ease; } }
		public override float delay { get { return m_Delay; } }
		public override int loops { get { return m_Loops; } }
        public override bool ignoreTimescale { get { return m_IgnoreTimescale; } }
        public override LoopType loopType { get { return m_LoopType; } }

        public abstract void Apply();
    }
}

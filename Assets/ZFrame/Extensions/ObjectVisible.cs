using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    public class ObjectVisible : UIBehaviour
    {
        private const int iInvisible = 31;
        
        public GameObject target;
        private int m_Layer = -1;

        public void SetUITarget(GameObject target)
        {
            this.target = target;
            m_Layer = target.layer;
            OnCanvasGroupChanged();
        }

        public void UnsetUITarget()
        {
            this.target = null;
        }

        protected override void OnEnable()
        {
            if (target && target.layer != iInvisible) {
                m_Layer = target.layer;
            }
            OnCanvasGroupChanged();
        }

        protected override void OnCanvasGroupChanged()
        {
            if (m_Layer < 0) return;

            if (target) {
                var cv = this.FindCanvasGroup();
                if (cv) {
                    if (cv.alpha == 1) {
                        target.SetLayerRecursively(m_Layer);
                    } else if (target.layer == m_Layer) {
                        target.SetLayerRecursively(iInvisible);
                    }
                }
            }
        }
    }
}

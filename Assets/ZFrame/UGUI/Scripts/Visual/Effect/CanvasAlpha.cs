using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace ZFrame.UGUI
{
    public class CanvasAlpha : UIBehaviour
    {
        [SerializeField]
        private CanvasGroup m_CanvasGroup;

        protected override void OnCanvasGroupChanged()
        {
            var cv = this.FindCanvasGroup();
            if (m_CanvasGroup && cv) {
                m_CanvasGroup.alpha = cv.alpha;
            }
        }
    }
}

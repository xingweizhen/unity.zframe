using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.UGUI
{
    [RequireComponent(typeof(ILabel))]
    public sealed class UILink : UIEventTrigger
    {
        private ILabel m_Label;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Label = GetComponent(typeof(ILabel)) as ILabel;
        }
        
        public override bool Execute(TriggerType id, object data)
        {
            var graphic = m_Label as Graphic;
            if (graphic == null) return false;

            var canvas = graphic.canvas;
            if (canvas == null) return false;

            var cam = canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            var info = m_Label.FindLink(Input.mousePosition, cam);
            if (string.IsNullOrEmpty(info.linkId)) return false;

            return base.Execute(id, data);
        }
    }
}

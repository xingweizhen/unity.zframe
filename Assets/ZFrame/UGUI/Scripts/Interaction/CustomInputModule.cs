using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZFrame.UGUI
{
    public class CustomInputModule : StandaloneInputModule
    {
        public override void Process()
        {
            base.Process();

            foreach (var data in m_PointerData.Values) {
                if (data.pointerId < 0 || data.pointerCurrentRaycast.gameObject == null) continue;
                var go = data.pointerCurrentRaycast.gameObject;
                LogMgr.D("{0} raycast @{1}/{2}", data.pointerId, go.transform.GetHierarchy(), go.name);
            }
        }
    }
}

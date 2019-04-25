using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.UGUI
{
    public class GrayscaleOnDisable : MonoBehaviour, IStateTransition
    {
        public void OnStateTransition(SelectingState state, bool instant)
        {
            var sel = GetComponent(typeof(IStateTransTarget)) as IStateTransTarget;
            if (sel != null && sel.targetGraphic) {
                var material = sel.targetGraphic.material;
                sel.targetGraphic.material = UGUITools.ToggleGrayscale(material, state == SelectingState.Disabled);
            }
        }
    }
}

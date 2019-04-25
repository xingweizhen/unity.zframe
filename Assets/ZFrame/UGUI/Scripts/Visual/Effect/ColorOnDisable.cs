using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.UGUI
{
	public class ColorOnDisable : MonoBehaviour, IStateTransition
	{
        [SerializeField] private Color m_Color = Color.white;
        [SerializeField] private Color m_DisabledColor = Color.gray;
        		
		public void OnStateTransition(SelectingState state, bool instant)
		{
			var sel = GetComponent(typeof(IStateTransTarget)) as IStateTransTarget;
			if (sel != null && sel.targetGraphic) {
				sel.targetGraphic.color = state == SelectingState.Disabled ? m_DisabledColor : m_Color;
			}
		}
	}
}

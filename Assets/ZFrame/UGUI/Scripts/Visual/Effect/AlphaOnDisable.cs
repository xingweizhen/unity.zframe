using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.UGUI
{
	public class AlphaOnDisable : MonoBehaviour, IStateTransition
	{
		[SerializeField] private float m_Alpha = 0.5f;
		
		public void OnStateTransition(SelectingState state, bool instant)
		{
			var sel = GetComponent(typeof(IStateTransTarget)) as IStateTransTarget;
			if (sel != null && sel.targetGraphic) {
				var tarAlpha = state == SelectingState.Disabled ? m_Alpha : 1f;
				var cvgrp = sel.targetGraphic.GetComponent(typeof(CanvasGroup)) as CanvasGroup;
				if (cvgrp) {
					cvgrp.alpha = tarAlpha;
					return;
				}

				var color = sel.targetGraphic.color;
				color.a = tarAlpha;
				sel.targetGraphic.color = color;
			}
		}
	}
}

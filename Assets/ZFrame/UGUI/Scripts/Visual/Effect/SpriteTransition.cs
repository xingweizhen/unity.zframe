using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.UGUI
{
	public class SpriteTransition : MonoBehaviour, IStateTransition
	{
		[SerializeField, AssetRef(type: typeof(Sprite))]
		private string m_Normal;

		[SerializeField, AssetRef(type: typeof(Sprite))]
		private string m_Highlighted;

		[SerializeField, AssetRef(type: typeof(Sprite))]
		private string m_Pressed;

		[SerializeField, AssetRef(type: typeof(Sprite))]
		private string m_Disabled;

		private readonly Sprite[] m_StateSprites = new Sprite[4];

        protected SelectingState curState;
        private Sprite GetSprite(SelectingState state)
		{
			var iState = (int)state;
			if (m_StateSprites[iState] == null) {
				string spritePath = null;
				switch (state) {
					case SelectingState.Normal:
						spritePath = m_Normal;
						break;
					case SelectingState.Highlighted:
						spritePath = m_Highlighted;
						break;
					case SelectingState.Pressed:
						spritePath = m_Pressed;
						break;
					case SelectingState.Disabled:
						spritePath = m_Disabled;
						break;
				}

				if (string.IsNullOrEmpty(spritePath)) spritePath = m_Normal;
                
				spritePath = spritePath.Substring(UGUITools.settings.atlasRoot.Length);
				m_StateSprites[iState] = UISprite.LoadSprite(spritePath, this);
			}

			return m_StateSprites[iState];
		}

		public void SetStateSprite(SelectingState state, string spritePath)
		{
			switch (state) {
				case SelectingState.Normal:
					if (spritePath != m_Normal) {
						m_Normal = spritePath;
						m_StateSprites[(int)state] = null;
					}

					break;
				case SelectingState.Highlighted:
					if (spritePath != m_Highlighted) {
						m_Highlighted = spritePath;
						m_StateSprites[(int)state] = null;
					}

					break;
				case SelectingState.Pressed:
					if (spritePath != m_Pressed) {
						m_Pressed = spritePath;
						m_StateSprites[(int)state] = null;
					}

					break;
				case SelectingState.Disabled:
					if (spritePath != m_Disabled) {
						m_Disabled = spritePath;
						m_StateSprites[(int)state] = null;
					}

					break;
				default:
					return;
			}

			if (curState == state) {
				OnStateTransition(state, false);
			}
		}

		public string GetStateSprite(SelectingState state)
		{
			switch (state) {
				case SelectingState.Normal: return m_Normal;
				case SelectingState.Highlighted: return m_Highlighted;
				case SelectingState.Pressed: return m_Pressed;
				case SelectingState.Disabled: return m_Disabled;
			}

			return null;
		}

		public void OnStateTransition(SelectingState state, bool instant)
		{
            curState = state;
            var sel = GetComponent(typeof(IStateTransTarget)) as IStateTransTarget;
			var image = sel != null ? sel.targetGraphic as Image : null;
			if (image != null) {
				image.overrideSprite = GetSprite(state);
			}
		}
	}
}
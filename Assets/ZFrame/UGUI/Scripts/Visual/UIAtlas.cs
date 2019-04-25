using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace ZFrame.UGUI
{
	[CreateAssetMenu(menuName = "资源库/图集", fileName = "New Atlas.asset")]
	public sealed class UIAtlas : ScriptableObject
	{
		[SerializeField] private SpriteAtlas m_Atlas;
		public SpriteAtlas atlas { get { return m_Atlas; } }
	}
}

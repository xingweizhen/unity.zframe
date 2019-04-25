using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame
{
	public class MaterialFloatProperty : MaterialPropertyValue
	{
		[System.Serializable]
		public class PropValue
		{
			public string name;
			public float value;
		}
		
		[SerializeField]
		private PropValue[] m_Values;

		protected override void SetProperty(MaterialPropertyBlock prop)
		{
			for (int i = 0; i < m_Values.Length; ++i) {
				var v = m_Values[i];
				prop.SetFloat(v.name, v.value);
			}
		}

//		protected override void SetProperty(MaterialPropertyBlock prop, string name, float value)
//		{
//			prop.SetFloat(name, value);
//		}
	}
}

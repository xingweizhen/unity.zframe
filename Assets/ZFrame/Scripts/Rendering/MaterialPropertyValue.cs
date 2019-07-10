using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using ZFrame.UGUI;

namespace ZFrame
{
	[RequireComponent(typeof(Renderer))]
	public abstract class MaterialPropertyValue: MonoBehaviour
	{
//		[System.Serializable]
//		public class PropValue<T>
//		{
//			public string name;
//			public T value;
//		}

//		[SerializeField]
//		private PropValue[] m_Values;

		private void Start()
		{
			var rdr = GetComponent(typeof(Renderer)) as Renderer;
			if (rdr) {
				var prop = MaterialPropertyTool.Begin(rdr);
				SetProperty(prop);
//				for (int i = 0; i < m_Values.Length; ++i) {
//					var v = m_Values[i];
//					SetProperty(prop, v.name, v.value);
//				}
				MaterialPropertyTool.Finish();
			}
		}

		protected abstract void SetProperty(MaterialPropertyBlock prop);
		//protected abstract void SetProperty(MaterialPropertyBlock prop, string name, T value);
	}
}

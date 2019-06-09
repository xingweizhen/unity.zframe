using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame
{
	public abstract class NoBoxingValue<T>
	{	
		public static NoBoxingValue<T> Instance;
    
		protected T m_Value;
    
		public static object Apply(T value)
		{
	#if UNITY_EDITOR
			if (Instance == null) return value;
	#endif
			Instance.m_Value = value;
			return Instance;
		}
	}
}


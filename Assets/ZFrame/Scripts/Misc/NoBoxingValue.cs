using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame
{
	public abstract class NoBoxingValue<T>
	{	
		public static NoBoxingValue<T> Instance;
    
		protected T m_Value;
    
		public static NoBoxingValue<T> Apply(T value)
		{
			Instance.m_Value = value;
			return Instance;
		}
	}
}


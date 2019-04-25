using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame
{
	public abstract class NoBoxingValue<T> //where T : NoBoxingValue<T, V>, new()
	{
		protected NoBoxingValue() {}
		
		public static NoBoxingValue<T> Instance;
    
		protected T m_Value;
    
		public static NoBoxingValue<T> Apply(T value)
		{
			Instance.m_Value = value;
			return Instance;
		}
	}

	public class NoBoxingBool : NoBoxingValue<bool>
	{
		
	}
	
	public class NoBoxingInt : NoBoxingValue<int>
	{
		
	}
	
	public class NoBoxingFloat : NoBoxingValue<float>
	{
		
	}
	
	public class NoBoxingVector2 : NoBoxingValue<Vector2>
	{
		
	}
}


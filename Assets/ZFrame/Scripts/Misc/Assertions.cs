using UnityEngine;
using System.Collections;
using System.Diagnostics;

namespace ZFrame.Assertions
{
    public static class Assert
    {
        [Conditional("UNITY_EDITOR")]
        public static void IsTrue(bool condition, string message = null)
        {
			if (!condition) throw new System.Exception(string.Format("Value is {0}, expected {1}", condition, true));
        }

        [Conditional("UNITY_EDITOR")]
        public static void IsFalse(bool condition, string message = null)
        {
			if (condition) throw new System.Exception(string.Format("Value is {0}, expected {1}", condition, false));
        }

        [Conditional("UNITY_EDITOR")]
        public static void IsNotNull(Object value, string message = null)
        {
			if (value == null) throw new System.Exception(string.Format("Value is {0}, expected NOT null", value));
        }

        [Conditional("UNITY_EDITOR")]
        public static void IsNotNull<T>(T value) where T : class
        {
			if (value == null) throw new System.Exception(string.Format("Value is {0}, expected NOT null", value));
        }
    }
}

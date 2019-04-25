using UnityEngine;
using System.Collections;

namespace TinyJSON
{
    public class ProxyUserdata : Variant
    {
        private object value;

        public ProxyUserdata(object value)
		{
			this.value = value;
		}

        public override void Set(System.IConvertible value)
        {
            this.value = value;
        }

        public override void Set(string value)
        {
            this.value = value;
        }

        public override object ToType(System.Type conversionType, System.IFormatProvider provider)
        {
            if (conversionType == null || value == null) return value;

            return conversionType.IsAssignableFrom(value.GetType()) ? value : null;
        }

        public override string ToJSONString(bool prettyPrinted = false)
        {
            return value != null ? value.ToString() : "NULL";
        }
    }
}

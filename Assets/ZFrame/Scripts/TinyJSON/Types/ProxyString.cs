using System;


namespace TinyJSON
{
	public sealed class ProxyString : Variant
	{
		private string value;


		public ProxyString( string value )
		{
			this.value = value;
		}
		
		public override TypeCode GetTypeCode()
		{
			return TypeCode.String;
		}

        public override void Set(IConvertible value)
        {
            this.value = value.ToString();
        }

        public override void Set(string value)
        {
            this.value = value;
        }

        public override string ToString( IFormatProvider provider )
		{
			return value;
		}

		public override string ToJSONString (bool prettyPrinted = false)
		{
			return string.Format("\"{0}\"", value);
		}
	}
}


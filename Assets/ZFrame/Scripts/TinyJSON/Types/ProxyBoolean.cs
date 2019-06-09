using System;


namespace TinyJSON
{
	public sealed class ProxyBoolean : Variant
	{
		private bool value;

		public ProxyBoolean( bool value )
		{
			this.value = value;
		}

		public override TypeCode GetTypeCode()
		{
			return TypeCode.Boolean;
		}
		
        public override void Set(IConvertible value)
        {
            this.value = (bool)value;
        }

        public override void Set(string value)
        {
            value = value.ToLower();
            if (value == "false") {
                this.value = false;
            } else if (value == "true") {
                this.value = true;
            } else {
                throw new NotSupportedException(
                    string.Format("不能将字符串{0}转为一个布尔型", value));
            }
        }

        public override bool ToBoolean( IFormatProvider provider )
		{
			return value;
		}

		public override string ToJSONString (bool prettyPrinted = false)
		{
			return value.ToString().ToLower();
		}
	}
}


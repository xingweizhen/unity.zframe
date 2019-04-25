using System;
using System.Collections;
using System.Collections.Generic;


namespace TinyJSON
{
	public sealed class ProxyArray : Variant, IEnumerable<Variant>
	{
		private List<Variant> list;


		public ProxyArray()
		{
			list = new List<Variant>();
		}


		IEnumerator<Variant> IEnumerable<Variant>.GetEnumerator()
		{
			return list.GetEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}


		public void Add( Variant item )
		{
			list.Add( item );
		}

        public override void Set(IConvertible value)
        {
            throw new NotSupportedException("无法直接设置ProxyArray的值");
        }

        public override void Set(string value)
        {
            throw new NotSupportedException("无法直接设置ProxyArray的值");
        }

        public override Variant this[ int index ]
		{
			get { return list[index]; }
			set { list[index] = value; }
		}

        public override Variant this[string key]
        {
            get { return null; }
            set { return; }
        }


		public int Count
		{
			get { return list.Count; }
		}

		public override string ToJSONString (bool prettyPrinted = false)
		{
			string currIndent, nextIndent;
			generateIndent(out currIndent, out nextIndent);
			s_GlobalIndent += 1;

			var strbld = new System.Text.StringBuilder();
			strbld.Append('[');
			if (prettyPrinted) strbld.AppendLine();
			for (int i = 0; i < this.Count; ++i) {
				if (prettyPrinted) strbld.Append(nextIndent);
				if (i < this.Count - 1) {
					strbld.AppendFormat("{0},", this[i].ToJSONString(prettyPrinted));
				} else {
					strbld.Append(this[i].ToJSONString(prettyPrinted));
				}
				if (prettyPrinted) strbld.AppendLine();
			}

			if (prettyPrinted) strbld.Append(currIndent);
			strbld.Append(']');
			s_GlobalIndent -= 1;
			return strbld.ToString();
		}
	}
}


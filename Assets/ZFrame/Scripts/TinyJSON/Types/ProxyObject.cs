using System;
using System.Collections;
using System.Collections.Generic;


namespace TinyJSON
{
    public sealed class ProxyObject : Variant, IEnumerable<KeyValuePair<string, Variant>>
    {
        private Dictionary<string, Variant> dict;


        public ProxyObject()
        {
            dict = new Dictionary<string, Variant>();
        }


        IEnumerator<KeyValuePair<string, Variant>> IEnumerable<KeyValuePair<string, Variant>>.GetEnumerator()
        {
            return dict.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.GetEnumerator();
        }


        public void Add(string key, Variant item)
        {
            dict.Add(key, item);
        }

        public override void Set(IConvertible value)
        {
            throw new NotSupportedException("无法直接设置ProxyObject的值");
        }

        public override void Set(string value)
        {
            throw new NotSupportedException("无法直接设置ProxyObject的值");
        }

        public override Variant this[string key] {
            get {
                Variant val = null;
                dict.TryGetValue(key, out val);
                return val;
            }
            set {
                var exist = dict.ContainsKey(key);
                if (value != null) {
                    if (exist) dict[key] = value; else dict.Add(key, value);
                } else if (exist) {
                    dict.Remove(key);
                }                
            }
        }

        public override Variant this[int index] {
            get { return null; }
            set { return; }
        }


        public int Count {
            get { return dict.Count; }
        }

        public override string ToJSONString(bool prettyPrinted = false)
        {
            string currIndent, nextIndent;
            generateIndent(out currIndent, out nextIndent);
            s_GlobalIndent += 1;

            var strbld = new System.Text.StringBuilder();
            strbld.Append('{');
            if (prettyPrinted) strbld.AppendLine();
            var nTotal = dict.Count;
            var nCurr = 0;
            foreach (var kv in dict) {
                nCurr += 1;
                if (prettyPrinted) strbld.Append(nextIndent);
                if (nCurr < nTotal) {
                    strbld.AppendFormat("\"{0}\":{1},", kv.Key, kv.Value.ToJSONString(prettyPrinted));
                } else {
                    strbld.AppendFormat("\"{0}\":{1}", kv.Key, kv.Value.ToJSONString(prettyPrinted));
                }
                if (prettyPrinted) strbld.AppendLine();

            }

            if (prettyPrinted) strbld.Append(currIndent);
            strbld.Append('}');
            s_GlobalIndent -= 1;

            return strbld.ToString();
        }
    }
}


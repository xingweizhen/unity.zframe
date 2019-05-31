using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Asset
{
	public abstract class AssetProcessSettings : ScriptableObject
	{
        //public enum PropertyType { Boolean, Integer, Float };

        //protected class ProcessProperty
        //{
        //    public PropertyType type;
        //    public float value;

        //    public static implicit operator bool(ProcessProperty prop) { return prop.value != 0; }
        //    public static implicit operator float(ProcessProperty prop) { return prop.value; }
        //    public static implicit operator int(ProcessProperty prop) { return (int)prop.value; }
        //}

        public abstract class AbstractSettings
        {
            [NamedProperty("名称")]
            public string name;
            public int flags;
            public List<string> folders;

            [SerializeField]
            protected bool m_Disable;
        }

        public abstract System.Enum props { get; set;  }

		public abstract void OnPreprocess(AssetImporter ai);
		public abstract void OnPostprocess(Object obj);

		protected static bool ContainsAsset(IEnumerable<string> folders, string path)
		{
			foreach (var folder in folders) {
				if (path.Contains(folder)) {
					return true;
				}
			}

			return false;
		}

		protected static bool ContainsFlag(int flags, int value)
		{
			return (flags & value) != 0;
		}
	}
}

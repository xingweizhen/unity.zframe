using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Settings
{
	public abstract class AssetProcessSettings : ZFrameSettings4Folder
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

        protected abstract class AbstractSettings : SettingsBase
        {
	        public int flags;
        }

        [SerializeField]
        private string m_IgnorePattern;

        protected bool IsPathIgnore(string path)
        {
            if (string.IsNullOrEmpty(m_IgnorePattern)) return false;

            return System.Text.RegularExpressions.Regex.IsMatch(path, m_IgnorePattern);
        }

        public abstract System.Enum props { get; set; }

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

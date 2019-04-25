using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Asset
{
	public abstract class AssetProcessSettings : ScriptableObject
	{	
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

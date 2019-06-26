using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Asset
{
    [CreateAssetMenu(menuName ="资源库/预加载资源")]
    public class PreloadAssets : ScriptableObject
    {
        [SerializeField]
        private PreloadAsset[] m_Preloads;
        public PreloadAsset[] assets { get { return m_Preloads; } }
    }

    [System.Serializable]
    public class PreloadAsset
    {
        [AssetRef("", bundleOnly = true)]
        public string path;
        public LoadMethod method;
        public PreloadAsset(string path, LoadMethod method)
        {
            this.path = path;
            this.method = method;
        }

        public static bool operator ==(PreloadAsset a, PreloadAsset b)
        {
            return a.path == b.path;
        }

        public static bool operator !=(PreloadAsset a, PreloadAsset b)
        {
            return a.path != b.path;
        }

        public override bool Equals(object obj)
        {
            if (obj is PreloadAsset) {
                return path == ((PreloadAsset)obj).path;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Settings
{
    public class AssetBundleSettings : ScriptableObject
    {
        [SerializeField]
        private string[] m_BUNDLEPaths;

        [SerializeField]
        private string[] m_CATEGORYPaths;

        [SerializeField]
        private string[] m_OBOPaths;

        [SerializeField]
        private string[] m_SCENEPaths;

        [SerializeField]
        private string[] m_IgnorePaths;

        private void CollectPaths(ICollection<string> collection, string[] array)
        {
            if (array != null) {
                foreach (var path in array) {
                    if (System.IO.Directory.Exists(path)) {
                        collection.Add(path);
                    }
                }
            }
        }

        public void CollectPaths(
            ICollection<string> bundles, 
            ICollection<string> categories, 
            ICollection<string> obos, 
            ICollection<string> scenes,
            ICollection<string> ignores = null)
        {
            CollectPaths(bundles, m_BUNDLEPaths);
            CollectPaths(categories, m_CATEGORYPaths);
            CollectPaths(obos, m_OBOPaths);
            CollectPaths(scenes, m_SCENEPaths);
            if (ignores != null) CollectPaths(ignores, m_IgnorePaths);
        }
    }
}

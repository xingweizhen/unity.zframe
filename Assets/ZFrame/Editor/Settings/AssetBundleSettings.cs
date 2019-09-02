using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Settings
{
    [SettingsMenu("Editor", "AssetBundle命名规则")]
    public class AssetBundleSettings : ScriptableObject
    {
        [SerializeField] private string[] m_BUNDLEPaths;
        [SerializeField] private string[] m_CATEGORYPaths;
        [SerializeField] private string[] m_OBOPaths;
        [SerializeField] private string[] m_SCENEPaths;
        [SerializeField] private string[] m_IgnorePaths;

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

        public IEnumerator<string> ForEachBundlePath()
        {
            if (m_BUNDLEPaths != null) foreach (var path in m_BUNDLEPaths) if (System.IO.Directory.Exists(path)) yield return path;
        }

        public IEnumerator<string> ForEachCategoryPath()
        {
            if (m_CATEGORYPaths != null) foreach (var path in m_CATEGORYPaths) if (System.IO.Directory.Exists(path)) yield return path;
        }

        public IEnumerator<string> ForEachOBOPath()
        {
            if (m_OBOPaths != null) foreach (var path in m_OBOPaths) if (System.IO.Directory.Exists(path)) yield return path;
        }

        public IEnumerator<string> ForEachScenePath()
        {
            if (m_SCENEPaths != null) foreach (var path in m_SCENEPaths) if (System.IO.Directory.Exists(path)) yield return path;
        }

        public IEnumerator<string> ForEachIgnorePath()
        {
            if (m_IgnorePaths != null) foreach (var path in m_IgnorePaths) if (System.IO.Directory.Exists(path)) yield return path;
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

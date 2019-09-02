using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Asset
{
    [SettingsMenu("Resources", "资源管理设置")]
    public class AssetLoaderSettings : ScriptableObject
    {
        public string editorPersistentDataPath = "Issets/PersistentData";
        public string editorStreamingAssetsPath = "Issets/StreamingAssets";
        public string assetBundleFolder = "AssetBundles";
        public string downloadFolder = "Downloads";
        public string assetMD5File = "md5";
        public string assetListFile = "filelist";
        public string shaderBundle = "shaders";

        private static AssetLoaderSettings m_Inst;
        public static AssetLoaderSettings Instance {
            get {
                if (m_Inst == null)
                    m_Inst = Resources.Load("AssetLoaderSettings", typeof(AssetLoaderSettings)) as AssetLoaderSettings;
                return m_Inst;
            }
        }
    }
}

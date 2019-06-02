using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.UGUI
{
    using Asset;
    public class UGUISettings : ScriptableObject
    {
        [SerializeField, AssetPath("UI预设根目录", typeof(UnityEditor.DefaultAsset))]
        private string m_UItFolder;
        public string uiFolder { get { return m_UItFolder; } }
        
        [SerializeField, NamedProperty("图集包所在路径")] private string m_AtlasRoot = "atlas/";
        public string atlasRoot { get { return m_AtlasRoot; } }

        [SerializeField, NamedProperty("界面资源包名")] private string m_UIBundlePath = "ui";
        public string uiBundlePath { get { return m_UIBundlePath; } }

        [SerializeField, AssetRef(type: typeof(Font), name: "主字体")]
        private string m_FontPath;

        [SerializeField, AssetRef(type: typeof(AtlasReference), name: "图集引用")]
        private string m_AtlasRefPath;

        [SerializeField, NamedProperty("界面分辨率")] private Vector2Int m_DefRes = new Vector2Int(1280, 720);
        public Vector2Int defRes { get { return m_DefRes; } }

        [SerializeField, AssetRef(type: typeof(Localization), name: "本地化资源")]
        private string m_LocAssetPath;
        public string locAssetPath { get { return m_LocAssetPath; } }

        [SerializeField, NamedProperty("默认语言")] private string m_DefLang = "cn";
        public string defaultLang { get { return m_DefLang; } }

        [SerializeField, NamedProperty("常规界面最高层级")] private int m_MaxDepth = 90;
        public int maxDepth { get { return m_MaxDepth; } }

        [SerializeField, NamedProperty("长按时常(秒)")] private float m_LongpressTime = 0.5f;
        public float longpressTime { get { return m_LongpressTime; } }

        private Font m_Font;
        public Font font {
            get {
                if (m_Font == null && !string.IsNullOrEmpty(m_FontPath)) {
                    if (AssetLoader.Instance == null) {
#if UNITY_EDITOR
                        m_Font = AssetLoader.EditorLoadAsset(typeof(Font), m_FontPath) as Font;
#endif
                    } else {
                        m_Font = AssetLoader.Instance.Load(typeof(Font), m_FontPath) as Font;
                    }
                }

                return m_Font;
            }
        }

        private AtlasReference m_AtlasRef;
        public AtlasReference atlasRef {
            get {
                if (m_AtlasRef == null && !string.IsNullOrEmpty(m_AtlasRefPath)) {
                    if (AssetLoader.Instance == null) {
#if UNITY_EDITOR
                        m_AtlasRef = AssetLoader.EditorLoadAsset(typeof(AtlasReference), m_AtlasRefPath) as AtlasReference;
#endif
                    } else {
                        m_AtlasRef = AssetLoader.Instance.Load(typeof(AtlasReference), m_AtlasRefPath) as AtlasReference;
                    }
                    if (m_AtlasRef == null) m_AtlasRefPath = null;
                }

                return m_AtlasRef;
            }
        }


    }
}

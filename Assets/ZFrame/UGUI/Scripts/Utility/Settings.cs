using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.UGUI
{
    using Asset;
    public class Settings : ScriptableObject
    {
        [SerializeField] private Vector2 m_DefRes = new Vector2(1280, 720);
        public Vector2 defRes { get { return m_DefRes; } }

        [SerializeField] private string m_UIBundlePath = "UI/";
        public string uiBundlePath { get { return m_UIBundlePath; } }
        
        [SerializeField] private string m_AtlasRoot = "atlas/";
        public string atlasRoot { get { return m_AtlasRoot; } }

        [SerializeField, AssetRef(type: typeof(Font))]
        private string m_FontPath;
        
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


        [SerializeField, AssetRef(type: typeof(AtlasReference))]
        private string m_AtlasRefPath;

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


        [SerializeField] private float m_LongpressTime = 0.5f;
        public float longpressTime { get { return m_LongpressTime; } }

        [SerializeField, AssetRef(type: typeof(Localization))]
        private string m_LocAssetPath;
        public string locAssetPath { get { return m_LocAssetPath; } }

        [SerializeField] private string m_DefLang = "cn";
        public string defaultLang { get { return m_DefLang; } }

        [SerializeField] private int m_MaxDepth = 90;
        public int maxDepth { get { return m_MaxDepth; } }
    }
}

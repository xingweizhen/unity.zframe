using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZFrame.Asset
{
    [CreateAssetMenu(menuName = "Settings/TextureProcess")]
    public class TextureProcessSettings : AssetProcessSettings
    {
        [System.Flags]
        public enum Prop
        {
            MaxTextureSize = 1,
            CompressionQuality = 2,
            CrunchedCompression = 4,
            TextureCompression = 8,
            MipmapEnabled = 16,
            TextureType = 32,
        }

        [System.Serializable]
        public struct Settings
        {
            public int flags;
            public int MaxTextureSize;
            public int CompressionQuality;
            public bool CrunchedCompression;
            public TextureImporterCompression TextureCompression;
            public bool MipmapEnabled;
            public TextureImporterType TextureType;
            public List<string> folders;
            [SerializeField]
            private bool m_Disable;

            public void OnPreprocess(TextureImporter ti)
            {
                if (m_Disable) return;

                if (ContainsAsset(folders, ti.assetPath)) {
                    if (ContainsFlag(flags, (int)Prop.MaxTextureSize)) ti.maxTextureSize = MaxTextureSize;
                    if (ContainsFlag(flags, (int)Prop.CompressionQuality)) ti.compressionQuality = CompressionQuality;
                    if (ContainsFlag(flags, (int)Prop.CrunchedCompression)) ti.crunchedCompression = CrunchedCompression;
                    if (ContainsFlag(flags, (int)Prop.TextureCompression)) ti.textureCompression = TextureCompression;
                    if (ContainsFlag(flags, (int)Prop.MipmapEnabled)) ti.mipmapEnabled = MipmapEnabled;
                    if (ContainsFlag(flags, (int)Prop.TextureType)) ti.textureType = TextureType;
                }
            }

            public void OnPostprocess(Texture tex)
            {

            }
        }

        [SerializeField, HideInInspector] private List<Settings> m_SettingsList;

        private Prop m_Props;
        public override System.Enum props {
            get { return m_Props; }
            set { m_Props = (Prop)value; }
        }

        public override void OnPreprocess(AssetImporter ai)
        {
            var ti = (TextureImporter)ai;
            foreach (var setting in m_SettingsList) {
                setting.OnPreprocess(ti);
            }
        }

        public override void OnPostprocess(Object obj)
        {
            var tex = (Texture)obj;
            foreach (var setting in m_SettingsList) {
                setting.OnPostprocess(tex);
            }
        }
    }
}

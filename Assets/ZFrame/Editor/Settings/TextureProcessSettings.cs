﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZFrame.Settings
{
    [SettingsMenu("Editor", "贴图导入属性")]
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
            WrapMode = 64,
            FilterMode = 128,
            NPOTScale = 256,
            sRGB = 512,
            AlphaSource = 1024,
            AlphaIsTransparency = 2048,
            SpritePackingTag = 4096,
        }

        [System.Serializable]
        protected class Settings : AbstractSettings
        {
            public int MaxTextureSize;
            public int CompressionQuality;
            public bool CrunchedCompression;
            public TextureImporterCompression TextureCompression;
            public bool MipmapEnabled;
            public TextureImporterType TextureType;
            public TextureWrapMode WrapMode;
            public FilterMode FilterMode;
            public TextureImporterNPOTScale NPOTScale;
            public bool sRGB;
            public TextureImporterAlphaSource AlphaSource;
            public bool AlphaIsTransparency;
            public string SpritePackingTag;

            public void OnPreprocess(TextureImporter ti)
            {
                if (m_Disable) return;

                if (ContainsAsset(folders, ti.assetPath)) {
                    if (ContainsFlag(flags, (int)Prop.MaxTextureSize) && ti.maxTextureSize > MaxTextureSize) ti.maxTextureSize = MaxTextureSize;
                    if (ContainsFlag(flags, (int)Prop.CompressionQuality)) ti.compressionQuality = CompressionQuality;
                    if (ContainsFlag(flags, (int)Prop.CrunchedCompression)) ti.crunchedCompression = CrunchedCompression;
                    if (ContainsFlag(flags, (int)Prop.TextureCompression)) ti.textureCompression = TextureCompression;
                    if (ContainsFlag(flags, (int)Prop.MipmapEnabled)) ti.mipmapEnabled = MipmapEnabled;
                    if (ContainsFlag(flags, (int)Prop.TextureType)) ti.textureType = TextureType;
                    if (ContainsFlag(flags, (int)Prop.WrapMode)) ti.wrapMode = WrapMode;
                    if (ContainsFlag(flags, (int)Prop.FilterMode)) ti.filterMode = FilterMode;
                    if (ContainsFlag(flags, (int)Prop.NPOTScale)) ti.npotScale = NPOTScale;
                    if (ContainsFlag(flags, (int)Prop.sRGB)) ti.sRGBTexture = sRGB;
                    if (ContainsFlag(flags, (int)Prop.AlphaSource)) ti.alphaSource = AlphaSource;
                    if (ContainsFlag(flags, (int)Prop.AlphaIsTransparency)) ti.alphaIsTransparency = AlphaIsTransparency;
                    if (ContainsFlag(flags, (int)Prop.SpritePackingTag)) ti.spritePackingTag = SpritePackingTag;
                    
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
            if (IsPathIgnore(ai.assetPath)) return;

            var ti = (TextureImporter)ai;
            foreach (var setting in m_SettingsList) {
                setting.OnPreprocess(ti);
            }
        }

        public override void OnPostprocess(Object obj)
        {
            if (obj && IsPathIgnore(obj.name)) return;

            var tex = (Texture)obj;
            foreach (var setting in m_SettingsList) {
                setting.OnPostprocess(tex);
            }
        }
    }
}

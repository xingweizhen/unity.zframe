using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZFrame.Settings
{
    [SettingsMenu("Editor", "音频导入属性")]
    public class AudioProcessSettings : AssetProcessSettings
    {
        [System.Flags]
        public enum Prop
        {
            ForceToMono = 1,
            Ambisonic = 2,
            LoadInBackground = 4,
            PreloadAudioData = 8,
            LoadType = 16,
            SampleRateSetting = 32,
            SampleRateOverride = 64,
            CompressionFormat = 128,
            Quality = 256,
            ConversionMode = 512,
        }

        [System.Serializable]
        protected class Settings : AbstractSettings
        {
            public bool ForceToMono;
            public bool Ambisonic;
            public bool LoadInBackground;
            public bool PreloadAudioData;
            public AudioClipLoadType LoadType;
            public AudioSampleRateSetting SampleRateSetting;
            public uint SampleRateOverride;
            public AudioCompressionFormat CompressionFormat;
            [Range(0, 1)]
            public float Quality;
            public int ConversionMode;
            public TextureImporterAlphaSource AlphaSource;
            public bool AlphaIsTransparency;

            public void OnPreprocess(AudioImporter ai)
            {
                if (m_Disable) return;

                if (ContainsAsset(folders, ai.assetPath)) {
                    if (ContainsFlag(flags, (int)Prop.ForceToMono)) ai.forceToMono = ForceToMono;
#if UNITY_2017_3_OR_NEWER
                    if (ContainsFlag(flags, (int)Prop.Ambisonic)) ai.ambisonic = Ambisonic;
#endif
                    if (ContainsFlag(flags, (int)Prop.LoadInBackground)) ai.loadInBackground = LoadInBackground;
                    if (ContainsFlag(flags, (int)Prop.PreloadAudioData)) ai.preloadAudioData = PreloadAudioData;

                    var def = ai.defaultSampleSettings;
                    var changed = false;
                    if (ContainsFlag(flags, (int)Prop.LoadType)) { changed = true; def.loadType = LoadType; }
                    if (ContainsFlag(flags, (int)Prop.SampleRateSetting)) { changed = true; def.sampleRateSetting = SampleRateSetting; }
                    if (ContainsFlag(flags, (int)Prop.SampleRateOverride)) { changed = true; def.sampleRateOverride = SampleRateOverride; }
                    if (ContainsFlag(flags, (int)Prop.CompressionFormat)) { changed = true; def.compressionFormat = CompressionFormat; }
                    if (ContainsFlag(flags, (int)Prop.Quality)) { changed = true; def.quality = Quality; }
                    if (ContainsFlag(flags, (int)Prop.ConversionMode)) { changed = true; def.conversionMode = ConversionMode; }
                    if (changed) ai.defaultSampleSettings = def;
                }
            }

            public void OnPostprocess(AudioClip clip)
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

            var audi = (AudioImporter)ai;
            foreach (var setting in m_SettingsList) {
                setting.OnPreprocess(audi);
            }
        }

        public override void OnPostprocess(Object obj)
        {
            if (obj && IsPathIgnore(obj.name)) return;

            var clip = (AudioClip)obj;
            foreach (var setting in m_SettingsList) {
                setting.OnPostprocess(clip);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Asset
{
    public class TheAssetPostprocessor : AssetPostprocessor
    {
        private TextureProcessSettings m_Settings;

        private TextureProcessSettings GetSettings()
        {
            if (m_Settings == null) {
                m_Settings = AssetDatabase.LoadAssetAtPath(
                    "Assets/Editor/TextureProcessSettings.asset", 
                    typeof(TextureProcessSettings)) as TextureProcessSettings;
            }

            return m_Settings;
        }

        private void OnPreprocessTexture()
        {
            var settings = GetSettings();

            if (settings != null) settings.OnPreprocess(assetImporter);
        }

        private void OnPostprocessTexture(Texture2D texture)
        {
            var settings = GetSettings();

            if (settings != null) settings.OnPostprocess(texture);
        }
    }
}

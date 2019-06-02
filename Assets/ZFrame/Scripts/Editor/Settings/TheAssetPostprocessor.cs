using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Editors
{
    using Settings;
    public class TheAssetPostprocessor : AssetPostprocessor
    {
        private TextureProcessSettings GetSettings()
        {
            var guids = AssetDatabase.FindAssets("t:TextureProcessSettings");
            if (guids != null && guids.Length > 0) {
                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var settings = AssetDatabase.LoadAssetAtPath<TextureProcessSettings>(path);
                    if (settings) return settings;
                }
            }
            return null;
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

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets) {
                // Auto Set AssetBundle Name
                AssetBundleMenu.AutoSetAssetBundleName(str);                
            }
            foreach (string str in deletedAssets) {
                //Debug.Log("Deleted Asset: " + str);
            }

            for (int i = 0; i < movedAssets.Length; i++) {
                // Auto Set AssetBundle Name
                AssetBundleMenu.AutoSetAssetBundleName(movedAssets[i]);
            }
        }
    }
}

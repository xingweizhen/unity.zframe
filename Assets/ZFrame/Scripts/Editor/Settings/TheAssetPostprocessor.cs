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
        private const string TEXPROC_SETTINGS = "t:TextureProcessSettings";
        private const string MODELPROC_SETTINGS = "t:ModelProcessSettings";
        private const string AUDIOPROC_SETTINGS = "t:AudioProcessSettings";

        private AssetProcessSettings GetSettings(string filter)
        {
            var guids = AssetDatabase.FindAssets(filter);
            if (guids != null && guids.Length > 0) {
                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var settings = AssetDatabase.LoadAssetAtPath<AssetProcessSettings>(path);
                    if (settings) return settings;
                }
            }
            return null;
        }

        private void OnPreprocessTexture()
        {
            var settings = GetSettings(TEXPROC_SETTINGS);

            if (settings != null) settings.OnPreprocess(assetImporter);
        }

        private void OnPostprocessTexture(Texture2D texture)
        {
            var settings = GetSettings(TEXPROC_SETTINGS);

            if (settings != null) settings.OnPostprocess(texture);
        }

        private void OnPreprocessModel()
        {
            var settings = GetSettings(MODELPROC_SETTINGS);

            if (settings != null) settings.OnPreprocess(assetImporter);
        }

        private void OnPostprocessModel(GameObject g)
        {
            var settings = GetSettings(MODELPROC_SETTINGS);

            if (settings != null) settings.OnPostprocess(g);
        }

        private void OnPreprocessAudio()
        {
            var settings = GetSettings(AUDIOPROC_SETTINGS);

            if (settings != null) settings.OnPreprocess(assetImporter);
        }

        private void OnPostprocessAudio(AudioClip clip)
        {
            var settings = GetSettings(AUDIOPROC_SETTINGS);

            if (settings != null) settings.OnPostprocess(clip);
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

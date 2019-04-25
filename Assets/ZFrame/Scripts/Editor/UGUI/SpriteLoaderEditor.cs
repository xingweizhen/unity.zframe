using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ZFrame.UGUI
{
    [CustomEditor(typeof(SpriteLoader))]
    [CanEditMultipleObjects]
    public class SpriteLoaderEditor : Editor
    {
        private Object cachedSprite;

        private void UpdateSprite()
        {
            if (Application.isPlaying) return;

            var self = target as SpriteLoader;

            string assetbundleName, assetName;
            Asset.AssetLoader.GetAssetpath(self.assetPath, out assetbundleName, out assetName);
            var paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetbundleName, assetName);
            string assetPath = null;
            if (paths != null && paths.Length > 0) {
                assetPath = paths[0];
            }
            
            var img = self.GetComponent<UnityEngine.UI.Image>();
            if (img) {
                img.overrideSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (self.nativeSizeOnLoaded) {
                    img.SetNativeSize();
                }
                img.SetAllDirty();
                return;
            }

            var tex = self.GetComponent<UnityEngine.UI.RawImage>();
            if (tex) {
                tex.texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                if (self.nativeSizeOnLoaded) {
                    tex.SetNativeSize();
                }
                tex.SetAllDirty();
            }
        }

        public override void OnInspectorGUI()
        {
            var self = target as SpriteLoader;
            if (cachedSprite == null && !string.IsNullOrEmpty(self.assetPath)) {
                string assetbundleName, assetName;
                Asset.AssetLoader.GetAssetpath(self.assetPath, out assetbundleName, out assetName);
                var paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetbundleName, assetName);
                if (paths != null && paths.Length > 0) {
                    cachedSprite = AssetDatabase.LoadMainAssetAtPath(paths[0]);
                }
            }
            
            var currentSprite = EditorGUILayout.ObjectField("拖拽对象来添加->", cachedSprite, typeof(Object), false);
            if (currentSprite is Sprite || currentSprite is Texture) {
                var path = AssetDatabase.GetAssetPath(currentSprite);
                var ai = AssetImporter.GetAtPath(path);
                if (string.IsNullOrEmpty(ai.assetBundleName)) {
                    LogMgr.W("{0}没有标志为一个AssetBundle。", currentSprite);
                    self.assetPath = "";
                    currentSprite = null;
                } else {
                    var lastPoint = ai.assetBundleName.LastIndexOf('.');
                    var abName = ai.assetBundleName.Substring(0, lastPoint);
                    self.assetPath = string.Format("{0}/{1}", abName, currentSprite.name);
                }
            } else {
                currentSprite = null;
                self.assetPath = "";
            }


            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nativeSizeOnLoaded"));
            EditorGUILayout.LabelField(string.Format("Asset Path: [{0}]", self.assetPath));
            serializedObject.ApplyModifiedProperties();
            
            if (currentSprite != cachedSprite) {
                cachedSprite = currentSprite;
                UpdateSprite();
            }

            if (!Application.isPlaying) {
                if (GUILayout.Button("加载")) {
                    UpdateSprite();
                }
            }

        }
             
    }
}

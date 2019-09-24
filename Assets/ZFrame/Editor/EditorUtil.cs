using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.EventSystems;
using UnityEditor;

namespace ZFrame
{
    using UGUI;
    using Editors;

    public static class EditorUtil
    {
        public static Object LayoutObject(string label, ref string assetPath, System.Type objType, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(label)) EditorGUILayout.LabelField(label);
            EditorGUI.BeginChangeCheck();
            var obj = EditorGUILayout.ObjectField(AssetPathToObject(assetPath, objType), objType, false, options);
            if (EditorGUI.EndChangeCheck()) {
                assetPath = ObjectToAssetPath(obj, 0);
            }

            EditorGUILayout.EndHorizontal();
            return obj;
        }

        public static Object AssetPathToObject(string assetPath, System.Type type)
        {
            if (!string.IsNullOrEmpty(assetPath)) {			
                var atlasRoot = UGUITools.settings.atlasRoot;
                if (assetPath.OrdinalIgnoreCaseStartsWith(atlasRoot)) {
                    if (type != typeof(SpriteAtlas) && assetPath[assetPath.Length - 1] != '/') {
                        var spritePath = assetPath.Substring(atlasRoot.Length);
                        return UISprite.LoadSprite(spritePath, null);
                    }
                }				

                return Asset.AssetLoader.EditorLoadAsset(type, assetPath);
            }

            return null;
        }

        public static string ObjectToAssetPath(Object obj, int mode)
        {
            if (obj != null) {
                var path = AssetDatabase.GetAssetPath(obj);
                var ai = AssetImporter.GetAtPath(path);
                if (!string.IsNullOrEmpty(ai.assetBundleName)) {
                    switch (mode) {
                        case 0: return string.Concat(ai.assetBundleName, "/", obj.name);
                        case 1: return ai.assetBundleName + '/';
                        case 2: return ai.assetBundleName;
                    }
                }
				
                var atlasRoot = UGUITools.settings.atlasRoot;
                var sprite = obj as Sprite;
                string atlasPath = null, atlasName = null, spriteName = null;
                if (sprite != null) {
                    atlasPath = UISpriteEditor.GetSpriteAssetRef(sprite, out atlasName, out spriteName);
                }
                 switch (mode) {
                        case 0: return string.Format("{0}{1}/{2}", atlasRoot, atlasName, spriteName);
                        case 1: return string.Format("{0}{1}/", atlasRoot, atlasName);
                        case 2: return atlasRoot + atlasName;
                    }
            }

            return null;
        }
    }
}

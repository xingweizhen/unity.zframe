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
                assetPath = ObjectToAssetPath(obj, false);
            }

            EditorGUILayout.EndHorizontal();
            return obj;
        }

        public static Object AssetPathToObject(string assetPath, System.Type type)
        {
            var atlasRoot = UGUITools.settings.atlasRoot;
            if (!string.IsNullOrEmpty(assetPath)) {
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

        public static string ObjectToAssetPath(Object obj, bool bundleOnly)
        {
            if (obj != null) {
                var atlasRoot = UGUITools.settings.atlasRoot;

                var path = AssetDatabase.GetAssetPath(obj);
                var ai = AssetImporter.GetAtPath(path);
                if (!string.IsNullOrEmpty(ai.assetBundleName)) {
                    return bundleOnly
                        ? ai.assetBundleName + '/'
                        : string.Concat(ai.assetBundleName, "/", obj.name);
                }

                var sprite = obj as Sprite;
                string atlasPath = null, atlasName = null, spriteName = null;
                if (sprite != null) {
                    atlasPath = UISpriteEditor.GetSpriteAssetRef(sprite, out atlasName, out spriteName);
                }

                if (!string.IsNullOrEmpty(atlasPath)) {
                    return bundleOnly
                        ? string.Format("{0}{1}/", atlasRoot, atlasName)
                        : string.Format("{0}{1}/{2}", atlasRoot, atlasName, spriteName);
                }
            }

            return null;
        }
    }
}

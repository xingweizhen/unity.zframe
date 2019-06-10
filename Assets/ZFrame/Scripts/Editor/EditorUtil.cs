using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.EventSystems;
using UnityEditor;
using System.Reflection;

namespace ZFrame
{
    using UGUI;
    using Editors;

    public static class EditorUtil
    {        
        public static string SearchField(string value, params GUILayoutOption[] options)
        {
            MethodInfo info = typeof(EditorGUILayout).GetMethod("ToolbarSearchField", 
                BindingFlags.NonPublic | BindingFlags.Static, null, 
                new[] { typeof(string), typeof(GUILayoutOption[]) }, null);
            if (info != null) {
                value = (string)info.Invoke(null, new object[] { value, options });
            }
            return value;
        }

        public static string SearchField(string value, string[] searchModes, ref int searchMode, params GUILayoutOption[] options)
        {
            MethodInfo info = typeof(EditorGUILayout).GetMethod("ToolbarSearchField", 
                BindingFlags.NonPublic | BindingFlags.Static, null, 
                new[] { typeof(string), typeof(string[]), typeof(int).MakeByRefType(), typeof(GUILayoutOption[]) }, null);
            if (info != null) {
                value = (string)info.Invoke(null, new object[] { value, searchModes, searchMode, options });
            }
            return value;
        }

        public static void DefaultProperties(this MaterialEditor self, MaterialProperty[] props)
        {
            self.SetDefaultGUIWidths();
//            if (self.m_InfoMessage != null)
//                EditorGUILayout.HelpBox(self.m_InfoMessage, MessageType.Info);
//            else
//                GUIUtility.GetControlID(MaterialEditor.s_ControlHash, FocusType.Passive, new Rect(0.0f, 0.0f, 0.0f, 0.0f));
            foreach (var prop in props) {
                if ((prop.flags & (MaterialProperty.PropFlags.HideInInspector |
                                MaterialProperty.PropFlags.PerRendererData)) ==
                    MaterialProperty.PropFlags.None)
                    self.DrawProperty(prop);
            }
        }

        public static void DrawProperty(this MaterialEditor self, MaterialProperty prop, string displayName = null)
        {
            if (prop != null) self.ShaderProperty(prop, displayName ?? prop.displayName);
        }

        public static bool KeywordCheck(Material mat, string displayName, string keyword, bool value, bool left = false)
        {
            EditorGUI.BeginChangeCheck();
            if (left) {
                value = EditorGUILayout.ToggleLeft(displayName, value);
            } else {
                value = EditorGUILayout.Toggle(displayName, value);
            }

            if (EditorGUI.EndChangeCheck()) {
                mat.SetKeyword(keyword, value);
            }

            return value;
        }

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

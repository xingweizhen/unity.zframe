using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;

namespace ZFrame
{
    public static class EditorAPI
    {
        private static GUIContent m_TempContent;
        public static GUIContent TempContent(string text, Texture image = null)
        {
            if (m_TempContent == null) {
                m_TempContent = new GUIContent();
            }
            m_TempContent.text = text;
            m_TempContent.image = image;
            return m_TempContent;
        }

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

        #region MaterialEditor
        public static void DefaultProperties(this MaterialEditor self, MaterialProperty[] props)
        {
            self.SetDefaultGUIWidths();

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
        #endregion
    }
}
#endif

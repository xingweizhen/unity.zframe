using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using System.Reflection;

namespace ZFrame
{
    using UGUI;
    public static class EditorUtil
    {
        public static void DrawInteractEvent(SerializedProperty data, bool enableType = true, bool enableName = true)
        {
            EditorGUILayout.LabelField(data.displayName);

            var eventType = data.FindPropertyRelative("type");
            var eventName = data.FindPropertyRelative("name");
            var eventParam = data.FindPropertyRelative("param");

            EditorGUI.BeginDisabledGroup(!enableType);
            EditorGUILayout.PropertyField(eventType);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!enableName);
            EditorGUILayout.PropertyField(eventName);
            EditorGUI.EndDisabledGroup();

            var enumValue = eventName.enumValueIndex;
            EditorGUI.BeginDisabledGroup(enumValue == (int)UIEvent.Auto || enumValue == (int)UIEvent.Close);
            EditorGUILayout.PropertyField(eventParam);
            EditorGUI.EndDisabledGroup();
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

    }
}

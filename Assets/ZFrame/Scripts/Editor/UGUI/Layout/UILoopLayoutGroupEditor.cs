using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace ZFrame.Editors
{
    using UGUI;
    [CustomEditor(typeof(UILoopLayoutGroup), true)]
    public class UILoopLayoutGroupEditor : Editor
    {
        SerializedProperty m_Padding;
        SerializedProperty m_Spacing;
        SerializedProperty m_ChildAlignment;
        SerializedProperty m_ChildControlWidth;
        SerializedProperty m_ChildControlHeight;
        SerializedProperty m_ChildForceExpandWidth;
        SerializedProperty m_ChildForceExpandHeight;

        SerializedProperty m_RawPading, m_Template, m_MinSize;

        SerializedProperty m_Event;

        protected virtual void OnEnable()
        {
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_Spacing = serializedObject.FindProperty("m_Spacing");
            m_ChildAlignment = serializedObject.FindProperty("m_ChildAlignment");
            m_ChildControlWidth = serializedObject.FindProperty("m_ChildControlWidth");
            m_ChildControlHeight = serializedObject.FindProperty("m_ChildControlHeight");
            m_ChildForceExpandWidth = serializedObject.FindProperty("m_ChildForceExpandWidth");
            m_ChildForceExpandHeight = serializedObject.FindProperty("m_ChildForceExpandHeight");

            m_RawPading = serializedObject.FindProperty("m_RawPading");
            m_Template = serializedObject.FindProperty("m_Template");
            m_MinSize = serializedObject.FindProperty("m_MinSize");

            m_Event = serializedObject.FindProperty("m_Event");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_RawPading, true);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_Padding, true);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(m_Spacing, true);
            EditorGUILayout.PropertyField(m_ChildAlignment, true);

            Rect rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, -1, EditorGUIUtility.TrTextContent("Child Controls Size"));
            rect.width = Mathf.Max(50, (rect.width - 4) / 3);
            EditorGUIUtility.labelWidth = 50;
            ToggleLeft(rect, m_ChildControlWidth, EditorGUIUtility.TrTextContent("Width"));
            rect.x += rect.width + 2;
            ToggleLeft(rect, m_ChildControlHeight, EditorGUIUtility.TrTextContent("Height"));
            EditorGUIUtility.labelWidth = 0;

            rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, -1, EditorGUIUtility.TrTextContent("Child Force Expand"));
            rect.width = Mathf.Max(50, (rect.width - 4) / 3);
            EditorGUIUtility.labelWidth = 50;
            ToggleLeft(rect, m_ChildForceExpandWidth, EditorGUIUtility.TrTextContent("Width"));
            rect.x += rect.width + 2;
            ToggleLeft(rect, m_ChildForceExpandHeight, EditorGUIUtility.TrTextContent("Height"));
            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.PropertyField(m_Template);
            EditorGUILayout.PropertyField(m_MinSize);

            var self = target as UILoopLayoutGroup;
            if (!Application.isPlaying) {
                if (self.rawPading != null) {
                    if (self.padding == null) self.padding = new RectOffset();
                    self.padding.left = self.rawPading.left;
                    self.padding.right = self.rawPading.right;
                    self.padding.bottom = self.rawPading.bottom;
                    self.padding.top = self.rawPading.top;
                }
            } else {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField(string.Format("Total Item: {0}", self.totalItem));
                EditorGUILayout.LabelField(string.Format("Start Index: {0}", self.startIndex));
                EditorGUILayout.LabelField(string.Format("First Pos: {0}", self.firstPos));
                EditorGUILayout.LabelField(string.Format("Last Pos: {0}", self.lastPos));
            }
            EditorGUILayout.Separator();

            EditorUtil.DrawInteractEvent(m_Event, false, false);

            serializedObject.ApplyModifiedProperties();
        }

        private void ToggleLeft(Rect position, SerializedProperty property, GUIContent label)
        {
            bool toggle = property.boolValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            toggle = EditorGUI.ToggleLeft(position, label, toggle);
            EditorGUI.indentLevel = oldIndent;
            if (EditorGUI.EndChangeCheck()) {
                property.boolValue = property.hasMultipleDifferentValues ? true : !property.boolValue;
            }
            EditorGUI.showMixedValue = false;
        }
    }
}

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

        SerializedProperty m_RawPadding, m_Template, m_MinSize, m_Revert;

        SerializedProperty m_Event;
        
        private readonly GUIContent m_TrChildControlSize = new GUIContent("Child Controls Size");
        private readonly GUIContent m_TrChildForceExpand = new GUIContent("Child Force Expand");
        private readonly GUIContent m_TrWidth = new GUIContent("Width");
        private readonly GUIContent m_TrHeight = new GUIContent("Height");

        protected virtual void OnEnable()
        {
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_Spacing = serializedObject.FindProperty("m_Spacing");
            m_ChildAlignment = serializedObject.FindProperty("m_ChildAlignment");
            m_ChildControlWidth = serializedObject.FindProperty("m_ChildControlWidth");
            m_ChildControlHeight = serializedObject.FindProperty("m_ChildControlHeight");
            m_ChildForceExpandWidth = serializedObject.FindProperty("m_ChildForceExpandWidth");
            m_ChildForceExpandHeight = serializedObject.FindProperty("m_ChildForceExpandHeight");

            m_RawPadding = serializedObject.FindProperty("m_RawPadding");
            m_Template = serializedObject.FindProperty("m_Template");
            m_MinSize = serializedObject.FindProperty("m_MinSize");
            m_Revert = serializedObject.FindProperty("m_Revert");

            m_Event = serializedObject.FindProperty("m_Event");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_RawPadding, true);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_Padding, true);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(m_Spacing, true);
            EditorGUILayout.PropertyField(m_ChildAlignment, true);

            Rect rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, -1, m_TrChildControlSize);
            rect.width = Mathf.Max(50, (rect.width - 4) / 3);
            EditorGUIUtility.labelWidth = 50;
            ToggleLeft(rect, m_ChildControlWidth, m_TrWidth);
            rect.x += rect.width + 2;
            ToggleLeft(rect, m_ChildControlHeight, m_TrHeight);
            EditorGUIUtility.labelWidth = 0;

            rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, -1, m_TrChildForceExpand);
            rect.width = Mathf.Max(50, (rect.width - 4) / 3);
            EditorGUIUtility.labelWidth = 50;
            ToggleLeft(rect, m_ChildForceExpandWidth, m_TrWidth);
            rect.x += rect.width + 2;
            ToggleLeft(rect, m_ChildForceExpandHeight, m_TrHeight);
            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.Separator();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.PropertyField(m_Template);
            EditorGUILayout.PropertyField(m_MinSize);
            EditorGUILayout.PropertyField(m_Revert);
            EditorGUI.EndDisabledGroup();

            var self = target as UILoopLayoutGroup;
            if (!Application.isPlaying) {
                if (self.rawPadding != null) {
                    if (self.padding == null) self.padding = new RectOffset();
                    self.padding.left = self.rawPadding.left;
                    self.padding.right = self.rawPadding.right;
                    self.padding.bottom = self.rawPadding.bottom;
                    self.padding.top = self.rawPadding.top;
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
                property.boolValue = property.hasMultipleDifferentValues || !property.boolValue;
            }
            EditorGUI.showMixedValue = false;
        }
    }
}

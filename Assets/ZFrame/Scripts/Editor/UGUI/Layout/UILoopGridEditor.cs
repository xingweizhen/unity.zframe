using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace ZFrame.UGUI
{
    [CustomEditor(typeof(UILoopGrid), true)]
    [CanEditMultipleObjects]
    public class UILoopGridEditor : Editor 
    {
        SerializedProperty m_Padding;
        SerializedProperty m_CellSize;
        SerializedProperty m_AutoStretch;
        SerializedProperty m_Spacing;
        SerializedProperty m_StartCorner;
        SerializedProperty m_StartAxis;
        SerializedProperty m_ChildAlignment;
        SerializedProperty m_Constraint;
        SerializedProperty m_ConstraintCount;

        SerializedProperty m_RawPading, m_Template;

        SerializedProperty m_Event;

        protected virtual void OnEnable()
        {
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_CellSize = serializedObject.FindProperty("m_CellSize");
            m_AutoStretch = serializedObject.FindProperty("m_AutoStretch");
            m_Spacing = serializedObject.FindProperty("m_Spacing");
            m_StartCorner = serializedObject.FindProperty("m_StartCorner");
            m_StartAxis = serializedObject.FindProperty("m_StartAxis");
            m_ChildAlignment = serializedObject.FindProperty("m_ChildAlignment");
            m_Constraint = serializedObject.FindProperty("m_Constraint");
            m_ConstraintCount = serializedObject.FindProperty("m_ConstraintCount");

            m_RawPading = serializedObject.FindProperty("m_RawPading");
            m_Template = serializedObject.FindProperty("m_Template");

            m_Event = serializedObject.FindProperty("m_Event");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_RawPading, true);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_Padding, true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(m_CellSize, true);
            EditorGUILayout.PropertyField(m_AutoStretch, true);
            EditorGUILayout.PropertyField(m_Spacing, true);
            EditorGUILayout.PropertyField(m_StartCorner, true);
            EditorGUILayout.PropertyField(m_StartAxis, true);
            EditorGUILayout.PropertyField(m_ChildAlignment, true);
            EditorGUILayout.PropertyField(m_Constraint, true);
            if (m_Constraint.enumValueIndex > 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_ConstraintCount, true);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(m_Template, true);

            var self = target as UILoopGrid;
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
                EditorGUILayout.LabelField(string.Format("Start Line: {0}", self.startLine));
            }
            EditorGUILayout.Separator();

            EditorUtil.DrawInteractEvent(m_Event, false, false);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UILoopGrid), true)]
    [CanEditMultipleObjects]
    public class UILoopGridEditor : Editor 
    {
        SerializedProperty m_CellSize;
        SerializedProperty m_AutoStretch;
        SerializedProperty m_Spacing;
        SerializedProperty m_StartCorner;
        SerializedProperty m_StartAxis;
        SerializedProperty m_ChildAlignment;
        SerializedProperty m_Constraint;
        SerializedProperty m_ConstraintCount;

        SerializedProperty m_RawPadding, m_Template;

        SerializedProperty m_Event;

        protected virtual void OnEnable()
        {
            m_CellSize = serializedObject.FindProperty("m_CellSize");
            m_AutoStretch = serializedObject.FindProperty("m_AutoStretch");
            m_Spacing = serializedObject.FindProperty("m_Spacing");
            m_StartCorner = serializedObject.FindProperty("m_StartCorner");
            m_StartAxis = serializedObject.FindProperty("m_StartAxis");
            m_ChildAlignment = serializedObject.FindProperty("m_ChildAlignment");
            m_Constraint = serializedObject.FindProperty("m_Constraint");
            m_ConstraintCount = serializedObject.FindProperty("m_ConstraintCount");

            m_RawPadding = serializedObject.FindProperty("m_RawPadding");
            m_Template = serializedObject.FindProperty("m_Template");

            m_Event = serializedObject.FindProperty("m_Event");
        }

        public override void OnInspectorGUI()
        {
            var self = target as UILoopGrid;
            serializedObject.Update();

            var paddingChanged = false;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_RawPadding, true);
            paddingChanged = EditorGUI.EndChangeCheck();
            EditorGUILayout.LabelField("Padding", string.Format("left:{0} right:{1} top:{2}, bottom:{3}",
                self.padding.left, self.padding.right, self.padding.top, self.padding.bottom));

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

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.PropertyField(m_Template, true);
            EventDataDrawer.Layout(m_Event, "Require Item", false, false);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying) {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField(string.Format("Total Item: {0}", self.totalItem));
                EditorGUILayout.LabelField(string.Format("Start Line: {0}", self.startLine));
            } else if (paddingChanged) {
                if (self.padding == null) self.padding = new RectOffset();
                self.padding.left = self.rawPadding.left;
                self.padding.right = self.rawPadding.right;
                self.padding.bottom = self.rawPadding.bottom;
                self.padding.top = self.rawPadding.top;
                UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(self.GetComponent<RectTransform>());
            }

        }
    }
}

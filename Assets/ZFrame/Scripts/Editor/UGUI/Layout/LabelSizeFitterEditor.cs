using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

namespace ZFrame.Editors
{
    using UGUI;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(LabelSizeFitter))]
    public class LabelSizeFitterEditor : Editor
    {

        SerializedProperty fitWidth, fitHeight;
        SerializedProperty m_MinWidth, m_MinHeight;
        private void OnEnable()
        {
            fitWidth = serializedObject.FindProperty("autoWidth");
            fitHeight = serializedObject.FindProperty("autoHeight");
            m_MinWidth = serializedObject.FindProperty("m_MinWidth");
            m_MinHeight = serializedObject.FindProperty("m_MinHeight");
        }

        public override void OnInspectorGUI()
        {
            var self = target as LabelSizeFitter;
            var group = self.transform.parent.GetComponent(typeof(ILayoutController));
            if (group == null) {
                EditorGUILayout.HelpBox("Missing <ILayoutController> on parent", MessageType.Error);
            }

            base.OnInspectorGUI();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(fitWidth, new GUIContent("Auto Width"));
            if (self.autoWidth) {
                EditorGUILayout.LabelField(string.Format("Auto = {0}", self.minWidth));                
            } else {
                EditorGUILayout.PropertyField(m_MinWidth, new GUIContent(""));
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(fitHeight, new GUIContent("Auto Heigth"));
            if (self.autoHeight) {
                EditorGUILayout.LabelField(string.Format("Auto = {0}", self.minHeight));                
            } else {
                EditorGUILayout.PropertyField(m_MinHeight, new GUIContent(""));
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}

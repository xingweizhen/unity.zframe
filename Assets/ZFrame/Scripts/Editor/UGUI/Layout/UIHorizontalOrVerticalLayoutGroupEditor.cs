using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    [CustomEditor(typeof(UIHorizontalOrVerticalLayoutGroup))]
    public class UIHorizontalOrVerticalLayoutGroupEditor : HorizontalOrVerticalLayoutGroupEditor
    {
        protected SerializedProperty autoFitSize;
        protected SerializedProperty smoothLaying;

        protected override void OnEnable()
        {
            base.OnEnable();
            autoFitSize = serializedObject.FindProperty("autoFitSize");
            smoothLaying = serializedObject.FindProperty("smoothLaying");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(autoFitSize, new GUIContent("Auto Fit Size"));
            EditorGUILayout.PropertyField(smoothLaying, new GUIContent("Smooth Laying"));
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.UI;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UISlider))]
    public class UISliderEditor : SliderEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("antiProgress"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minLmt"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxLmt"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RadialHandle"));

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Event Setting", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EventDataDrawer.Layout(serializedObject.FindProperty("m_ValueChanged"), false);
            --EditorGUI.indentLevel;

            serializedObject.ApplyModifiedProperties();

        }
    }
}


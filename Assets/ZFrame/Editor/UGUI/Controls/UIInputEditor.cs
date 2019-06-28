using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UIInput))]
    public class UIInputEditor : InputFieldEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Event Setting", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EventDataDrawer.Layout(serializedObject.FindProperty("m_ValueChanged"), false);
            EventDataDrawer.Layout(serializedObject.FindProperty("m_Submit"), false);
            --EditorGUI.indentLevel;

            serializedObject.ApplyModifiedProperties();
        }
    }
}

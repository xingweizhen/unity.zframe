using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.UI;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UIProgress))]
    [CanEditMultipleObjects]
    public class UIProgressEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Separator();

            var self = target as UIProgress;
            var prevValue = self.value;
            var currValue = EditorGUILayout.Slider(prevValue, self.minValue, self.maxValue);
            if (currValue != prevValue) self.value = currValue;

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Event Setting", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorUtil.DrawInteractEvent(serializedObject.FindProperty("m_ValueChanged"), false);
            --EditorGUI.indentLevel;

            serializedObject.ApplyModifiedProperties();

        }
    }
}


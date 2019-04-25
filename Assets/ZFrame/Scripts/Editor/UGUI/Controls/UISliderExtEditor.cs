using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.UI;

namespace ZFrame.UGUI
{
    [CustomEditor(typeof(UISliderExt))]
    public class UISliderExtEditor : SliderEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PrevLayer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FollowRect"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("totalLayer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LayerColors"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lbLayer"));

            var self = target as UISliderExt;
            var prevValue = self.value;
            var currValue = EditorGUILayout.Slider(prevValue, self.minValue, self.maxValue);
            if (currValue != prevValue) self.value = currValue;

            serializedObject.ApplyModifiedProperties();

        }
    }
}


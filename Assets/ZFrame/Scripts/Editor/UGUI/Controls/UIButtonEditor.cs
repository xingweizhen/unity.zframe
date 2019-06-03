using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UIButton), true), CanEditMultipleObjects]
    public class UIButtonEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Separator();

            if (((UIButton)target).transition == UnityEngine.UI.Selectable.Transition.None) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TargetGraphic"));
            }

            EditorGUILayout.LabelField("Event Setting", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorUtil.DrawInteractEvent(serializedObject.FindProperty("m_Event"), false);
            --EditorGUI.indentLevel;

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clickSfx"));
            serializedObject.ApplyModifiedProperties();
            
        }
    }
}

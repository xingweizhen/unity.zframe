using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    [CustomEditor(typeof(UIToggle)), CanEditMultipleObjects]
    public class UIToggleEditor : ToggleEditor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Event Setting", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorUtil.DrawInteractEvent(serializedObject.FindProperty("m_Event"), false);
            --EditorGUI.indentLevel;

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CheckedTrans"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UncheckedTrans"));            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clickSfx"));
            serializedObject.ApplyModifiedProperties();
            
        }
    }
}

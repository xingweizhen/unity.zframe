using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace ZFrame.UGUI
{
    [CustomEditor(typeof(UIScrollView)), CanEditMultipleObjects]
    public class UIScrollViewEditor : ScrollRectEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("Event Setting", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorUtil.DrawInteractEvent(serializedObject.FindProperty("m_Event"), false, false);
            EditorUtil.DrawInteractEvent(serializedObject.FindProperty("m_BeginDrag"), false, false);
            EditorUtil.DrawInteractEvent(serializedObject.FindProperty("m_Drag"), false, false);
            EditorUtil.DrawInteractEvent(serializedObject.FindProperty("m_EndDrag"), false, false);
            EditorUtil.DrawInteractEvent(serializedObject.FindProperty("m_ChildOverlap"), false, false);
            --EditorGUI.indentLevel;

            serializedObject.ApplyModifiedProperties();
        }
    }
}

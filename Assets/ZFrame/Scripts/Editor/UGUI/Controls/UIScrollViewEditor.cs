using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UIScrollView)), CanEditMultipleObjects]
    public class UIScrollViewEditor : ScrollRectEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("Event Setting", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EventDataDrawer.Layout(serializedObject.FindProperty("m_Event"), "Value Changed", false, false);
            EventDataDrawer.Layout(serializedObject.FindProperty("m_BeginDrag"), false, false);
            EventDataDrawer.Layout(serializedObject.FindProperty("m_Drag"), false, false);
            EventDataDrawer.Layout(serializedObject.FindProperty("m_EndDrag"), false, false);
            EventDataDrawer.Layout(serializedObject.FindProperty("m_ChildOverlap"), false, false);
            --EditorGUI.indentLevel;

            serializedObject.ApplyModifiedProperties();
        }
    }
}

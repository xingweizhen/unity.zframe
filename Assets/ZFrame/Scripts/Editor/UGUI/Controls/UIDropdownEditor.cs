using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.UI;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UIDropdown), true)]
    public class UIDropdownEditor : DropdownEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Event Setting", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorUtil.DrawInteractEvent(serializedObject.FindProperty("m_Event"), false);
            --EditorGUI.indentLevel;
        }
    }
}

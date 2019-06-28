using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.UI;
using System.Collections;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UIEventTrigger), true)]
    [CanEditMultipleObjects]
    public class UIEventTriggerEditor : Editor
    {
        private ReorderableList m_EventsList;
        private SerializedProperty m_Events;

        private void OnEnable()
        {
            m_Events = serializedObject.FindProperty("m_Events");
            m_EventsList = new ReorderableList(serializedObject, m_Events, true, true, true, true);
            m_EventsList.elementHeight = (EditorGUIUtility.singleLineHeight + 2) * 2;

            m_EventsList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "事件列表");
            m_EventsList.drawElementCallback = (Rect rect, int index, bool selected, bool focused) => {
                var element = m_EventsList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element);
            };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            m_EventsList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

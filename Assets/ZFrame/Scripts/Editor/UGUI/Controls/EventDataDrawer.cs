using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomPropertyDrawer(typeof(EventData))]
    public class EventDataDrawer : PropertyDrawer 
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var eventType = property.FindPropertyRelative("type");
            var eventName = property.FindPropertyRelative("name");
            var eventParam = property.FindPropertyRelative("param");

            position.height =  EditorGUIUtility.singleLineHeight;

            EditorGUI.BeginDisabledGroup(eventType.enumValueIndex < 0);
            EditorGUI.PropertyField(position, eventType);
            EditorGUI.EndDisabledGroup();

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, eventName);

            position.y += EditorGUIUtility.singleLineHeight;
            var enumValue = eventName.enumValueIndex;
            EditorGUI.BeginDisabledGroup(enumValue == (int)UIEvent.Auto || enumValue == (int)UIEvent.Close);
            EditorGUI.PropertyField(position, eventParam);
            EditorGUI.EndDisabledGroup();
        }
    }
}

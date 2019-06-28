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
            return EditorGUIUtility.singleLineHeight * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var eventType = property.FindPropertyRelative("type");
            var eventName = property.FindPropertyRelative("name");
            var eventParam = property.FindPropertyRelative("param");

            position.height = EditorGUIUtility.singleLineHeight;
            //EditorGUI.LabelField(position, label);

            var rect = position;
            rect.width /= 2;
            EditorGUI.PropertyField(rect, eventType, GUIContent.none);

            rect.x += rect.width;
            EditorGUI.PropertyField(rect, eventName, GUIContent.none);

            position.y += EditorGUIUtility.singleLineHeight;
            var enumValue = eventName.enumValueIndex;
            EditorGUI.BeginDisabledGroup(enumValue == (int)UIEvent.Auto || enumValue == (int)UIEvent.Close);
            EditorGUI.PropertyField(position, eventParam, GUIContent.none);
            EditorGUI.EndDisabledGroup();
        }

        public static void Layout(SerializedProperty data, string displayName, bool enableType = true, bool enableName = true)
        {
            var eventType = data.FindPropertyRelative("type");
            var eventName = data.FindPropertyRelative("name");
            var eventParam = data.FindPropertyRelative("param");

            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(rect, displayName);

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            rect.xMin += EditorGUIUtility.labelWidth;
            rect.width /= 2;

            EditorGUI.BeginDisabledGroup(!enableType);
            if (System.Enum.IsDefined(typeof(TriggerType), eventType.intValue)) {
                eventType.intValue = (int)(TriggerType)EditorGUI.EnumPopup(rect, (TriggerType)eventType.intValue);
            } else {
                EditorGUI.LabelField(rect, string.Format("[{0}]", eventType.intValue), EditorStyles.popup);
            }
            EditorGUI.EndDisabledGroup();

            rect.x += rect.width;
            EditorGUI.BeginDisabledGroup(!enableName);
            EditorGUI.PropertyField(rect, eventName, GUIContent.none);
            EditorGUI.EndDisabledGroup();

            var paramRt = EditorGUILayout.GetControlRect();
            paramRt.xMin += EditorGUIUtility.labelWidth;
            var enumValue = eventName.enumValueIndex;
            EditorGUI.BeginDisabledGroup(enumValue == (int)UIEvent.Auto || enumValue == (int)UIEvent.Close);
            EditorGUI.PropertyField(paramRt, eventParam, GUIContent.none);
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel = indentLevel;
        }

        public static void Layout(SerializedProperty data, bool enableType = true, bool enableName = true)
        {
            Layout(data, data.displayName, enableType, enableName);
        }
    }
}

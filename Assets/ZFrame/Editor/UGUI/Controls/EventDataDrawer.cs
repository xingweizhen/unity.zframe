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
        private static float GetHeight() { return EditorGUIUtility.singleLineHeight * 2 + 2; }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawGUI(position, property, label);
        }

        private static void DrawGUI(Rect position, SerializedProperty property, GUIContent label, bool enableType = true, bool enableName = true)
        {
            var eventType = property.FindPropertyRelative("type");
            var eventName = property.FindPropertyRelative("name");
            var eventParam = property.FindPropertyRelative("param");

            var labelWidth = EditorGUIUtility.labelWidth;

            var labelRt = new Rect(position.x, position.y, 0, position.height);
            if (!string.IsNullOrEmpty(label.text)) {
                labelRt.width = labelWidth;
                EditorGUI.LabelField(labelRt, label);
            }

            var contentWidth = position.width - labelRt.width;

            var rect = new Rect(labelRt.xMax, position.y, contentWidth / 2, EditorGUIUtility.singleLineHeight);
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginDisabledGroup(!enableType);
            if (System.Enum.IsDefined(typeof(TriggerType), eventType.intValue)) {
                eventType.intValue = (int)(TriggerType)EditorGUI.EnumPopup(rect, (TriggerType)eventType.intValue);
            } else {
                EditorGUI.LabelField(rect, string.Format("[{0}]", eventType.intValue), EditorStyles.popup);
            }
            EditorGUI.EndDisabledGroup();

            rect.x = rect.xMax;
            EditorGUI.BeginDisabledGroup(!enableName);
            EditorGUI.PropertyField(rect, eventName, GUIContent.none);
            EditorGUI.EndDisabledGroup();

            var paramRt = new Rect(labelRt.xMax, rect.yMax + 2, contentWidth, EditorGUIUtility.singleLineHeight);
            var enumValue = eventName.enumValueIndex;
            EditorGUI.BeginDisabledGroup(enumValue == (int)UIEvent.Auto || enumValue == (int)UIEvent.Close);
            EditorGUI.PropertyField(paramRt, eventParam, GUIContent.none);
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel = indentLevel;
        }

        public static void Layout(SerializedProperty data, string displayName, bool enableType = true, bool enableName = true)
        {
            DrawGUI(EditorGUILayout.GetControlRect(false, GetHeight()), data, new GUIContent(displayName), enableType, enableName);
        }

        public static void Layout(SerializedProperty data, bool enableType = true, bool enableName = true)
        {
            DrawGUI(EditorGUILayout.GetControlRect(false, GetHeight()), data, new GUIContent(data.displayName), enableType, enableName);
        }
    }
}

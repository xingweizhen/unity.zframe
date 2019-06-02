using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Editors
{
    [CustomPropertyDrawer(typeof(EnumValueAttribute))]
    public class EnumValueAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var enumValue = attribute as EnumValueAttribute;
            if (!string.IsNullOrEmpty(enumValue.name)) {
                label.text = enumValue.name;
            }

            var enumValues = new string[property.enumDisplayNames.Length];
            for (int i = 0; i < enumValues.Length; ++i) {
                enumValues[i] = string.Format(enumValue.format, i, property.enumDisplayNames[i]);
            }

            var intW = 30;
            var width = position.width;
            position.width -= intW;
            var index = EditorGUI.Popup(position, label.text, property.enumValueIndex, enumValues);
            property.enumValueIndex = index;

            position.x = position.width + 15;
            position.width = intW;
            EditorGUI.TextField(position, index.ToString());
        }
    }
}

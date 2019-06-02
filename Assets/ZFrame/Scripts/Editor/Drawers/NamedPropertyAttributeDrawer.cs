using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZFrame.Editors
{
    [CustomPropertyDrawer(typeof(NamedPropertyAttribute))]
    public class NamedPropertyAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.text = ((NamedPropertyAttribute)attribute).name;
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
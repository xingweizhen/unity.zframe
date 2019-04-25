using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(NamedPropertyAttribute))]
public class NamedPropertyAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
//        label = new GUIContent(label) {
//            text = (attribute as NamedPropertyAttribute).name
//        };
        
        label.text = ((NamedPropertyAttribute)attribute).name;
        EditorGUI.PropertyField(position, property, label);
    }
}

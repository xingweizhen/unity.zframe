using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(EnumFlagsValueAttribute))]
public class EnumFlagsValueAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = attribute as EnumFlagsValueAttribute;
        property.intValue = EditorGUI.MaskField(position, label, property.intValue, attr.flags);
    }
}

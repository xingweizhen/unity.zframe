using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(ColorLib.NamedColor))]
public class NamedColorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var name = property.FindPropertyRelative("name");
        var color = property.FindPropertyRelative("color");

        position.x += 10;

        var rect = position;
        rect.width = position.width / 3;
        var nameValue = EditorGUI.TextField(rect, name.stringValue);
        rect.x += rect.width + 10;
        rect.width = position.width - rect.x;
        var colorValue = EditorGUI.ColorField(rect, color.colorValue);

        name.stringValue = nameValue;
        color.colorValue = colorValue;
    }
}

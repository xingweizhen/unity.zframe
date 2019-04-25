using UnityEngine;
using UnityEditor;
using System.Collections;

//[CustomPropertyDrawer(typeof(ColorLib.FontColor))]
public class FontColorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //var name = property.FindPropertyRelative("name");
        //var outline = property.FindPropertyRelative("outline");
        //var gradient1 = property.FindPropertyRelative("gradient1");
        //var gradient2 = property.FindPropertyRelative("gradient2");
    }    
}

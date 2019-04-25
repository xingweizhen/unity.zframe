using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomPropertyDrawer(typeof(ElementListAttribute))]
public class ElementListAttributeDrawer : PropertyDrawer
{
    private const int BTN_WIDTH = 30;

    private  void DrawArrayTool(Rect position, SerializedProperty property)
    {
        string path = property.propertyPath;
        int arrayInd = path.LastIndexOf(".Array");

        SerializedObject so = property.serializedObject;
        string arrayPath = path.Substring(0, arrayInd);
        SerializedProperty arrayProp = so.FindProperty(arrayPath);

        //Next we need to grab the index from the path string
        int indStart = path.IndexOf("[") + 1;
        int indEnd = path.IndexOf("]");

        string indString = path.Substring(indStart, indEnd - indStart);

        int myIndex = int.Parse(indString);
        Rect rcButton = position;
        rcButton.height = EditorGUIUtility.singleLineHeight;
        rcButton.x = position.xMax - BTN_WIDTH * 4;
        rcButton.width = BTN_WIDTH;

        bool lastEnabled = GUI.enabled;

        if (myIndex == 0)
            GUI.enabled = false;

        if (GUI.Button(rcButton, "Up")) {
            arrayProp.MoveArrayElement(myIndex, myIndex - 1);
            so.ApplyModifiedProperties();

        }

        rcButton.x += BTN_WIDTH;
        GUI.enabled = lastEnabled;
        var isLastIdx = myIndex >= arrayProp.arraySize - 1;
        if (isLastIdx)
            GUI.enabled = false;

        if (GUI.Button(rcButton, "Dn")) {
            arrayProp.MoveArrayElement(myIndex, myIndex + 1);
            so.ApplyModifiedProperties();
        }

        GUI.enabled = lastEnabled;

        rcButton.x += BTN_WIDTH;
        if (GUI.Button(rcButton, "Del")) {
            arrayProp.DeleteArrayElementAtIndex(myIndex);
            so.ApplyModifiedProperties();
        }

        rcButton.x += BTN_WIDTH;
        if (GUI.Button(rcButton, "Ins")) {
            arrayProp.InsertArrayElementAtIndex(myIndex);
            so.ApplyModifiedProperties();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property);
        //var elementList = attribute as ElementListAttribute;
        //var n = property.serializedObject.targetObjects.Length;
        //return EditorGUIUtility.singleLineHeight * elementList.elementHeight * n;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DrawArrayTool(position, property);
        
        if (!property.isExpanded)
            position.width -= BTN_WIDTH * 4;

        EditorGUI.PropertyField(position, property, label, true);
    }
}

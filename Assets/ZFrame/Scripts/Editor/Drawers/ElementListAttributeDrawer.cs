using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Editors
{
    [CustomPropertyDrawer(typeof(ElementListAttribute))]
    public class ElementListAttributeDrawer : PropertyDrawer
    {
        private const int BTN_WIDTH = 30;

        private bool DrawArrayTool(Rect position, SerializedProperty arrayProp, int myIndex)
        {
            var deleted = false;
            
            Rect rcButton = position;
            rcButton.height = EditorGUIUtility.singleLineHeight;
            rcButton.x = position.xMax - BTN_WIDTH * 4;
            rcButton.width = BTN_WIDTH;

            EditorGUI.BeginDisabledGroup(myIndex == 0);
            if (GUI.Button(rcButton, "Up", EditorStyles.miniButtonLeft)) {
                arrayProp.MoveArrayElement(myIndex, myIndex - 1);
            }
            EditorGUI.EndDisabledGroup();

            rcButton.x += BTN_WIDTH;
            var isLastIdx = myIndex >= arrayProp.arraySize - 1;
            EditorGUI.BeginDisabledGroup(isLastIdx);
            if (GUI.Button(rcButton, "Dn", EditorStyles.miniButtonMid)) {
                arrayProp.MoveArrayElement(myIndex, myIndex + 1);
            }
            EditorGUI.EndDisabledGroup();

            rcButton.x += BTN_WIDTH;
            if (GUI.Button(rcButton, "Del", EditorStyles.miniButtonMid)) {
                //arrayProp.DeleteArrayElementAtIndex(myIndex);
                //so.ApplyModifiedProperties();
                deleted = true;
            }

            rcButton.x += BTN_WIDTH;
            if (GUI.Button(rcButton, "Ins", EditorStyles.miniButtonRight)) {
                arrayProp.InsertArrayElementAtIndex(myIndex);
            }

            return deleted;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string path = property.propertyPath;
            int arrayInd = path.LastIndexOf(".Array", StringComparison.Ordinal);

            SerializedObject so = property.serializedObject;
            string arrayPath = path.Substring(0, arrayInd);
            SerializedProperty arrayProp = so.FindProperty(arrayPath);

            //Next we need to grab the index from the path string
            int indStart = path.IndexOf("[", StringComparison.Ordinal) + 1;
            int indEnd = path.IndexOf("]", StringComparison.Ordinal);

            string indString = path.Substring(indStart, indEnd - indStart);

            int myIndex = int.Parse(indString);
            
            var deleted = DrawArrayTool(position, arrayProp, myIndex);
            
            if (!property.isExpanded) position.width -= BTN_WIDTH * 4;
            EditorGUI.PropertyField(position, property, label, true);
            
            if (deleted) {
                arrayProp.DeleteArrayElementAtIndex(myIndex);    
                so.ApplyModifiedProperties();
            }
        }
    }
}
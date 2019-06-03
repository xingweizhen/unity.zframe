using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using ZFrame.UGUI;
using ZFrame.Asset;

namespace ZFrame.Editors
{
    [CustomPropertyDrawer(typeof(AssetRefAttribute))]
    public class AssetRefAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var assetRef = (AssetRefAttribute)attribute;
            if (assetRef.name != null) {
                label.text = assetRef.name;
            }

            return EditorGUIUtility.singleLineHeight * (string.IsNullOrEmpty(label.text) ? 1 : 2);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //label = new GUIContent(label);
            var assetRef = (AssetRefAttribute)attribute;
            if (assetRef.name != null) {
                label.text = assetRef.name;
            }

            position.height = EditorGUIUtility.singleLineHeight;
            Object asset = EditorUtil.AssetPathToObject(property.stringValue, assetRef.type);

            Object newObj = null;
            if (string.IsNullOrEmpty(label.text)) {
                label.text = property.stringValue;
                newObj = EditorGUI.ObjectField(position, label, asset, assetRef.type, false);
            } else {
                newObj = EditorGUI.ObjectField(position, label, asset, assetRef.type, false);
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(position, " ", property.stringValue);
            }

            if (newObj != asset) {
                property.stringValue = EditorUtil.ObjectToAssetPath(newObj, assetRef.bundleOnly);
            }
        }
    }
}
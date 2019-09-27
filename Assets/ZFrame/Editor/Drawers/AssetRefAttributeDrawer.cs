using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
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
            var assetRef = (AssetRefAttribute)attribute;
            if (assetRef.name != null) {
                label.text = assetRef.name;
            }

            position.height = EditorGUIUtility.singleLineHeight;
            Object asset = EditorUtil.AssetPathToObject(
                assetRef.mode == 2 ? property.stringValue + "/" : property.stringValue,
                assetRef.type);

            EditorGUI.BeginChangeCheck();
            if (string.IsNullOrEmpty(label.text)) {
                label.text = property.stringValue;
                asset = EditorGUI.ObjectField(position, label, asset, assetRef.type, false);
            } else {
                asset = EditorGUI.ObjectField(position, label, asset, assetRef.type, false);
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(position, " ", property.stringValue);
            }

            if (EditorGUI.EndChangeCheck()) {                
                property.stringValue = EditorUtil.ObjectToAssetPath(asset, assetRef.mode);
            }
        }
    }
}
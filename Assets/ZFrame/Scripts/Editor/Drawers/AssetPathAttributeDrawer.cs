using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Editors
{
	[CustomPropertyDrawer(typeof(AssetPathAttribute))]
	public class AssetPathAttributeDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var assetRef = (AssetPathAttribute)attribute;
			if (!string.IsNullOrEmpty(assetRef.name)) {
				label.text = assetRef.name;
			}

			return EditorGUIUtility.singleLineHeight * (string.IsNullOrEmpty(label.text) ? 1 : 2);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var assetRef = (AssetPathAttribute)attribute;
			if (!string.IsNullOrEmpty(assetRef.name)) {
				label.text = assetRef.name;
			}

			position.height = EditorGUIUtility.singleLineHeight;

			var obj = AssetDatabase.LoadAssetAtPath(property.stringValue, assetRef.type);
			EditorGUI.BeginChangeCheck();
			if (string.IsNullOrEmpty(label.text)) {
				label.text = property.stringValue;
				label.text = property.stringValue;
				obj = EditorGUI.ObjectField(position, label, obj, assetRef.type, false);
			} else {
				obj = EditorGUI.ObjectField(position, label, obj, assetRef.type, false);
				position.y += EditorGUIUtility.singleLineHeight;
				EditorGUI.LabelField(position, " ", property.stringValue);
			}

			if (EditorGUI.EndChangeCheck()) {
				property.stringValue = obj ? AssetDatabase.GetAssetPath(obj) : null;
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using ZFrame.UGUI;

[CustomPropertyDrawer(typeof(AssetRefAttribute))]
public class AssetRefAttributeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        label = new GUIContent(label);
        var assetRef = (AssetRefAttribute)attribute;
        if (!string.IsNullOrEmpty(assetRef.name)) {
            label.text = assetRef.name;
        }

        return EditorGUIUtility.singleLineHeight * (string.IsNullOrEmpty(label.text) ? 1 : 2);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = new GUIContent(label);
    
        var assetRef = (AssetRefAttribute)attribute;
        if (!string.IsNullOrEmpty(assetRef.name)) {
            label.text = assetRef.name;
        }
        
        Object asset = null;
        position.height = EditorGUIUtility.singleLineHeight;
        var assetPath = property.stringValue;
        var atlasRoot = UGUITools.settings.atlasRoot;
        if (!string.IsNullOrEmpty(assetPath)) {
            if (assetPath.OrdinalIgnoreCaseStartsWith(atlasRoot)) {
                if (assetRef.type != typeof(SpriteAtlas) && assetPath[assetPath.Length - 1] != '/') {
                    var spritePath = assetPath.Substring(atlasRoot.Length);
                    asset = UISprite.LoadSprite(spritePath, null);
                } else {
                    asset = ZFrame.Asset.AssetLoader.EditorLoadAsset(assetRef.type, assetPath);
                }
            } else {
                asset = ZFrame.Asset.AssetLoader.EditorLoadAsset(assetRef.type, assetPath);
            }
        }
        
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
            if (newObj == null) {
                property.stringValue = null;
            } else {
                var path = AssetDatabase.GetAssetPath(newObj);
                var ai = AssetImporter.GetAtPath(path);
                if (!string.IsNullOrEmpty(ai.assetBundleName)) {
                    property.stringValue = assetRef.bundleOnly
                        ? ai.assetBundleName + '/'
                        : string.Concat(ai.assetBundleName, "/", newObj.name);
                } else {
                    var sprite = newObj as Sprite;
                    string atlasPath = null, atlasName = null, spriteName = null;
                    if (sprite != null) {
                        atlasPath = UISpriteEditor.GetSpriteAssetRef(sprite, out atlasName, out spriteName);
                    }

                    if (!string.IsNullOrEmpty(atlasPath)) {
                        property.stringValue = assetRef.bundleOnly
                            ? string.Format("{0}{1}/", atlasRoot, atlasName)
                            : string.Format("{0}{1}/{2}", atlasRoot, atlasName, spriteName);
                    } else {
                        property.stringValue = null;
                    }
                }
            }
        }
    }
}

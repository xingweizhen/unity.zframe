using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using ZFrame.UGUI;
using ZFrame.Asset;

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
        
        position.height = EditorGUIUtility.singleLineHeight;
        Object asset = AssetPathToObject(property.stringValue, assetRef.type);
        
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
            property.stringValue = ObjectToAssetPath(newObj, assetRef.bundleOnly);
        }
    }

    private static Object AssetPathToObject(string assetPath, System.Type type)
    {
        var atlasRoot = UGUITools.settings.atlasRoot;
        if (!string.IsNullOrEmpty(assetPath)) {
            if (assetPath.OrdinalIgnoreCaseStartsWith(atlasRoot)) {
                if (type != typeof(SpriteAtlas) && assetPath[assetPath.Length - 1] != '/') {
                    var spritePath = assetPath.Substring(atlasRoot.Length);
                    return UISprite.LoadSprite(spritePath, null);
                } else {
                    return AssetLoader.EditorLoadAsset(type, assetPath);
                }
            } else {
                return AssetLoader.EditorLoadAsset(type, assetPath);
            }
        }
        return null;
    }

    private static string ObjectToAssetPath(Object obj, bool bundleOnly)
    {
        if (obj != null) {
            var atlasRoot = UGUITools.settings.atlasRoot;

            var path = AssetDatabase.GetAssetPath(obj);
            var ai = AssetImporter.GetAtPath(path);
            if (!string.IsNullOrEmpty(ai.assetBundleName)) {
                return bundleOnly
                    ? ai.assetBundleName + '/'
                    : string.Concat(ai.assetBundleName, "/", obj.name);
            } else {
                var sprite = obj as Sprite;
                string atlasPath = null, atlasName = null, spriteName = null;
                if (sprite != null) {
                    atlasPath = UISpriteEditor.GetSpriteAssetRef(sprite, out atlasName, out spriteName);
                }

                if (!string.IsNullOrEmpty(atlasPath)) {
                    return bundleOnly
                        ? string.Format("{0}{1}/", atlasRoot, atlasName)
                        : string.Format("{0}{1}/{2}", atlasRoot, atlasName, spriteName);
                }


            }
        }
        return null;
    }

    public static string Layout(string label, string assetPath, System.Type objType, Object obj = null)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label);
        EditorGUI.BeginChangeCheck();
        obj = EditorGUILayout.ObjectField(AssetPathToObject(assetPath, objType), objType, false);
        if (EditorGUI.EndChangeCheck()) {
            assetPath = ObjectToAssetPath(obj, false);
        }        
        EditorGUILayout.EndHorizontal();
        return assetPath;
    }
}

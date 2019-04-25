using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AssetsMgr))]
public class AssetsMgrEditor : MonoBehaviorEditor 
{
	SerializedProperty printLoadedLuaStack, useLuaAssetBundle, useAssetBundleLoader;

    protected override void OnEnable()
	{
		base.OnEnable();
		
        printLoadedLuaStack = serializedObject.FindProperty("m_PrintLoadedLuaStack");
        useLuaAssetBundle = serializedObject.FindProperty("m_UseLuaAssetBundle");
        useAssetBundleLoader = serializedObject.FindProperty("m_UseAssetBundleLoader");
    }

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI();        
        EditorGUILayout.PropertyField(printLoadedLuaStack, new GUIContent("显示Lua脚本加载顺序"));
		if (Application.isPlaying) {
			EditorGUI.BeginDisabledGroup(true);
		}
        EditorGUILayout.PropertyField(useLuaAssetBundle, new GUIContent("使用打包的Lua脚本"));
		if (useLuaAssetBundle.boolValue) {
			EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle(new GUIContent("使用打包的资源"), true);
			EditorGUI.EndDisabledGroup();
		} else {
            EditorGUILayout.PropertyField(useAssetBundleLoader, new GUIContent("使用打包的资源"));
		}
		serializedObject.ApplyModifiedProperties();
		if (Application.isPlaying) {			
			EditorGUI.EndDisabledGroup();
		}
		
		
	}
}

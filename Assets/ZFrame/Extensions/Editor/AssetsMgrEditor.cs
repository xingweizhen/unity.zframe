﻿using UnityEngine;
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

	private bool OnEditorPrefGUI(string label, string prefKey)
	{
		EditorGUI.BeginChangeCheck();
		var tglValue = EditorGUILayout.Toggle(label, EditorPrefs.GetBool(prefKey));
		if (EditorGUI.EndChangeCheck()) {
			EditorPrefs.SetBool(prefKey, tglValue);
		}

		return tglValue;
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI();

		OnEditorPrefGUI("显示Lua脚本加载顺序", AssetsMgr.kPrintLuaLoading);
		
		EditorGUI.BeginDisabledGroup(Application.isPlaying);
		var useLuaAb = OnEditorPrefGUI("使用打包的Lua脚本", AssetsMgr.kUseLuaAssetBundle);
		if (useLuaAb) {
			EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle("使用打包的资源", true);
			EditorGUI.EndDisabledGroup();
		} else {
			OnEditorPrefGUI("使用打包的资源", AssetsMgr.kUseAssetBundleLoader);
		}
		EditorGUI.EndDisabledGroup();
	}
}

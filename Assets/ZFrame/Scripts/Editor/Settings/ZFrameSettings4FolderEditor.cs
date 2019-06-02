using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace ZFrame.Editors
{
	using Settings;
	[CustomEditor(typeof(ZFrameSettings4Folder), true)]
	public class ZFrameSettings4FolderEditor : Editor
	{
		protected ReorderableList m_Folders;
		protected SerializedProperty m_SettingsList, m_SelSettings;
		protected int m_SelIndex;
		private string[] __SettingsNames;

		protected string[] m_SettingsNames {
			get {
				if (__SettingsNames == null) {
					__SettingsNames = new string[m_SettingsList.arraySize];
					for (var i = 0; i < __SettingsNames.Length; ++i) {
						var settingsName = m_SettingsList.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
						if (string.IsNullOrEmpty(settingsName)) settingsName = "Settings";
						__SettingsNames[i] = string.Format("#{0}-{1}", i, settingsName);
					}
				}

				return __SettingsNames;
			}
		}

		protected virtual void OnEnable()
		{
			m_SettingsList = serializedObject.FindProperty("m_SettingsList");
			m_SelIndex = 0;
			m_SelSettings = m_SettingsList.arraySize > 0 ? m_SettingsList.GetArrayElementAtIndex(0) : null;
            
			m_Folders = new ReorderableList(serializedObject, null, true, false, false, true) {
				headerHeight =  0,
				elementHeight = EditorGUIUtility.singleLineHeight + 2,
				drawElementCallback = (rect, index, isActive, isFocused) => {
					FrameworkSettingsWindow.DrawFolderPath(m_Folders.serializedProperty, index, rect);
				},
			};
			if (m_SelSettings != null) {
				m_Folders.serializedProperty = m_SelSettings.FindPropertyRelative("folders");
			}
		}
		
		protected virtual void UpdateSelectedItem()
		{
			if (m_SelIndex < m_SettingsList.arraySize) {
				m_SelSettings = m_SettingsList.GetArrayElementAtIndex(m_SelIndex);
				m_Folders.serializedProperty = m_SelSettings.FindPropertyRelative("folders");
			} else {
				m_SelSettings = null;
				m_Folders.serializedProperty = null;
			}
		}

		protected virtual void DrawSettingsHeader()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			m_SelIndex = EditorGUILayout.Popup(m_SelIndex, m_SettingsNames, "dropdown");
			if (EditorGUI.EndChangeCheck()) UpdateSelectedItem();

			if (GUILayout.Button("新增", "buttonleft")) {
				m_SelIndex = m_SettingsList.arraySize;
				m_SettingsList.InsertArrayElementAtIndex(m_SelIndex);
				__SettingsNames = null;
				UpdateSelectedItem();
			}

			EditorGUI.BeginDisabledGroup(m_SelIndex >= m_SettingsList.arraySize);
			if (GUILayout.Button("X", "buttonright")) {
				var settingsName = m_SelSettings.FindPropertyRelative("name").stringValue;
				if (EditorUtility.DisplayDialog("删除设置",
					string.Format("是否要删除设置[{0}]，设置删除后将无法恢复。", settingsName),
					"删除", "取消")) {
					m_SettingsList.DeleteArrayElementAtIndex(m_SelIndex);
					if (m_SelIndex == m_SettingsList.arraySize && m_SelIndex > 0) {
						m_SelIndex -= 1;
					}

					__SettingsNames = null;
					UpdateSelectedItem();
				}
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
		}

		protected void DrawSettingsName()
		{
			var settingsName = m_SelSettings.FindPropertyRelative("name");
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(settingsName);
			if (EditorGUI.EndChangeCheck()) {
				__SettingsNames[m_SelIndex] = string.Format("#{0}-{1}", m_SelIndex, settingsName.stringValue);
			}
		}
		
		protected virtual void DrawSettings()
		{
			EditorGUILayout.PropertyField(m_SelSettings, true);
		}

		public override void OnInspectorGUI()
		{
			DrawSettingsHeader();
			if (m_SelSettings != null) {
				EditorGUILayout.Separator();
				DrawSettingsName();
				
				var disableRule = false;
				var disable = m_SelSettings.FindPropertyRelative("m_Disable");
				if (disable != null) {
					EditorGUI.BeginChangeCheck();
					disableRule = !EditorGUILayout.Toggle("启用设置", !disable.boolValue);
					if (EditorGUI.EndChangeCheck()) {
						disable.boolValue = disableRule;
					}
				}
				EditorGUI.BeginDisabledGroup(disableRule);
				DrawSettings();
				EditorGUI.EndDisabledGroup();
				
				EditorGUILayout.Separator();
				DrawFolderList(m_Folders);
			} else { }
            
			serializedObject.ApplyModifiedProperties();
		}

		public static void DrawFolderList(ReorderableList list)
		{
			var array = list.serializedProperty;
			if (array == null) return;

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUI.BeginChangeCheck();
			var folder = EditorGUILayout.ObjectField("添加文件夹", null, typeof(DefaultAsset), true);
			if (EditorGUI.EndChangeCheck()) {
				var path = folder != null ? AssetDatabase.GetAssetPath(folder) : null;
				for (int i = 0; i < array.arraySize; ++i) {
					if (path == array.GetArrayElementAtIndex(i).stringValue) {
						path = null;
					}
				}
				if (!string.IsNullOrEmpty(path)) {
					var insertIndex = array.arraySize;
					array.InsertArrayElementAtIndex(insertIndex);
					array.GetArrayElementAtIndex(insertIndex).stringValue = path;
				}
			}
			
			list.DoLayoutList();
			EditorGUILayout.EndVertical();
		}
	}
}

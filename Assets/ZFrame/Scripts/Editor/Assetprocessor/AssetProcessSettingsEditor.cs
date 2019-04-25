using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ZFrame.Asset
{
    [CustomEditor(typeof(AssetProcessSettings), true)]
    public class AssetProcessSettingsEditor : Editor
    {
        private SerializedProperty m_SettingsList;
        private int m_SettingsIndex;
        private ReorderableList m_Folders;

        private void OnEnable()
        {
            m_SettingsList = serializedObject.FindProperty("m_SettingsList");
            m_Folders = new ReorderableList(serializedObject, null, true, true, true, true) {
                elementHeight = EditorGUIUtility.singleLineHeight + 2,
                drawHeaderCallback = rect => {
                    EditorGUI.LabelField(rect, "适用于以下文件夹：");
                },
                drawElementCallback = (rect, index, isActive, isFocused)=> {
                    var elm = m_Folders.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.LabelField(rect, elm.stringValue);
                },
            };
            m_SettingsIndex = 0;
        }

        private void InspectSettings(SerializedProperty settings)
        {
            var self = (AssetProcessSettings)target;

            var enumType = self.props.GetType();
            var flags = settings.FindPropertyRelative("flags");
            self.props = (System.Enum)System.Enum.ToObject(enumType, flags.intValue);
            flags.intValue = System.Convert.ToInt32(EditorGUILayout.EnumFlagsField("属性管理", self.props));

            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(flags);
            EditorGUI.EndDisabledGroup();
            foreach (var enVal in System.Enum.GetValues(enumType)) {
                var value = System.Convert.ToInt32(enVal);
                if ((flags.intValue & value) != 0) {
                    var enumName = System.Enum.GetName(enumType, enVal);
                    EditorGUILayout.PropertyField(settings.FindPropertyRelative(enumName));
                }
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Separator();            
            var folder = EditorGUILayout.ObjectField("拖入文件夹", null, typeof(DefaultAsset), true);
            string path = folder != null ? AssetDatabase.GetAssetPath(folder) : null;

            EditorGUI.indentLevel++;
            var folders = settings.FindPropertyRelative("folders");
            m_Folders.serializedProperty = folders;            
            for (int i = 0; i < folders.arraySize; ++i) {
                if (path == folders.GetArrayElementAtIndex(i).stringValue) {
                    path = null;
                }
            }
            if (!string.IsNullOrEmpty(path)) {
                var insertIndex = folders.arraySize;
                folders.InsertArrayElementAtIndex(insertIndex);
                folders.GetArrayElementAtIndex(insertIndex).stringValue = path;
            }
            m_Folders.DoLayoutList();
            EditorGUI.indentLevel--;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("新增配置")) {
                m_SettingsIndex = m_SettingsList.arraySize;
                m_SettingsList.InsertArrayElementAtIndex(m_SettingsIndex);
            }
            if(GUILayout.Button(string.Format("删除当前#{0}/{1}", m_SettingsIndex, m_SettingsList.arraySize))) {
                m_SettingsList.DeleteArrayElementAtIndex(m_SettingsIndex);
            }
            if (GUILayout.Button("上一个配置")) {
                m_SettingsIndex -= 1;
            }
            if (GUILayout.Button("下一个配置")) {
                m_SettingsIndex += 1;
            }
            EditorGUILayout.EndHorizontal();

            m_SettingsIndex = Mathf.Clamp(m_SettingsIndex, 0, Mathf.Max(0, m_SettingsList.arraySize - 1));
            if (m_SettingsIndex < m_SettingsList.arraySize) {
                var setttings = m_SettingsList.GetArrayElementAtIndex(m_SettingsIndex);
                InspectSettings(setttings);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}


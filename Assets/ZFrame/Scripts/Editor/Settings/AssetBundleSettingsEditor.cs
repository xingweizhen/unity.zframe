using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace ZFrame
{
    [CustomEditor(typeof(AssetBundleSettings))]
    public class AssetBundleSettingsEditor : Editor
    {
        private ReorderableList m_BUNDLEList, m_CATEGORYList, m_OBOList, m_SCENEList, m_IgnoreList;
        private bool m_Bundle = true, m_Category = true, m_OBO = true, m_SCENE = true, m_Ignore;

        private ReorderableList NewReorderableList(string property)
        {
            var serializedProperty = serializedObject.FindProperty(property);
            return new ReorderableList(serializedObject, serializedProperty, true, false, false, true) {
                drawElementCallback = (rect, index, isActive, isFocus) => {
                    var path = serializedProperty.GetArrayElementAtIndex(index).stringValue;

                    var defColor = GUI.color; 
                    GUI.color = System.IO.Directory.Exists(path) ? defColor : Color.red;
                    EditorGUI.LabelField(rect, path);
                    GUI.color = defColor;
                },
            };
        }

        private void OnEnable()
        {
            m_BUNDLEList = NewReorderableList("m_BUNDLEPaths");
            m_CATEGORYList = NewReorderableList("m_CATEGORYPaths");
            m_OBOList = NewReorderableList("m_OBOPaths");
            m_SCENEList = NewReorderableList("m_SCENEPaths");
            m_IgnoreList = NewReorderableList("m_IgnorePaths");
        }

        private void DrawFolderEditGUI(string name, ReorderableList list, ref bool toggle)
        {
            EditorGUILayout.BeginVertical("GroupBox");
            toggle = EditorGUILayout.Foldout(toggle, name);
            if (toggle) {
                var folder = EditorGUILayout.ObjectField("添加文件夹", null, typeof(DefaultAsset), true);
                string path = folder != null ? AssetDatabase.GetAssetPath(folder) : null;
                var arrayProperty = list.serializedProperty;
                for (int i = 0; i < arrayProperty.arraySize; ++i) {
                    if (path == arrayProperty.GetArrayElementAtIndex(i).stringValue) {
                        path = null;
                    }
                }
                if (!string.IsNullOrEmpty(path)) {
                    var insertIndex = arrayProperty.arraySize;
                    arrayProperty.InsertArrayElementAtIndex(insertIndex);
                    arrayProperty.GetArrayElementAtIndex(insertIndex).stringValue = path;
                }

                list.DoLayoutList();
            }
            EditorGUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            DrawFolderEditGUI("整包资源路径定义", m_BUNDLEList, ref m_Bundle);
            DrawFolderEditGUI("分类资源路径定义", m_CATEGORYList, ref m_Category);
            DrawFolderEditGUI("独立资源路径定义", m_OBOList, ref m_OBO);
            DrawFolderEditGUI("场景资源路径定义", m_SCENEList, ref m_SCENE);

            EditorGUILayout.Separator();
            DrawFolderEditGUI("被忽略的资源路径", m_IgnoreList, ref m_Ignore);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

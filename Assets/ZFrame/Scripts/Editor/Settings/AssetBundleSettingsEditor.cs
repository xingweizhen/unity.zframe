using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.MemoryProfiler;
using UnityEditorInternal;

namespace ZFrame.Editors
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
                headerHeight = 0,
                drawElementCallback = (rect, index, isActive, isFocus) => {
                    FrameworkSettingsWindow.DrawFolderPath(serializedProperty, index, rect);
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
            toggle = EditorGUILayout.Foldout(toggle, name);
            if (toggle) {
                ZFrameSettings4FolderEditor.DrawFolderList(list);
            }
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

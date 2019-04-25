using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace ZFrame.Asset
{
    [CustomEditor(typeof(ArtStandardChecker))]
    public class ArtStandardCheckerEditor : Editor
    {
        private SerializedProperty m_Cfgs;
        private int m_SelIndex;
        private Vector2 m_Scroll;
        private ReorderableList paths;

        private void OnEnable()
        {
            m_Cfgs = serializedObject.FindProperty("m_Cfgs");
            paths = new ReorderableList(serializedObject, null, true, true, true, true) {
                elementHeight = EditorGUIUtility.singleLineHeight + 2,
                drawHeaderCallback = rect => {
                    EditorGUI.LabelField(rect, "适用于以下文件夹：");
                },
                drawElementCallback = (rect, index, isActive, isFocused) => {
                    var elm = paths.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.LabelField(rect, elm.stringValue);
                },
            };
            m_SelIndex = -1;
        }

        private void InspectSettings(SerializedProperty settings)
        {   
            EditorGUILayout.PropertyField(settings.FindPropertyRelative("cfgName"));
            EditorGUILayout.PropertyField(settings.FindPropertyRelative("textureSize"));
            EditorGUILayout.PropertyField(settings.FindPropertyRelative("triangles"));
            EditorGUILayout.PropertyField(settings.FindPropertyRelative("bones"));

            EditorGUILayout.Separator();
            var folder = EditorGUILayout.ObjectField("拖入文件夹", null, typeof(DefaultAsset), true);
            string path = folder != null ? AssetDatabase.GetAssetPath(folder) : null;
            
            var folders = settings.FindPropertyRelative("paths");
            paths.serializedProperty = folders;
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
            paths.DoLayoutList();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var self = (ArtStandardChecker)target;
            
            if (GUILayout.Button("新")) {
                m_SelIndex = m_Cfgs.arraySize;
                m_Cfgs.InsertArrayElementAtIndex(m_SelIndex);
            }
            if (GUILayout.Button("检查所有")) self.Check();

            var bg = GUI.backgroundColor;
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            for (int i = 0; i < m_Cfgs.arraySize; i++) {
                EditorGUILayout.Separator();
                var cfg = m_Cfgs.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                var title = cfg.FindPropertyRelative("cfgName").stringValue;
                if (GUILayout.Toggle(m_SelIndex == i, "#" + i + title, EditorStyles.foldout)) {
                    m_SelIndex = i;
                }

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("删除", EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(false))) {
                    m_Cfgs.DeleteArrayElementAtIndex(i);
                    --i;
                }

                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("检查", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false))) {
                    self.Check(i);
                }

                GUI.backgroundColor = bg;
                EditorGUILayout.EndHorizontal();

                if (m_SelIndex == i) {
                    InspectSettings(cfg);
                }
            }

            EditorGUILayout.EndScrollView();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}

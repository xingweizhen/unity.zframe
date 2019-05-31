using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame
{
    using Asset;
    public class FrameworkSettingsWindow : EditorWindow
    {
        [MenuItem("ZFrame/设置选项...")]
        private static void Open()
        {
            GetWindow<FrameworkSettingsWindow>();
        }

        private class SettingsMenu
        {
            public string name { get; private set; }
            public string type { get; private set; }
            public Editor editor { get; private set; }
            public SettingsMenu(string showName, string typeName)
            {
                name = showName;
                type = typeName;
                var guids = AssetDatabase.FindAssets("t:" + typeName);
                if (guids != null && guids.Length > 0) {
                    editor = Editor.CreateEditor(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0])));
                }
            }

            public SettingsMenu(string showName, ScriptableObject asset)
            {
                name = showName;
                if (asset != null) {
                    editor = Editor.CreateEditor(asset);
                }
            }
        }

        private GUIStyle __SectionItem;
        private GUIStyle m_SectionItem {
            get {
                if (__SectionItem == null) {
                    __SectionItem = new GUIStyle(EditorStyles.toolbarButton);
                    __SectionItem.fixedHeight = 40;
                }
                return __SectionItem;
            }
        }
        
        private string[] m_Menu;
        private SettingsMenu[] m_Settings;
        private int m_MenuIdx;
        private Vector2 m_SectionScroll, m_ContentScroll;

        private void OnEnable()
        {
            m_Settings = new SettingsMenu[] {
                new SettingsMenu("AssetBundle Name", "AssetBundleSettings"),
                new SettingsMenu("Texture Import", "TextureProcessSettings"),
                new SettingsMenu("UGUI Settings", UGUI.UGUITools.settings),
            };
            m_Menu = new string[m_Settings.Length];
            for (var i = 0; i < m_Menu.Length; ++i) m_Menu[i] = m_Settings[i].name;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical("GroupBox", GUILayout.Width(200));
                m_SectionScroll = EditorGUILayout.BeginScrollView(m_SectionScroll);
                m_MenuIdx = GUILayout.SelectionGrid(m_MenuIdx, m_Menu, 1, m_SectionItem);
                EditorGUILayout.EndScrollView();
                GUILayout.FlexibleSpace();                
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("GroupBox");
                {
                    var settings = m_Settings[m_MenuIdx];

                    GUILayout.Label(settings.name, EditorStyles.boldLabel);
                    EditorGUILayout.Separator();

                    m_ContentScroll = EditorGUILayout.BeginScrollView(m_ContentScroll);
                    if (settings.editor) {
                        settings.editor.OnInspectorGUI();
                    } else {
                        GUILayout.Label(string.Format("未找到配置文件<{0}>", settings.type));
                    }
                    EditorGUILayout.EndScrollView();
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}

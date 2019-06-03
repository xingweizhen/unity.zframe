using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace ZFrame.Editors
{
    using Asset;
    using Settings;
    
    public class FrameworkSettingsWindow : EditorWindow
    {
        [MenuItem("ZFrame/设置选项...")]
        private static void Open()
        {
            GetWindow<FrameworkSettingsWindow>("游戏框架设置选项");
        }

        private class SettingsMenu
        {
            public string name { get; private set; }
            public System.Type type { get; private set; }
            public string folder { get; private set; }
            public Editor editor;
            public SettingsMenu(string showName, System.Type assetType, string location)
            {
                name = showName;
                type = assetType;
                folder = location;
                var guids = AssetDatabase.FindAssets("t:" + assetType.Name);
                if (guids != null && guids.Length > 0) {
                    editor = Editor.CreateEditor(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0])));
                }
            }

            public SettingsMenu(string showName, ScriptableObject asset)
            {
                name = showName;
                if (asset != null) {
                    type = asset.GetType();
                    editor = Editor.CreateEditor(asset);
                }
            }
        }

        private GUIStyle __SectionItem;
        private GUIStyle m_SectionItem {
            get {
                if (__SectionItem == null) {
                    __SectionItem = new GUIStyle("LargeButton");
                    __SectionItem.fixedHeight = 40;
                }
                return __SectionItem;
            }
        }

        private GUIStyle __SettingTitle;
        private GUIStyle m_SettingTitle {
            get {
                if (__SettingTitle == null) {
                    __SettingTitle = new GUIStyle("MeTimeLabel");
                    __SettingTitle.fontSize = 24;
                    __SettingTitle.alignment = TextAnchor.MiddleLeft;
                }
                return __SettingTitle;
            }
        }
        
        private string[] m_Menu;
        private SettingsMenu[] m_Settings;
        private int m_MenuIdx;
        private Vector2 m_SectionScroll, m_ContentScroll;

        private void OnEnable()
        {
            m_Settings = new [] {
                new SettingsMenu("UGUI界面设置", typeof(UGUI.UGUISettings), "Resources"),
                new SettingsMenu("UGUI编辑设置", typeof(UGUIEditorSettings), "Editor"),
                new SettingsMenu("AssetBundle Name", typeof(AssetBundleSettings), "Editor"),
                new SettingsMenu("贴图导入属性", typeof(TextureProcessSettings), "Editor"),
                new SettingsMenu("美术资源标准", typeof(ArtStandardChecker), "Editor"),
                new SettingsMenu("应用版本号", typeof(VersionInfo), "Resources"),
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

                    GUILayout.Label(settings.name, m_SettingTitle);
                    GUILayout.Space(30);

                    m_ContentScroll = EditorGUILayout.BeginScrollView(m_ContentScroll);
                    if (settings.editor && settings.editor.target) {
                        settings.editor.OnInspectorGUI();
                    } else {
                        DestroyImmediate(settings.editor);
                        
                        GUILayout.Label(string.Format("未找到配置文件<{0}>", settings.type));
                        if (GUILayout.Button("创建")) {
                            SystemTools.NeedDirectory("Assets/" + settings.folder);
                            var path = string.Format("Assets/{0}/{1}.asset", settings.folder, settings.type.Name);
                            AssetDatabase.CreateAsset(CreateInstance(settings.type), path);
                            settings.editor = Editor.CreateEditor(AssetDatabase.LoadMainAssetAtPath(path));
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawFolderPath(SerializedProperty property, int index, Rect rect)
        {
            var path = property.GetArrayElementAtIndex(index).stringValue;
            var exist = System.IO.Directory.Exists(path);
            if (!exist) path += " (Missing)";
            EditorGUI.LabelField(rect, path, exist ? EditorStyles.label : "ErrorLabel");
        }
    }
}

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
            public Editor editor;
            public SettingsMenu(string showName, System.Type assetType)
            {
                name = showName;
                type = assetType;
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
                new SettingsMenu("UGUI界面设置", UGUI.UGUITools.settings),
                new SettingsMenu("AssetBundle Name", typeof(AssetBundleSettings)),
                new SettingsMenu("贴图导入属性", typeof(TextureProcessSettings)),
                new SettingsMenu("美术资源标准", typeof(ArtStandardChecker)),
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
                            SystemTools.NeedDirectory("Assets/Editor");
                            var path = string.Format("Assets/Editor/{0}.asset", settings.type.Name);
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

        public static bool DrawSettingsHeader(SerializedProperty soArray, ref string[] settingsNames, ref int selectedIdx)
        {
            var changed = false;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            selectedIdx = EditorGUILayout.Popup(selectedIdx, settingsNames, "dropdown");
            changed = EditorGUI.EndChangeCheck();

            if (GUILayout.Button("新增", "buttonleft")) {
                selectedIdx = soArray.arraySize;
                soArray.InsertArrayElementAtIndex(selectedIdx);
                settingsNames = null;
                changed = true;
            }

            EditorGUI.BeginDisabledGroup(selectedIdx >= soArray.arraySize);
            if (GUILayout.Button("X", "buttonright")) {
                soArray.DeleteArrayElementAtIndex(selectedIdx);
                if (selectedIdx == soArray.arraySize && selectedIdx > 0) {
                    selectedIdx -= 1;
                }
                settingsNames = null;
                changed = true;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            return changed;
        }

        public static void DrawFolderList(ReorderableList list)
        {
            var array = list.serializedProperty;
            if (array == null) return;
            
            EditorGUI.BeginChangeCheck();
            var folder = EditorGUILayout.ObjectField("拖入文件夹", null, typeof(DefaultAsset), true);
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

            EditorGUI.indentLevel++;
            list.DoLayoutList();
            EditorGUI.indentLevel--;
        }
    }
}

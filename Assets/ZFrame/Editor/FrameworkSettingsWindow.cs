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

            public SettingsMenu(System.Type assetType)
            {
                type = assetType;
                var asset = GetSettings("t:" + assetType.Name);
                if (asset) editor = Editor.CreateEditor(asset);

                var attr = GetAttribute(assetType);
                if (attr != null) {
                    name = attr.name;
                    folder = attr.location;
                } else {
                    name = type.Name;
                    folder = string.Empty;
                }
            }

            public void OnGUI()
            {
                if (editor && editor.target) {
                    editor.OnInspectorGUI();
                } else {
                    DestroyImmediate(editor);

                    GUILayout.Label(string.Format("未找到配置文件<{0}>", type));
                    if (!string.IsNullOrEmpty(folder)) {
                        var assetPath = string.Format("Assets/{0}/{1}.asset", folder, type.Name);
                        if (GUILayout.Button("创建->" + assetPath)) {
                            SystemTools.NeedDirectory("Assets/" + folder);
                            AssetDatabase.CreateAsset(CreateInstance(type), assetPath);
                            editor = Editor.CreateEditor(AssetDatabase.LoadMainAssetAtPath(assetPath));
                        }
                    }
                }
            }
        }

        private static System.Type[] GetSettingsTypes()
        {
            return new[] {
                    typeof(UGUI.UGUISettings), typeof(UGUIEditorSettings),
                    typeof(AssetLoaderSettings), typeof(AssetBundleSettings),
                    typeof(TextureProcessSettings), typeof(ModelProcessSettings), typeof(AudioProcessSettings),
                    typeof(ArtStandardChecker),
                    typeof(VersionInfo),  typeof(BuildPlayerSettings),
                };
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
            var types = GetSettingsTypes();
            m_Settings = new SettingsMenu[types.Length];
            for (var i = 0; i < types.Length; ++i) {
                m_Settings[i] = new SettingsMenu(types[i]);
            }
            m_Menu = new string[m_Settings.Length];
            for (var i = 0; i < m_Menu.Length; ++i) m_Menu[i] = m_Settings[i].name;
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope()) {
                using (new EditorGUILayout.VerticalScope("GroupBox", GUILayout.Width(200))) {
                    m_SectionScroll = EditorGUILayout.BeginScrollView(m_SectionScroll);
                    m_MenuIdx = GUILayout.SelectionGrid(m_MenuIdx, m_Menu, 1, m_SectionItem);
                    EditorGUILayout.EndScrollView();
                    GUILayout.FlexibleSpace();
                }

                using (new EditorGUILayout.VerticalScope("GroupBox")) {
                    var settings = m_Settings[m_MenuIdx];

                    GUILayout.Label(settings.name, m_SettingTitle);
                    GUILayout.Space(30);

                    m_ContentScroll = EditorGUILayout.BeginScrollView(m_ContentScroll);
                    settings.OnGUI();
                    EditorGUILayout.EndScrollView();

                    GUILayout.FlexibleSpace();
                }
            }
        }

        public static void DrawFolderPath(SerializedProperty property, int index, Rect rect)
        {
            var path = property.GetArrayElementAtIndex(index).stringValue;
            var exist = System.IO.Directory.Exists(path);
            if (!exist) path += " (Missing)";
            EditorGUI.LabelField(rect, path, exist ? EditorStyles.label : "ErrorLabel");
        }

        public static ScriptableObject GetSettings(string filter)
        {
            var guids = AssetDatabase.FindAssets(filter);
            if (guids != null && guids.Length > 0) {
                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    if (settings) return settings;
                }
            }
            return null;
        }

        private static SettingsMenuAttribute GetAttribute(System.Type assetType)
        {
            var attrs = assetType.GetCustomAttributes(typeof(SettingsMenuAttribute), false);
            return attrs != null && attrs.Length > 0 ? attrs[0] as SettingsMenuAttribute : null;
        }

#if UNITY_2018_3_OR_NEWER
        private class ZFrameSettingsProvider : SettingsProvider
        {
            private SettingsMenu m_Menu;

            public ZFrameSettingsProvider(string path, System.Type type) 
                : base(path, SettingsScope.Project, null)
            {
                m_Menu = new SettingsMenu(type);
            }

            public override void OnGUI(string searchContext)
            {
                m_Menu.OnGUI();
            }
        }

        [SettingsProviderGroup]
        private static SettingsProvider[] GetSettingsProviderGroup()
        {
            var types = GetSettingsTypes();
            var providers = new SettingsProvider[types.Length];
            for (var i = 0; i < types.Length; ++i) {
                var attr = GetAttribute(types[i]);
                var menuName = attr != null ? attr.name : types[i].Name;
                providers[i] = new ZFrameSettingsProvider("ZFrame/" + menuName, types[i]);
            }
            return providers;
        }
#endif
    }
}

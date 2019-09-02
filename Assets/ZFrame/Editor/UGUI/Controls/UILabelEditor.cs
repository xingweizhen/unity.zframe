using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TextEditor = UnityEditor.UI.TextEditor;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UILabel), true)]
    [CanEditMultipleObjects]
    public class UILabelEditor : TextEditor
    {
        private static string[] m_LocKeys;
        //private static List<string> m_Results = new List<string>();
        private List<string> m_AutoKeys = new List<string>();

        private SerializedProperty m_Localized, m_Ellipsis, m_RawText, m_Text, m_NonBreakingSpace, supportLinkText;
        private SerializedProperty m_FontPath;

        protected override void OnEnable()
        {
            if (m_LocKeys == null && UILabel.LOC) {
                m_LocKeys = UILabel.LOC.GetKeys();
            }

            m_AutoKeys.Clear();

            base.OnEnable();
            m_Ellipsis = serializedObject.FindProperty("m_Ellipsis");
            m_Localized = serializedObject.FindProperty("m_Localized");
            m_RawText = serializedObject.FindProperty("m_RawText");
            m_Text = serializedObject.FindProperty("m_Text");
            m_FontPath = serializedObject.FindProperty("m_FontPath");
            m_NonBreakingSpace = serializedObject.FindProperty("m_NonBreakingSpace");
            supportLinkText = serializedObject.FindProperty("supportLinkText");
        }

        public override void OnInspectorGUI()
        {
            var self = (UILabel)target;
            var cachedFont = self.font;
            var locText = m_Text.stringValue;
            
            base.OnInspectorGUI();
            if (cachedFont != self.font) {
                var path = AssetDatabase.GetAssetPath(self.font);
                var ai = AssetImporter.GetAtPath(path);
                if (ai != null && !string.IsNullOrEmpty(ai.assetBundleName)) {
                    m_FontPath.stringValue = string.Format("{0}/{1}", ai.assetBundleName, self.font.name);
                } else {
                    m_FontPath.stringValue = null;
                }
            }
            
            EditorGUILayout.LabelField("Extern Properties", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Font Path", 
                string.IsNullOrEmpty(m_FontPath.stringValue) ? "<NOT AssetBundle>" : m_FontPath.stringValue);
            EditorGUILayout.PropertyField(m_NonBreakingSpace);
            EditorGUILayout.PropertyField(m_Ellipsis);
            EditorGUILayout.PropertyField(supportLinkText);
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Localization", EditorStyles.boldLabel);
            if (UILabel.LOC) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Localized, EditorAPI.TempContent("Enable"));

                if (m_Localized.boolValue) {
                    if (locText != m_Text.stringValue) m_AutoKeys.Clear();
                    if (m_AutoKeys.Count == 0 && !m_AutoKeys.Contains(m_Text.stringValue)) {
                        using (var itor = UILabel.LOC.Find(self.text, UGUITools.settings.defaultLang)) {
                            while (itor.MoveNext()) m_AutoKeys.Add(itor.Current);
                        }
                    }

                    EditorGUILayout.PropertyField(m_RawText);
                    if (m_AutoKeys.Count > 0 && !m_AutoKeys.Contains(m_RawText.stringValue)) {
                        var indentLevel = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = 0;

                        var rect = EditorGUILayout.GetControlRect(false, 1);
                        rect.xMin += EditorGUIUtility.labelWidth;
                        EditorGUI.DrawRect(rect, EditorStyles.label.normal.textColor);
                        foreach (var key in m_AutoKeys) {
                            rect = EditorGUILayout.GetControlRect();
                            rect.xMin += EditorGUIUtility.labelWidth;
                            if (EditorGUI.ToggleLeft(rect, EditorAPI.TempContent(key), false)) {
                                m_RawText.stringValue = key;
                            }
                        }
                        EditorGUI.indentLevel = indentLevel;
                    }
                }
                EditorGUI.indentLevel--;
            } else {
                EditorGUILayout.LabelField("本地化文件不存在，无法配置本地化", EditorStyles.helpBox);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

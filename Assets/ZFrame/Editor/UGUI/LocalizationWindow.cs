using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Editors
{
    using UGUI;
    public class LocalizationWindow : EditorWindow
    {
        private Vector2 m_KeyPos;
        private List<string> m_Keys;
        private HashSet<string> m_CustomKeys;
        private string[] m_Langs;
        private int m_SelIdx;
        private bool m_ShowLangsContent = true;

        private void OnEnable()
        {
            m_SelIdx = -1;
            m_Keys = new List<string>(UILabel.LOC.GetKeys());
            m_Keys.Sort((a, b) => string.CompareOrdinal(a, b));
            m_Langs = new string[UILabel.LOC.langs.Length];
            for (var i = 1; i < m_Langs.Length; ++i) {
                m_Langs[i] = string.Format("{0}({1})", UILabel.LOC.Get("@LANG", i), UILabel.LOC.langs[i]);
            }

            m_CustomKeys = new HashSet<string>();
            foreach (var custom in UILabel.LOC.customTexts) m_CustomKeys.Add(custom.key);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            m_KeyPos = EditorGUILayout.BeginScrollView(m_KeyPos, EditorStyles.helpBox, GUILayout.Width(200));            
            for (var i = 0; i < m_Keys.Count; ++i) {
                var txt = m_Keys[i];
                var fontStyle = m_CustomKeys.Contains(m_Keys[i]) ? FontStyle.Bold : FontStyle.Normal;
                var buttonStyle = 
                    m_SelIdx == i ?
                    CustomEditorStyles.FocusedButton(TextAnchor.MiddleLeft, fontStyle) :
                    CustomEditorStyles.NormalButton(TextAnchor.MiddleLeft, fontStyle);
                if (GUILayout.Button(txt, buttonStyle)) {
                    m_SelIdx = i;
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndScrollView();

            if (m_SelIdx >= 0) {
                var key = m_Keys[m_SelIdx];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(key);
                EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), EditorStyles.label.normal.textColor);
                m_ShowLangsContent = EditorGUILayout.ToggleLeft("显示文本内容", m_ShowLangsContent);
                if (m_ShowLangsContent) {
                    EditorGUILayout.Separator();
                    for (var i = 1; i < m_Langs.Length; ++i) {
                        EditorGUILayout.LabelField(m_Langs[i], EditorStyles.boldLabel);
                        EditorGUILayout.TextArea(UILabel.LOC.Get(key, i));
                        EditorGUILayout.Separator();
                    }
                }

                EditorGUILayout.Separator();

                // 转换为大写
                if (GUILayout.Button("字符转为大写")) {
                    var dirty = false;
                    for (var i = 1; i < m_Langs.Length; ++i) {
                        var value = UILabel.LOC.Get(key, i);
                        dirty = UILabel.LOC.Set(key, i, value.ToUpper()) | dirty;
                    }
                    if (dirty) UILabel.LOC.SaveLocalization();
                }
                if (m_CustomKeys.Contains(key)) {
                    EditorGUILayout.Separator();
                    EditorGUILayout.LabelField("预定义文本，代码中会动态引用。");
                }
                EditorGUILayout.Separator();

                // 出现位置
                EditorGUILayout.LabelField("引用了此文本的界面：");
                if (!m_CustomKeys.Contains(key)) {
                    using (var itor = FindTextUsedInUI(key)) {
                        while (itor.MoveNext()) {
                            EditorGUILayout.LabelField(itor.Current);
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            } else {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("选择一个文本来查看详细信息");
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndHorizontal();
        }


        public IEnumerator<string> FindTextUsedInUI(string key)
        {
            foreach (var path in AssetDatabase.GetAssetPathsFromAssetBundle(UGUITools.settings.uiBundlePath)) {
                var prefab = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
                if(prefab) {
                    foreach (var com in prefab.GetComponentsInChildren(typeof(ILabel), true)) {
                        var lb = com as ILabel;
                        if(lb != null && lb.localized && lb.rawText == key) {
                            yield return string.Format("{0}/{1}", prefab.name, com.GetHierarchy(prefab.transform));
                        }
                    }
                }
            }
        }
    }
}

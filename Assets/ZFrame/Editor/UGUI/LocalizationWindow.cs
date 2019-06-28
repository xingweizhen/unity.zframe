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
            m_KeyPos = EditorGUILayout.BeginScrollView(m_KeyPos, GUILayout.Width(200));
            var defColor = GUI.color;
            for (var i = 0; i < m_Keys.Count; ++i) {
                GUI.color = m_SelIdx == i ? Color.yellow : defColor;
                var txt = m_Keys[i];
                if (m_CustomKeys.Contains(m_Keys[i])) txt = "* " + txt;

                if (GUILayout.Button(txt, CustomEditorStyles.LeftToolbar)) {
                    m_SelIdx = i;
                    GUI.FocusControl(null); 
                }
            }
            GUI.color = defColor;
            EditorGUILayout.EndScrollView();

            if (m_SelIdx >= 0) {
                var key = m_Keys[m_SelIdx];
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("KEY: ");
                EditorGUILayout.TextField(key);
                EditorGUILayout.EndHorizontal();
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
                
                // 出现位置
                EditorGUILayout.LabelField("应用于：");
                if (!m_CustomKeys.Contains(key)) {
                    using (var itor = FindTextUsedInUI(key)) {
                        while (itor.MoveNext()) {
                            EditorGUILayout.LabelField(itor.Current);
                        }
                    }
                }
                
                // 转换为大写
                if (GUILayout.Button("字符转为大写")) {
                    var dirty = false;
                    for (var i = 1; i < m_Langs.Length; ++i) {
                        var value = UILabel.LOC.Get(key, i);
                        dirty = UILabel.LOC.Set(key, i, value.ToUpper()) | dirty;
                    }
                    if (dirty) UILabel.LOC.SaveLocalization();
                }

                EditorGUILayout.EndVertical();
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

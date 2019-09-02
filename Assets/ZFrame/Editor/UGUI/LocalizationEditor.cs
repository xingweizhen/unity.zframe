using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.Editors
{
    using UGUI;
    [CustomEditor(typeof(Localization))]
    public class LocalizationEditor : Editor
    {
        private const int VIEW_COUNT = 30;

        private SerializedProperty m_LocalizeText, m_CustomTexts;
        private ReorderableList m_CustomTextsList;

        private readonly List<int> m_ViewList = new List<int>(VIEW_COUNT);
        private int m_Page;
        private bool m_ListDirty;
        private string m_SearchText;

        private void OnEnable()
        {
            m_ListDirty = true;
            m_SearchText = string.Empty;
            m_Page = 0;
            m_LocalizeText = serializedObject.FindProperty("m_LocalizeText");
            m_CustomTexts = serializedObject.FindProperty("m_CustomTexts");

            m_CustomTextsList =
                new ReorderableList(m_ViewList, typeof(Localization.CustomLoc), false, true, true, true) {
                    drawHeaderCallback = (rect) => {
                        rect.width /= 2;
                        //EditorGUI.LabelField(rect, "自定义文本");
                        EditorGUI.LabelField(rect, "KEY");
                        rect.x += rect.width;
                        EditorGUI.LabelField(rect, UGUITools.settings.defaultLang);
                    },
                    drawElementCallback = (rect, index, selected, focused) => {
                        var elmIndex = m_ViewList[index];
                        SerializedProperty element = m_CustomTexts.GetArrayElementAtIndex(elmIndex);

                        rect.y += 2;
                        rect.height = EditorGUIUtility.singleLineHeight;
                        using (new EditorGUI.PropertyScope(rect, null, element)) {
                            var key = element.FindPropertyRelative("key");
                            var value = element.FindPropertyRelative("value");

                            var pos = new Rect(rect.x, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight);
                            key.stringValue = EditorGUI.TextField(pos, key.stringValue);

                            pos.x += pos.width;
                            value.stringValue = EditorGUI.TextField(pos, value.stringValue);
                        }
                    },
                    onAddCallback = list => {
                        int newIndex;
                        if (list.index >= 0 && list.index < m_ViewList.Count) {
                            newIndex = m_ViewList[list.index];
                        } else {
                            if (m_ViewList.Count > 0) {
                                newIndex = m_ViewList[m_ViewList.Count - 1];
                                list.index = m_ViewList.Count - 1;
                            } else {
                                newIndex = 0;
                                list.index = 0;
                            }
                        }
                        m_CustomTexts.InsertArrayElementAtIndex(newIndex);
                        var elm = m_CustomTexts.GetArrayElementAtIndex(newIndex);
                        elm.FindPropertyRelative("key").stringValue = "New Key";
                        elm.FindPropertyRelative("value").stringValue = "";

                        m_ListDirty = true;
                    },
                    onRemoveCallback = list => {
                        if (list.index >= 0 && list.index < m_ViewList.Count) {
                            m_CustomTexts.DeleteArrayElementAtIndex(m_ViewList[list.index]);
                            m_ListDirty = true;
                        }
                    },
                    onChangedCallback = (list) => {

                    },
                };
        }

        private void ExportFromFileButton()
        {
            if (GUILayout.Button("从导出文件同步", EditorStyles.miniButton)) {
                UILabel.LOC = null;
                for (var i = 0; i < m_CustomTexts.arraySize; ++i) {
                    var loc = m_CustomTexts.GetArrayElementAtIndex(i);
                    // ReSharper disable once PossibleNullReferenceException
                    var text = UILabel.LOC.Get(loc.FindPropertyRelative("key").stringValue,
                        UGUITools.settings.defaultLang);
                    if (!string.IsNullOrEmpty(text))
                        loc.FindPropertyRelative("value").stringValue = text;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(m_LocalizeText);
            if (m_LocalizeText.objectReferenceValue == null) {
                if (GUILayout.Button("新建", EditorStyles.miniButton)) {
                    var path = EditorUtility.SaveFilePanel("创建本地化文件", "Assets", "localization", "txt");
                    if (!string.IsNullOrEmpty(path)) {
                        var assetPath = "Assets" + path.Substring(Application.dataPath.Length);
                        System.IO.File.CreateText(assetPath).Dispose();
                        AssetDatabase.Refresh();
                        m_LocalizeText.objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                    }   
                }
            } else {
                if (GUILayout.Button("更新", EditorStyles.miniButtonLeft)) {
                    UGUICreateMenu.UpdateLocConfig();
                    UILabel.LOC = null;

                    m_ListDirty = true;
                    var list = new List<Localization.CustomLoc>(UILabel.LOC.customTexts);
                    list.Sort((a, b) => string.CompareOrdinal(a.key, b.key));
                    UILabel.LOC.customTexts = list.ToArray();
                }
                if (GUILayout.Button("查看", EditorStyles.miniButtonRight)) {
                    UILabel.LOC = null;
                    EditorWindow.GetWindow(typeof(LocalizationWindow));
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            var searchText = m_SearchText;
            m_SearchText = EditorAPI.SearchField(searchText);
            bool searching = !string.IsNullOrEmpty(m_SearchText);

            m_CustomTextsList.displayAdd = !searching;
            m_CustomTextsList.displayRemove = !searching;
            if (!searching) {
                if (m_SearchText != searchText) {
                    m_ListDirty = true;
                }
                var maxPage = Mathf.CeilToInt(m_CustomTexts.arraySize / (float)VIEW_COUNT);
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("自定义文本列表({2}:{0}~{1})",
                    m_Page * VIEW_COUNT + 1, Mathf.Min(m_Page * VIEW_COUNT + VIEW_COUNT, m_CustomTexts.arraySize),
                    m_CustomTexts.arraySize));
                ExportFromFileButton();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(m_Page == 0);
                if (GUILayout.Button("上一页", EditorStyles.miniButtonLeft)) {
                    m_Page -= 1;
                    m_ListDirty = true;
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(m_Page == maxPage - 1);
                if (GUILayout.Button("下一页", EditorStyles.miniButtonMid)) {
                    m_Page += 1;
                    m_ListDirty = true;
                }
                EditorGUI.EndDisabledGroup();

                //if (GUILayout.Button("整理排序", EditorStyles.miniButtonRight)) {
                   
                //}
                EditorGUILayout.EndHorizontal();

                if (m_ListDirty) {
                    m_ListDirty = false;
                    m_ViewList.Clear();
                    var start = m_Page * VIEW_COUNT;
                    for (int i = start; i < Mathf.Min(m_CustomTexts.arraySize, start + VIEW_COUNT); ++i) {
                        m_ViewList.Add(i);
                    }
                }
            } else {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(string.Format("搜索最多显示{0}项结果", VIEW_COUNT));
                ExportFromFileButton();
                EditorGUILayout.EndHorizontal();
                if (m_SearchText != searchText) {
                    m_ViewList.Clear();
                    var lowerSearch = m_SearchText.ToLower();
                    var customTexts = ((Localization)target).customTexts;
                    for (var i = 0; i < customTexts.Length; ++i) {
                        var loc = customTexts[i];
                        var lowerK = loc.key.ToLower();
                        var lowerV = loc.value.ToLower();
                        if (lowerK.Contains(lowerSearch) || lowerV.Contains(lowerSearch)) {
                            m_ViewList.Add(i);
                            if (m_ViewList.Count >= VIEW_COUNT) break;
                        }
                    }
                }
            }
            
            m_CustomTextsList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace ZFrame.Editors
{
    public class MissingScriptWindow : EditorWindow
    {
        [MenuItem("Tools/查找丢失脚本...")]
        private static void Open()
        {
            GetWindow(typeof(MissingScriptWindow));
        }

        private GUIStyle m_SelTitle = "MeTimeLabel";
        private GUIStyle m_LargeBtn = "LargeButton";

        private Dictionary<Object, List<string>> m_Ref = new Dictionary<Object, List<string>>();

        private Vector2 m_PrefabScroll;
        private string m_PrefabName;

        private void FindMissingScripts()
        {
            m_Ref.Clear();

            string[] allassetpaths = AssetDatabase.GetAllAssetPaths();
            var prefabs = allassetpaths
                .Where(a => a.EndsWith("prefab"))
                .Select(a => AssetDatabase.LoadAssetAtPath<GameObject>(a));

            var transList = ListPool<Component>.Get();
            var cps = ListPool<Component>.Get();
            foreach (var prefab in prefabs) {
                if (prefab == null) continue;

                transList.Clear();
                var list = new List<string>();
                prefab.GetComponentsInChildren(typeof(Transform), transList, true);
                foreach (var trans in transList) {
                    cps.Clear();
                    trans.GetComponents(typeof(Component), cps);
                    foreach (var cp in cps) {
                        if (cp == null) {
                            var path = prefab.name;
                            if (trans != prefab.transform) {
                                path = path + "/" + trans.GetHierarchy(prefab.transform);
                            }
                            list.Add(path);
                        }
                    }
                }
                if (list.Count > 0) m_Ref.Add(prefab, list);
            }
            ListPool<Component>.Release(cps);
            ListPool<Component>.Release(transList);
        }

        private void OnGUI()
        {
            GUILayout.Label("MissingScriptWindow", m_SelTitle, GUILayout.ExpandHeight(false));

            var defColor = GUI.color;

            if (m_Ref.Count > 0) {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("刷新", m_LargeBtn)) {
                    FindMissingScripts();
                }
                GUILayout.FlexibleSpace();
                //if (GUILayout.Button("移除所有丢失的脚本", m_LargeBtn)) { }
                EditorGUILayout.EndHorizontal();
            } else {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("刷新", m_LargeBtn)) {
                    FindMissingScripts();
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }

            m_PrefabScroll = EditorGUILayout.BeginScrollView(m_PrefabScroll);
            foreach (var kv in m_Ref) {
                bool selected = m_PrefabName == kv.Key.name;
                GUI.color = selected  ? Color.yellow : defColor;
                if (GUILayout.Button(kv.Key.name, EditorStyles.toolbarDropDown)) {
                    m_PrefabName = kv.Key.name;
                    Selection.activeGameObject = kv.Key as GameObject;
                }
                if (selected) {
                    foreach (var path in kv.Value) {
                        GUILayout.Label("    " + path);
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            GUI.color = defColor;
        }
    }
}

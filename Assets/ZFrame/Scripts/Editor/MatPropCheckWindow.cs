﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace ZFrame.Editors
{
    public class MatPropCheckWindow : EditorWindow
    {
        [MenuItem("Tools/材质球纹理检查...")]
        private static void ShowWindow()
        {
            GetWindow(typeof(MatPropCheckWindow)).Show();
        }

        private Dictionary<Material, Dictionary<string, Object>> m_Ref = new Dictionary<Material, Dictionary<string, Object>>();
        private Dictionary<Shader, List<string>> m_ShaderTexEnvs = new Dictionary<Shader, List<string>>();

        private List<string> GetShaderTexEnvs(Shader shader)
        {
            List<string> list;
            if (!m_ShaderTexEnvs.TryGetValue(shader, out list)) {
                list = new List<string>();
                for (var i = 0; i < ShaderUtil.GetPropertyCount(shader); ++i) {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv) {
                        list.Add(ShaderUtil.GetPropertyName(shader, i));
                    }   
                }
                m_ShaderTexEnvs.Add(shader, list);
            }

            return list;
        }

        private Dictionary<string, Object> GetMaterialTexEnvs(Material mat)
        {
            var dict = new Dictionary<string, Object>();

            SerializedObject so = new SerializedObject(mat);            
            var iter = so.GetIterator();
            while (iter.NextVisible(true)) {
                if (iter.propertyPath.Contains("m_TexEnvs")) {
                    var arr = iter.FindPropertyRelative("Array");
                    for (int i = 0; i < arr.FindPropertyRelative("size").intValue; i++) {
                        var data = arr.FindPropertyRelative("data[" + i + "]");
                        var o = data.FindPropertyRelative("second").FindPropertyRelative("m_Texture").objectReferenceValue;
                        if (o != null) {
                            dict.Add(data.FindPropertyRelative("first").stringValue, o);
                        }
                    }
                    break;
                }
            }

            return dict;
        }

        private void RemoveMaterialTexEnv(Material mat, string propName)
        {
            SerializedObject so = new SerializedObject(mat);
            var iter = so.GetIterator();
            while (iter.NextVisible(true)) {
                if (iter.propertyPath.Contains("m_TexEnvs")) {
                    var arr = iter.FindPropertyRelative("Array");
                    for (int i = 0; i < arr.arraySize; i++) {
                        var data = arr.GetArrayElementAtIndex(i);
                        if (data.FindPropertyRelative("first").stringValue == propName) {
                            arr.DeleteArrayElementAtIndex(i);
                            so.ApplyModifiedProperties();
                            break;
                        }
                    }
                    break;
                }
            }
        }

        private void FindMaterias()
        {
            m_Ref.Clear();

            var mats = AssetDatabase.GetAllAssetPaths()
                .Where(s => s.EndsWith(".mat"))
                .Select(s => AssetDatabase.LoadAssetAtPath<Material>(s));

            foreach (var mat in mats) {
                if (mat.shader == null) continue;

                var propNams = GetShaderTexEnvs(mat.shader);
                var dict = GetMaterialTexEnvs(mat);
                foreach (var propName in dict.Keys) {
                    if (!propNams.Contains(propName)) {
                        m_Ref.Add(mat, dict);
                        break;
                    }
                }
            }
        }

        private void RemoveAllInvalidTexEnv()
        {
            foreach (var kv in m_Ref) {
                var mat = kv.Key;
                var propNams = GetShaderTexEnvs(mat.shader);

                foreach (var prop in kv.Value) {
                    var contains = propNams.Contains(prop.Key);
                    if (!contains) {
                        RemoveMaterialTexEnv(mat, prop.Key);
                    }
                }
            }
        }

        private Vector2 m_RefScroll;

        private void OnGUI()
        {
            var defColor = GUI.color;
            if (GUILayout.Button("刷新")) {
                FindMaterias();
            }
            if (GUILayout.Button("移除所有无效的纹理属性")) {
                RemoveAllInvalidTexEnv();
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            m_RefScroll = EditorGUILayout.BeginScrollView(m_RefScroll);
            Material selectedMat = null;
            Dictionary<string, Object> texProps = null;
            foreach (var res in m_Ref) {
                var selected = Selection.activeObject == res.Key;
                GUI.color = selected ? Color.yellow : defColor;
                if (GUILayout.Button(res.Key.name)) {
                    Selection.activeObject = res.Key;
                    selectedMat = res.Key;
                    texProps = res.Value;
                } else if (selected) {
                    selectedMat = res.Key;
                    texProps = res.Value;
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            if (selectedMat != null) {
                var propNams = GetShaderTexEnvs(selectedMat.shader);
                foreach (var prop in texProps) {
                    var contains = propNams.Contains(prop.Key);
                    GUI.color = contains ? defColor : Color.red;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(prop.Key, CustomEditorStyles.titleStyle, GUILayout.Width(400));
                    EditorGUILayout.LabelField(prop.Value.name);
                    if (!contains && GUILayout.Button("Del")) {
                        texProps.Remove(prop.Key);
                        RemoveMaterialTexEnv(selectedMat, prop.Key);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();                    
                }
                GUI.color = defColor;

                if (GUILayout.Button("删除所有多余的纹理引用")) {
                    var list = new List<string>();
                    foreach (var prop in texProps) {
                        if (prop.Value == null) continue;

                        if (!propNams.Contains(prop.Key)) {
                            RemoveMaterialTexEnv(selectedMat, prop.Key);
                            list.Add(prop.Key);
                        }
                    }
                    foreach (var propName in list) texProps.Remove(propName);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            GUI.color = defColor;
        }
    }
}

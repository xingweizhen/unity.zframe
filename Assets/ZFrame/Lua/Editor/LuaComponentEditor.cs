using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ZFrame.Editors
{
    using Lua;
    [CustomEditor(typeof(LuaComponent), true)]
    public class LuaComponentEditor : UGUI.UIWindowEditor
    {
        private ReorderableList m_MethodList;
        private SerializedProperty LocalMethods;

        protected override void OnEnable()
        {
            base.OnEnable();

            LocalMethods = serializedObject.FindProperty("LocalMethods");
            m_MethodList = new ReorderableList(serializedObject, LocalMethods, true, true, true, true) {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement
            };
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Lua脚本实现的函数列表");
        }

        private void DrawElement(Rect rect, int index, bool selected, bool focused)
        {
            SerializedProperty element = LocalMethods.GetArrayElementAtIndex(index);

            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, element, GUIContent.none);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Separator();

            var self = (LuaComponent)target;

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("luaScript"));
            m_MethodList.DoLayoutList();
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();

            if (!Application.isPlaying) {
                EditorGUILayout.Separator();
                if (GUILayout.Button("生成脚本...", CustomEditorStyles.richTextBtn)) {
                    LuaUIGenerator.ShowWindow();
                }
            }
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UIWindow), true)]
    [CanEditMultipleObjects()]
    public class UIWindowEditor : Editor
    {
        private ReorderableList m_PreloadList;
        private SerializedProperty m_PreloadAssets;

        protected virtual void OnEnable()
        {
            m_PreloadAssets = serializedObject.FindProperty("m_PreloadAssets");
            m_PreloadList = new ReorderableList(serializedObject, m_PreloadAssets, true, true, true, true) {
                elementHeight = EditorGUIUtility.singleLineHeight + 2,
                drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "依赖资源"),
                drawElementCallback = (rect, index, selected, focused) => {
                    var element = m_PreloadAssets.GetArrayElementAtIndex(index);
                    rect.y += 2;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                },
            };
        }

        protected void DrawDepthInspector()
        {
            var self = target as UIWindow;
            EditorGUILayout.LabelField("窗口层级 @ [" + self.depth + "]", EditorStyles.boldLabel);
        }

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI();
            DrawDepthInspector();
            EditorGUILayout.Separator();

            m_PreloadList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

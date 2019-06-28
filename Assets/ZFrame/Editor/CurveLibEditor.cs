using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections;
using System.IO;

namespace ZFrame.Editors
{
    [CustomEditor(typeof(CurveLib))]
    public class CurveLibEditor : Editor
    {
        private ReorderableList m_CurvesList;

        private void OnEnable()
        {
            var curves = serializedObject.FindProperty("m_Curves");

            m_CurvesList = new ReorderableList(serializedObject, curves, true, true, true, true);
            m_CurvesList.drawHeaderCallback = DrawNamedCurveHeader;
            m_CurvesList.drawElementCallback = DrawNamedCurveElement;
            m_CurvesList.elementHeight = (EditorGUIUtility.singleLineHeight + 2) * 2 + 10;
        }

        private void DrawNamedCurveHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "命名曲线库");
        }

        private void DrawNamedCurveElement(Rect rect, int index, bool selected, bool focused)
        {
            SerializedProperty element = m_CurvesList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;
            rect.height = m_CurvesList.elementHeight;

#if UNITY_5
        using (new EditorGUI.PropertyScope(rect, null, element)) {
#else
            {
                EditorGUI.BeginProperty(rect, null, element);
#endif
                var lbW = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 50;

                var name = element.FindPropertyRelative("name");
                var curve = element.FindPropertyRelative("curve");

                var pos = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                name.stringValue = EditorGUI.TextField(pos, "名称", name.stringValue);

                pos.y += pos.height + 2;
                curve.animationCurveValue = EditorGUI.CurveField(pos, "曲线", curve.animationCurveValue);

                EditorGUIUtility.labelWidth = lbW;
#if !UNITY_5
                EditorGUI.EndProperty();
#endif
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            m_CurvesList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
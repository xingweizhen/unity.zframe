using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    [CustomEditor(typeof(Fading), true)]
    [CanEditMultipleObjects()]
    public class FadingEditor : InteractFadeEditor
    {
        private SerializedProperty m_Group;
        private SerializedProperty m_Target;
        private SerializedProperty m_Ease;
        private SerializedProperty m_Duration;
        private SerializedProperty m_Delay;
        private SerializedProperty m_Loops;
        private SerializedProperty m_LoopType;
        private SerializedProperty m_IgnoreTimescale;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Group = serializedObject.FindProperty("m_Group");
            m_Target = serializedObject.FindProperty("m_Target");
            m_Ease = serializedObject.FindProperty("m_Ease");
            m_Duration = serializedObject.FindProperty("m_Duration");
            m_Delay = serializedObject.FindProperty("m_Delay");
            m_Loops = serializedObject.FindProperty("m_Loops");
            m_LoopType = serializedObject.FindProperty("m_LoopType");
            m_IgnoreTimescale = serializedObject.FindProperty("m_IgnoreTimescale");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Tween Settings", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;

            EditorGUILayout.PropertyField(m_Group);

            EditorGUILayout.PropertyField(m_Target);

            EditorGUILayout.PropertyField(m_Ease);

            EditorGUILayout.PropertyField(m_Duration);

            EditorGUILayout.PropertyField(m_Delay);

            EditorGUILayout.PropertyField(m_Loops);

            EditorGUILayout.PropertyField(m_LoopType);

            DrawFadeMask();

            EditorGUILayout.PropertyField(m_IgnoreTimescale);

            --EditorGUI.indentLevel;

            if (Application.isPlaying) {
                if (GUILayout.Button("Animate")) {
                    (target as Fading).DOFade(true, true);
                }
            } else {
                if (GUILayout.Button("Apply")) {
                    (target as Fading).Apply();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

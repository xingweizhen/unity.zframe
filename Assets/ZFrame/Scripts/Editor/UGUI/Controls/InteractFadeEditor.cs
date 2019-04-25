using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.UGUI
{
    [CustomEditor(typeof(InteractFade), true)]
    [CanEditMultipleObjects()]
    public class InteractFadeEditor : Editor 
    {
        private string[] AutoFadeNAME;
        private SerializedProperty m_Fadein, m_Fadeout;

        protected virtual void OnEnable()
        {
            var list = new List<string>();
            var type = typeof(AutoFade);
            var fields = type.GetFields();
            for (int i = 0; i < fields.Length; ++i) {
                var f = fields[i];
                if (f.FieldType == type) list.Add(f.Name);
            }
            AutoFadeNAME = list.ToArray();

            m_Fadein = serializedObject.FindProperty("m_Fadein");
            m_Fadeout = serializedObject.FindProperty("m_Fadeout");
        }

        protected void DrawFadeMask()
        {
            m_Fadein.intValue = EditorGUILayout.MaskField("Fadein Mask", m_Fadein.intValue, AutoFadeNAME);
            m_Fadeout.intValue = EditorGUILayout.MaskField("Fadeout Mask", m_Fadeout.intValue, AutoFadeNAME);
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Tween Settings", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            DrawFadeMask();
            --EditorGUI.indentLevel;

            serializedObject.ApplyModifiedProperties();
        }
    }
}

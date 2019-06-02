using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace ZFrame.Editors
{
    using Settings;
    [CustomEditor(typeof(ArtStandardChecker))]
    public class ArtStandardCheckerEditor : ZFrameSettings4FolderEditor
    {
        protected override void DrawSettings()
        {   
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_SelSettings.FindPropertyRelative("textureSize"));
            EditorGUILayout.PropertyField(m_SelSettings.FindPropertyRelative("triangles"));
            EditorGUILayout.PropertyField(m_SelSettings.FindPropertyRelative("bones"));
            EditorGUI.indentLevel--;
            
        }

        public override void OnInspectorGUI()
        {   
            var self = (ArtStandardChecker)target;
            
            if (GUILayout.Button("检查所有", "LargeButton")) self.Check();
            EditorGUILayout.Separator();

            base.OnInspectorGUI();
            if (m_SelSettings != null) {
                EditorGUILayout.Separator();
                if (GUILayout.Button("检查", "button")) {
                    self.Check(m_SelIndex);
                }
            }
        }
    }
}

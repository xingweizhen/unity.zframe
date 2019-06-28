using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Editors
{
    using Asset;
    [CustomEditor(typeof(VersionInfo))]
    public class VersionInfoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var self = target as VersionInfo;
            EditorGUILayout.LabelField("版本", string.Format("{0}({1})", self.version, self.code));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Version"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Separator();
            self.major = EditorGUILayout.IntField("Major", self.major);
            self.minor = EditorGUILayout.IntField("Minor", self.minor);
            self.build = EditorGUILayout.IntField("Build", self.build);
            self.revision = EditorGUILayout.IntField("Revision", self.revision);      
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Code"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}

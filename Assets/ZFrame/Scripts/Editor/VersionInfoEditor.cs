using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Asset
{
    [CustomEditor(typeof(VersionInfo))]
    public class VersionInfoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var self = target as VersionInfo;
            EditorGUILayout.LabelField("版本", string.Format("{0}({1})", self.version, self.code));
        }
    }
}

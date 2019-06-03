using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UIImageTextMixed), true)]
    [CanEditMultipleObjects]
    public class UIImageTextMixedEditor : UILabelEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AtlasPath"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}

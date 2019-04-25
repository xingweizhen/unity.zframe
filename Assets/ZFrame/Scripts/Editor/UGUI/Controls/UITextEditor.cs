using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZFrame.UGUI
{
#if USING_TMP
	[CustomEditor(typeof(UIText))]
	public class UITextEditor : Editor
	{
		private SerializedProperty m_RawText;
		private List<string> m_AutoKeys = new List<string>();
		private string m_Value;
		
		private void OnEnable()
		{
            m_AutoKeys.Clear();
            m_Value = ((UIText)target).text;
			
			m_RawText = serializedObject.FindProperty("m_RawText");
		}

		public override void OnInspectorGUI()
		{
			var self = (UIText)target;
			
			base.OnInspectorGUI();

			if (self.localized) {
				if (self.text != m_Value) {
					m_Value = self.text;
                    m_AutoKeys.Clear();
				}
				
				if (m_AutoKeys.Count == 0 && !string.IsNullOrEmpty(self.text)) {
                    using (var itor = UILabel.LOC.Find(self.text, UGUITools.settings.defaultLang)) {
                        while (itor.MoveNext()) m_AutoKeys.Add(itor.Current);
                    }
                }
                EditorGUILayout.PropertyField(m_RawText);
                if (m_AutoKeys.Count > 0 && !m_AutoKeys.Contains(m_RawText.stringValue)) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("已存在的键");
                    foreach (var  key in m_AutoKeys) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(key);
                        if (GUILayout.Button("*", EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
                            m_RawText.stringValue = key;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
					EditorGUI.indentLevel--;
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
#endif
}

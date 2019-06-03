using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(SpriteSizeFitter))]
    [CanEditMultipleObjects]
    public class SpriteSizeFitterEditor : Editor
    {
        private SerializedProperty m_AspectRadio, m_MinWidth, m_MinHeight;

        private void OnEnable()
        {
            m_AspectRadio = serializedObject.FindProperty("m_AspectRadio");
            m_MinWidth = serializedObject.FindProperty("m_MinWidth");
            m_MinHeight = serializedObject.FindProperty("m_MinHeight");
        }

        private void ReadonlyFloat(SerializedProperty property, float value, string content)
        {
            Rect position = EditorGUILayout.GetControlRect();
            var label = EditorGUI.BeginProperty(position, new GUIContent(content), property);
            Rect fieldPosition = EditorGUI.PrefixLabel(position, label);
            
            EditorGUI.LabelField(fieldPosition, value.ToString());
            EditorGUI.EndProperty();
        }

        private void InspectFloat(SerializedProperty property, float chkValue, float defValue, float minValue, string content)
        {
            Rect position = EditorGUILayout.GetControlRect();
            var label = EditorGUI.BeginProperty(position, new GUIContent(content), property);
            Rect fieldPosition = EditorGUI.PrefixLabel(position, label);

            Rect toggleRect = fieldPosition;
            toggleRect.width = 16;

            Rect floatFieldRect = fieldPosition;
            floatFieldRect.xMin += 16;

            EditorGUI.BeginChangeCheck();
            var enabled = EditorGUI.ToggleLeft(toggleRect, GUIContent.none, property.floatValue > chkValue);
            if (EditorGUI.EndChangeCheck()) {
                property.floatValue = enabled ? defValue : chkValue;
            }
            if (!property.hasMultipleDifferentValues && property.floatValue > chkValue) {
                //EditorGUI.FloatField(floatFieldRect, property.floatValue);
                EditorGUIUtility.labelWidth = 4; // Small invisible label area for drag zone functionality
                EditorGUI.BeginChangeCheck();
                float newValue = EditorGUI.FloatField(floatFieldRect, new GUIContent(" "), property.floatValue);
                if (EditorGUI.EndChangeCheck()) {
                    property.floatValue = Mathf.Max(minValue, newValue);
                }
                EditorGUIUtility.labelWidth = 0;
            }
            EditorGUI.EndProperty();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var self = target as SpriteSizeFitter;
            var rect = self.rectTransform.rect;
            InspectFloat(m_AspectRadio, 0, 1, 0.01f, "Aspect Radio");
            switch (self.aspectMode) {
                case AspectRatioFitter.AspectMode.None:
                    ReadonlyFloat(m_MinWidth, rect.width, "Fit Width");
                    ReadonlyFloat(m_MinHeight, rect.height, "Fit Height");
                    break;
                case AspectRatioFitter.AspectMode.WidthControlsHeight:
                    EditorGUILayout.PropertyField(m_MinWidth);
                    ReadonlyFloat(m_MinHeight, rect.height, "Fit Height");
                    break;
                case AspectRatioFitter.AspectMode.HeightControlsWidth:
                    ReadonlyFloat(m_MinWidth, rect.width, "Fit Width");
                    EditorGUILayout.PropertyField(m_MinHeight);
                    break;
                default:
                    EditorGUILayout.LabelField(string.Format("{0} mode has no effect.", self.aspectMode), EditorStyles.boldLabel);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

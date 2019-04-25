using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    using Asset;
    [CustomEditor(typeof(UITexture)), CanEditMultipleObjects]
    public class UITextureEditor : GraphicEditor
    {
        private SerializedProperty m_Texture;
        private SerializedProperty m_TexPath;
        private SerializedProperty m_UVRect;
        private GUIContent m_UVRectContent;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Texture = serializedObject.FindProperty("m_Texture");
            m_TexPath = serializedObject.FindProperty("m_TexPath");
            m_UVRectContent = new GUIContent("UV Rect");
            m_UVRect = serializedObject.FindProperty("m_UVRect");
            SetShowNativeSize(((UITexture)target).mainTexture, true);
        }

        public override void OnInspectorGUI()
        {
            var self = (UITexture)target;

            serializedObject.Update();
            if (!Application.isPlaying && m_Texture.objectReferenceValue) {
                if (GUILayout.Button("移除序列化的Texture")) {
                    m_Texture.objectReferenceValue = null;
                }
            }

            AppearanceControlsGUI();
            RaycastControlsGUI();
            EditorGUILayout.PropertyField(m_UVRect, m_UVRectContent);
            SetShowNativeSize(self.mainTexture, false);
            NativeSizeButtonGUI();

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(m_TexPath);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Type"), new GUIContent("Image Type"));
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Main Texture", self.mainTexture, typeof(Texture), false);
            EditorGUI.EndDisabledGroup();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}

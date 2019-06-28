using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;
using System.Reflection;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UITexture)), CanEditMultipleObjects]
    public class UITextureEditor : RawImageEditor
    {
        private SerializedProperty m_TexPath;
        private SerializedProperty __Texture;
        private SerializedProperty __UVRect;
        private GUIContent __UVRectContent;

        protected override void OnEnable()
        {
            base.OnEnable();

            var baseType = typeof(RawImageEditor);

            m_TexPath = serializedObject.FindProperty("m_TexPath");

            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            __Texture = serializedObject.FindProperty("m_Texture");
            __UVRect = serializedObject.FindProperty("m_UVRect");
            __UVRectContent = baseType.GetField("m_UVRectContent", flags).GetValue(this) as GUIContent;// EditorGUIUtility.TrTextContent("UV Rect");
            SetShowNativeSize(((UITexture)target).mainTexture, true);
        }

        public override void OnInspectorGUI()
        {
            var self = (UITexture)target;

            serializedObject.Update();
            if (!Application.isPlaying && __Texture.objectReferenceValue) {
                if (GUILayout.Button("移除序列化的Texture")) {
                    __Texture.objectReferenceValue = null;
                }
            }

            AppearanceControlsGUI();
            RaycastControlsGUI();
            EditorGUILayout.PropertyField(__UVRect, __UVRectContent);
            SetShowNativeSize(self.mainTexture, true);
            NativeSizeButtonGUI();

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(m_TexPath);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Type"), new GUIContent("Image Type"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}

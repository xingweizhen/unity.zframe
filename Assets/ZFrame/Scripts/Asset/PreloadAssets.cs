using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace ZFrame.Asset
{
    [CreateAssetMenu(menuName ="资源库/预加载资源")]
    public class PreloadAssets : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private PreloadAsset[] m_Preloads;
        public PreloadAsset[] assets { get { return m_Preloads; } }

#if UNITY_EDITOR
        [CustomEditor(typeof(PreloadAssets))]
        class SelfEditor : Editor
        {
            private ReorderableList m_List;

            private void OnEnable()
            {
                m_List = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Preloads"), true, true, true, true) {
                    elementHeight = EditorGUIUtility.singleLineHeight * 2 + 2,
                    drawElementCallback = (rect, index, isActive, isFocused) => {
                        rect.height = EditorGUIUtility.singleLineHeight;
                        var elm = m_List.serializedProperty.GetArrayElementAtIndex(index);
                        EditorGUI.PropertyField(rect, elm.FindPropertyRelative("path"));

                        rect.y = rect.yMax;
                        EditorGUI.PropertyField(rect, elm.FindPropertyRelative("method"));
                    },
                };
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                m_List.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }

    [System.Serializable]
    public class PreloadAsset
    {
        [AssetRef("", bundleOnly = true)]
        public string path;
        public LoadMethod method;
        public PreloadAsset(string path, LoadMethod method)
        {
            this.path = path;
            this.method = method;
        }

        public static bool operator ==(PreloadAsset a, PreloadAsset b)
        {
            var nulla = ReferenceEquals(a, null);
            var nullb = ReferenceEquals(b, null);
            if (nulla || nullb) {
                return nulla == nullb;                
            }
            return string.CompareOrdinal(a.path, b.path) == 0;
        }

        public static bool operator !=(PreloadAsset a, PreloadAsset b)
        {
            var nulla = ReferenceEquals(a, null);
            var nullb = ReferenceEquals(b, null);
            if (nulla || nullb) {
                return nulla != nullb;
            }

            return string.CompareOrdinal(a.path, b.path) != 0;
        }

        public override bool Equals(object obj)
        {
            var self = obj as PreloadAsset;
            return self != null && string.CompareOrdinal(self.path, path) == 0;
        }

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }
    }
}

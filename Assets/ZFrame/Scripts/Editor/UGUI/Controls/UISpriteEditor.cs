using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.U2D;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UISprite))]
    [CanEditMultipleObjects]
    public class UISpriteEditor : ImageEditor
    {
        private bool m_Grayscale;
        private SerializedProperty m_AtlasName, m_SpriteName;
        
        private SerializedProperty m_PreserveAspect, m_Type;

        private static System.Type SpriteEditorWindowType;

        public static void OpenSpriteEditor(SpriteAtlas atlas, Sprite sprite)
        {
            if (SpriteEditorWindowType == null) {
                SpriteEditorWindowType = typeof(EditorWindow).Assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == "SpriteEditorWindow");
            }

            var spriteName = sprite.name;
            if (spriteName.EndsWith("(Clone)")) {
                spriteName = spriteName.Substring(0, spriteName.Length - 7);
            }
            var path = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(atlas))
                .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == spriteName);
    
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture>(path);
            EditorWindow.GetWindow(SpriteEditorWindowType);
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();

            m_AtlasName = serializedObject.FindProperty("m_AtlasName");
            m_SpriteName = serializedObject.FindProperty("m_SpriteName");
            m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");
            m_Type = serializedObject.FindProperty("m_Type");
        }
        
        public static SpriteAtlas FindAtlas(string bundleName, string atlasName)
        {
            var atlasPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, atlasName);
            if (atlasPaths != null && atlasPaths.Length > 0) {
                return AssetDatabase.LoadAssetAtPath(atlasPaths[0], typeof(SpriteAtlas)) as SpriteAtlas;
            }
            return null;
        }

        private void UpdateSprite(Sprite newSprite)
        {
            var self = (UISprite)target;
            self.overrideSprite = newSprite;
            if (newSprite) {
                Image.Type oldType = (Image.Type)m_Type.enumValueIndex;
                if (newSprite.border.SqrMagnitude() > 0) {
                    m_Type.enumValueIndex = (int)Image.Type.Sliced;
                } else if (oldType == Image.Type.Sliced) {
                    m_Type.enumValueIndex = (int)Image.Type.Simple;
                }
            }
        }
        
        private void OnSpriteChanged(string spriteName)
        {
            m_SpriteName.stringValue = spriteName;
            
            var self = (UISprite)target;
            UpdateSprite(self.atlas.GetSprite(spriteName));
            serializedObject.ApplyModifiedProperties();
        }

        private bool OverrideSpriteGUI()
        {
            var self = (UISprite)target;
            EditorGUILayout.BeginHorizontal();

            bool changed = false;
            EditorGUI.BeginChangeCheck();
            var newSprite = EditorGUILayout.ObjectField(self.overrideSprite, typeof(Sprite), false) as Sprite;
            if (EditorGUI.EndChangeCheck()) {
                UpdateSprite(newSprite);
                if (newSprite) {
                    changed = true;
                } else {
                    self.overrideSprite = null;
                    m_SpriteName.stringValue = null;
                }
            }

            EditorGUI.BeginDisabledGroup(self.atlas == null);
            if (GUILayout.Button("Pick", EditorStyles.miniButtonLeft, GUILayout.Width(40))) {
                SpriteSelector.Show(self.atlas, m_SpriteName.stringValue, OnSpriteChanged);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(self.overrideSprite == null);
            if (GUILayout.Button("Edit", EditorStyles.miniButtonMid, GUILayout.Width(40))) {
                OpenSpriteEditor(self.atlas, self.overrideSprite);
            }

            if (GUILayout.Button("Del", EditorStyles.miniButtonRight, GUILayout.Width(40))) {
                self.overrideSprite = null;
                m_SpriteName.stringValue = null;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            return changed;
        }

        private void BaseInspectorGUI()
        {
            var self = (UISprite)target;
            
            serializedObject.Update();
            //SpriteGUI();
            AppearanceControlsGUI();
            RaycastControlsGUI();
            TypeGUI();
            ++EditorGUI.indentLevel;
            EditorGUILayout.PropertyField(m_PreserveAspect);
            --EditorGUI.indentLevel;

            var imgType = (Image.Type)m_Type.enumValueIndex;
            var showNative = self.overrideSprite && (imgType == Image.Type.Simple || imgType == Image.Type.Filled);
            SetShowNativeSize(true, true);
            EditorGUI.BeginDisabledGroup(!showNative);
            NativeSizeButtonGUI();
            EditorGUI.EndDisabledGroup();
        }

        private void AtlasGUI()
        {
            var atlasRoot = UGUITools.settings.atlasRoot;
            var self = (UISprite)target;
            
            var atlasName = m_AtlasName.stringValue;
            if (!string.IsNullOrEmpty(atlasName) && self.atlas == null) {
                foreach (var bundleName in AssetDatabase.GetAllAssetBundleNames()) {
                    if (bundleName.OrdinalStartsWith(atlasRoot)) {
                        if (bundleName.OrdinalIgnoreCaseEndsWith(atlasName)) {
                            self.atlas = FindAtlas(bundleName, atlasName);
                            break;
                        }
                    }
                }
            }

            EditorGUILayout.BeginHorizontal();
            var atlas = EditorGUILayout.ObjectField(self.atlas, typeof(SpriteAtlas), false) as SpriteAtlas;
            if (GUILayout.Button("SpriteAtlas", EditorStyles.miniButton, GUILayout.Width(120))) {
                EditorGUIUtility.ShowObjectPicker<SpriteAtlas>(self.atlas, false, string.Empty, 101);   
            }

            EditorGUILayout.EndHorizontal();

            if (Event.current.commandName == "ObjectSelectorUpdated") {
                var ctlId = EditorGUIUtility.GetObjectPickerControlID();
                if (ctlId == 101) {
                    atlas = EditorGUIUtility.GetObjectPickerObject() as SpriteAtlas;
                }
            }
            
            if (atlas != self.atlas) {
                self.atlas = atlas;
                self.overrideSprite = null;
                if (atlas) {
                    m_AtlasName.stringValue = atlas ? atlas.name : null;
                } else {
                    m_AtlasName.stringValue = null;
                    m_SpriteName.stringValue = null;
                }
            }
        }
       
        public override void OnInspectorGUI()
        {
            var self = (UISprite)target;

            /*
            if (self.overrideSprite && string.IsNullOrEmpty(m_SpriteName.stringValue)) {
                self.overrideSprite = null;
            }
            //*/
            
            self.sprite = self.overrideSprite;
            //base.OnInspectorGUI();
            BaseInspectorGUI();
            self.sprite = null;

            if (Application.isPlaying) {
                var grayscale = UGUITools.IsGrayscale(self.material);
                m_Grayscale = EditorGUILayout.Toggle("Grayscale", grayscale);
                if (grayscale != m_Grayscale) {
                    m_Material.objectReferenceValue = UGUITools.ToggleGrayscale(self.material, m_Grayscale);
                }
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Sprite Settings", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;

            AtlasGUI();
            
            if (OverrideSpriteGUI()) {
                var overrideSp = self.overrideSprite;
                self.sprite = overrideSp;
                if (overrideSp) {
                    string packingTag, spriteName;
                    var atlasPath = GetSpriteAssetRef(overrideSp, out packingTag, out spriteName);
                    m_SpriteName.stringValue = spriteName;
                    m_AtlasName.stringValue = packingTag;
                    self.atlas = FindAtlas(atlasPath, packingTag);
                } else {
                    m_SpriteName.stringValue = null;
                    m_AtlasName.stringValue = null;
                    self.atlas = null;
                }
            }
            
            /*
            if (!Application.isPlaying) {
                if (self.atlas && !string.IsNullOrEmpty(m_SpriteName.stringValue) && self.overrideSprite == null) {
                    self.overrideSprite = self.atlas.GetSprite(m_SpriteName.stringValue);
                    if (self.overrideSprite == null) {
                        //m_SpriteName.stringValue = null;
                    }
                }
            }
            //*/

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_AtlasName);
            EditorGUILayout.PropertyField(m_SpriteName);
            EditorGUI.EndDisabledGroup();
            --EditorGUI.indentLevel;

            serializedObject.ApplyModifiedProperties();
        }
        
        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            Image image = target as Image;
            if (image == null) return;

            Sprite sf = image.overrideSprite;
            if (sf == null) return;

            DrawSprite(sf, rect, image.canvasRenderer.GetColor(), background);
        }

        public override string GetInfoString()
        {
            Image image = (Image)target;
            Sprite sprite = image.overrideSprite;

            int x = (sprite != null) ? Mathf.RoundToInt(sprite.rect.width) : 0;
            int y = (sprite != null) ? Mathf.RoundToInt(sprite.rect.height) : 0;

            return string.Format("Image Size: {0}x{1}", x, y);
        }

        public static void DrawSprite(Sprite sprite, Rect rect, Color color, GUIStyle background)
        {
            // 1 获取程序集
            var asm = System.Reflection.Assembly.GetAssembly(typeof(ImageEditor));
            if (asm == null) {
                return;
            }

            //2 UnityEditor 内部类
            var sduType = asm.GetType("UnityEditor.UI.SpriteDrawUtility");
            var args = new object[] { sprite, rect, color };
            sduType.InvokeMember("DrawSprite", System.Reflection.BindingFlags.InvokeMethod, null, null, args);
        }
        
        public static string GetSpriteAssetRef(Sprite sprite, out string atlasName, out string spriteName)
        {
            string atlasPath = null, packingTag = null;
            spriteName = sprite != null ? sprite.name : null;
            
            var ti = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
            if (ti == null) goto END_FINDING_ATLAS;
            
            var atlasRoot = UGUITools.settings.atlasRoot;
            var listAtlas = new List<string>();
            foreach (var bundleName in AssetDatabase.GetAllAssetBundleNames()) {
                if (bundleName.OrdinalStartsWith(atlasRoot)) {
                    listAtlas.Add(bundleName);
                }
            }
            foreach (var atlasBundle in listAtlas) {
                var assets = AssetDatabase.GetAssetPathsFromAssetBundle(atlasBundle);
                foreach (var asset in assets) {
                    if (asset.OrdinalEndsWith(".spriteatlas")) {
                        var sprites = AssetDatabase.GetDependencies(asset);
                        foreach (var sp in sprites) {
                            if (sp == ti.assetPath) {
                                atlasPath = atlasBundle;
                                packingTag = System.IO.Path.GetFileNameWithoutExtension(asset);
                                goto END_FINDING_ATLAS;
                            }
                        }
                    }
                }
            }

            END_FINDING_ATLAS:
            atlasName = packingTag;
            return atlasPath;
        }
        
    }
}

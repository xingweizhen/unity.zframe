using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZFrame.Tween
{
    [TweenMenu("Others/Material Property", "Material Property")]
    public sealed class TweenMaterialProperty : TweenComponent<Renderer, Vector4>
    {
        private static MaterialPropertyBlock __Block;
        public static MaterialPropertyBlock sharedBlock { get { if (__Block == null) __Block = new MaterialPropertyBlock(); return __Block; } }
        public static void SharedPropertyBlock(MaterialPropertyBlock block) { __Block = block; }

        public enum PropertyType { Color, Vector, Float, Range, TexEnv }

        [SerializeField]
        private PropertyType m_PropertyType;

        [SerializeField]
        private string m_PropertyName;

        private int _PropertyId = -1;
        public int propertyId {
            get {
                if (_PropertyId < 0 && !string.IsNullOrEmpty(m_PropertyName)) {
                    _PropertyId = Shader.PropertyToID(m_PropertyName);
                }
                return _PropertyId;
            }
        }

        public void SetProperty(string propName, PropertyType propType)
        {
            m_PropertyName = propName;
            m_PropertyType = propType;
            _PropertyId = -1;
        }

        protected override void OnEnable()
        {
            _PropertyId = -1;

            base.OnEnable();
        }

        private float GetFloat()
        {
            sharedBlock.Clear();
            target.GetPropertyBlock(sharedBlock);
            return sharedBlock.GetFloat(propertyId);
        }

        private void SetFloat(float value)
        {
            sharedBlock.Clear();
            target.GetPropertyBlock(sharedBlock);
            sharedBlock.SetFloat(propertyId, value);
            target.SetPropertyBlock(sharedBlock);
        }

        private Vector4 GetVector()
        {
            sharedBlock.Clear();
            target.GetPropertyBlock(sharedBlock);
            return sharedBlock.GetVector(propertyId);
        }

        private void SetVector(Vector4 value)
        {
            sharedBlock.Clear();
            target.GetPropertyBlock(sharedBlock);
            sharedBlock.SetVector(propertyId, value);
            target.SetPropertyBlock(sharedBlock);
        }

        private void SetCurrentValue()
        {
            switch (m_PropertyType) {
                case PropertyType.Float:
                case PropertyType.Range:
                    m_From.x = GetFloat();
                    break;
                case PropertyType.Color:
                case PropertyType.Vector:
                case PropertyType.TexEnv:
                    m_From = GetVector();
                    break;

            }
        }
        public override void ResetStatus()
        {
            if (target && target.sharedMaterial && target.sharedMaterial.HasProperty(m_PropertyName)) {
                SetCurrentValue();
            } else {
                m_From = Vector4.one;
            }
            m_To = m_From;
        }

        protected override ZTweener StartTween(bool reset, bool forward)
        {
            if (target) {
                switch (m_PropertyType) {
                    case PropertyType.Float:
                    case PropertyType.Range:
                        SetFloat(m_From.x);
                        return target.Tween(GetFloat, SetFloat, m_To.x, duration);
                    case PropertyType.Color:
                    case PropertyType.Vector:
                    case PropertyType.TexEnv:
                        SetVector(m_From);
                        return target.Tween(GetVector, SetVector, m_To, duration);
                }
            }

            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _PropertyId = -1;
            var twGrp = FindGroup();
            if (twGrp) {

            }
        }

        [CustomEditor(typeof(TweenMaterialProperty))]
        private class MyEditor : TweenEditor
        {
            private static List<Material> _materials = new List<Material>();

            private struct Property
            {
                public string name;
                public ShaderUtil.ShaderPropertyType type;
            }

            protected override float m_ValueHeight {
                get {
                    var self = target as TweenMaterialProperty;
                    if (string.IsNullOrEmpty(self.m_PropertyName)) {
                        return base.m_ValueHeight;
                    } else {
                        switch (self.m_PropertyType) {
                            default:
                                return base.m_ValueHeight;
                            case PropertyType.Vector:
                            case PropertyType.TexEnv:
                                return EditorGUIUtility.singleLineHeight * 4 + 6;
                        }
                    }
                }
            }

            protected override string GetTweenName()
            {
                var self = target as TweenMaterialProperty;
                return self.foldout ? base.GetTweenName() : base.GetTweenName() + "." + self.m_PropertyName;
            }

            protected override void OnPropertiesGUI()
            {
                var self = target as TweenMaterialProperty;

                string propertyName = string.Empty;
                if (!string.IsNullOrEmpty(self.m_PropertyName)) {
                    if (self.m_PropertyType != PropertyType.TexEnv) {
                        propertyName = string.Format("{0} ({1})", self.m_PropertyName, self.m_PropertyType);
                    } else {
                        propertyName = string.Format("{0} (Scale and Offest)", self.m_PropertyName);
                    }
                } else {
                    propertyName = "<No Property>";
                }

                var rect = EditorGUILayout.GetControlRect();
                rect = EditorGUI.PrefixLabel(rect, new GUIContent("Property"));
                if (GUI.Button(rect, propertyName, EditorStyles.layerMaskField)) {
                    var propertyMenu = new GenericMenu();
                    if (self.target) {
                        var properties = new HashSet<Property>();
                        _materials.Clear();
#if UNITY_2018_2_OR_NEWER
                        self.target.GetSharedMaterials(_materials);
#else
                        _materials.AddRange(self.target.sharedMaterials);
#endif
                        for (var i = 0; i < _materials.Count; ++i) {
                            if (_materials[i] && _materials[i].shader) {
                                var shader = _materials[i].shader;
                                var nProp = ShaderUtil.GetPropertyCount(shader);
                                for (var iProp = 0; iProp < nProp; ++iProp) {
                                    var prop = new Property() {
                                        name = ShaderUtil.GetPropertyName(shader, iProp),
                                        type = ShaderUtil.GetPropertyType(shader, iProp),
                                    };

                                    if (properties.Contains(prop)) continue;
                                    properties.Add(prop);

                                    var desc = ShaderUtil.GetPropertyDescription(shader, iProp);

                                    if (prop.type == ShaderUtil.ShaderPropertyType.TexEnv) {
                                        prop.name += "_ST";
                                        desc += " Scale and Offest";
                                    }
                                    propertyMenu.AddItem(
                                        new GUIContent(string.Format("{0} (\"{1}\", {2})", prop.name, desc, prop.type)),
                                        self.m_PropertyName == prop.name && (int)self.m_PropertyType == (int)prop.type,
                                        () => {
                                            Undo.RecordObject(self, "Select Property");
                                            var oldType = self.m_PropertyType;
                                            self.SetProperty(prop.name, (PropertyType)(int)prop.type);

                                            if (oldType != self.m_PropertyType) {
                                                self.SetCurrentValue();
                                            }
                                        });
                                }
                            }
                        }
                    }
                    if (propertyMenu.GetItemCount() == 0)
                        propertyMenu.AddItem(new GUIContent("(No Valid Property)"), false, () => { });

                    propertyMenu.DropDown(rect);
                }
            }

            protected override void OnValueNameGUI(Rect rect)
            {
                var self = target as TweenMaterialProperty;
                if (string.IsNullOrEmpty(self.m_PropertyName)) {
                    EditorGUI.LabelField(rect, "NONE");
                } else {
                    switch (self.m_PropertyType) {
                        case PropertyType.Float:
                        case PropertyType.Range:
                            EditorGUI.LabelField(rect, "Float");
                            break;
                        case PropertyType.Color:
                            EditorGUI.LabelField(rect, "Color");
                            break;
                        case PropertyType.Vector:
                            rect.height = EditorGUIUtility.singleLineHeight;
                            EditorGUI.LabelField(rect, "X");

                            rect.y = rect.yMax + 2;
                            EditorGUI.LabelField(rect, "Y");

                            rect.y = rect.yMax + 2;
                            EditorGUI.LabelField(rect, "Z");

                            rect.y = rect.yMax + 2;
                            EditorGUI.LabelField(rect, "W");
                            break;
                        case PropertyType.TexEnv:
                            rect.height = EditorGUIUtility.singleLineHeight;
                            EditorGUI.LabelField(rect, "Tiling X");

                            rect.y = rect.yMax + 2;
                            EditorGUI.LabelField(rect, "Tiling Y");

                            rect.y = rect.yMax + 2;
                            EditorGUI.LabelField(rect, "Offset X");

                            rect.y = rect.yMax + 2;
                            EditorGUI.LabelField(rect, "Offset Y");
                            break;
                    }
                }
            }

            protected override void OnValueGUI(SerializedProperty property, Rect rect, string content)
            {
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 14;

                var self = target as TweenMaterialProperty;
                if (string.IsNullOrEmpty(self.m_PropertyName)) {
                    EditorGUI.LabelField(rect, string.Empty);
                } else {
                    switch (self.m_PropertyType) {
                        case PropertyType.Float: {
                                EditorGUI.BeginChangeCheck();
                                var v4 = property.vector4Value;
                                v4.x = EditorGUI.FloatField(rect, content, v4.x);
                                if (EditorGUI.EndChangeCheck()) {
                                    property.vector4Value = v4;
                                }
                            }
                            break;
                        case PropertyType.Range: {
                                EditorGUI.BeginChangeCheck();
                                var v4 = property.vector4Value;
                                v4.x = EditorGUI.FloatField(rect, content, v4.x);
                                if (EditorGUI.EndChangeCheck()) {
                                    property.vector4Value = v4;
                                }
                            }
                            break;
                        case PropertyType.Color: {
                                EditorGUI.BeginChangeCheck();
                                var v4 = property.vector4Value;
                                var color = EditorGUI.ColorField(rect, content, new Color(v4.x, v4.y, v4.z, v4.w));
                                if (EditorGUI.EndChangeCheck()) {
                                    property.vector4Value = new Vector4(color.r, color.g, color.b, color.a);
                                }
                            }
                            break;
                        case PropertyType.Vector:
                        case PropertyType.TexEnv: {
                                var v4 = property.vector4Value;
                                EditorGUI.BeginChangeCheck();
                                rect.height = EditorGUIUtility.singleLineHeight;
                                v4.x = EditorGUI.FloatField(rect, content, v4.x);

                                rect.y = rect.yMax + 2;
                                v4.y = EditorGUI.FloatField(rect, content, v4.y);

                                rect.y = rect.yMax + 2;
                                v4.z = EditorGUI.FloatField(rect, content, v4.z);

                                rect.y = rect.yMax + 2;
                                v4.w = EditorGUI.FloatField(rect, content, v4.w);
                                if (EditorGUI.EndChangeCheck()) {
                                    property.vector4Value = v4;
                                }
                            }
                            break;
                    }
                }
                EditorGUIUtility.labelWidth = labelWidth;
            }
        }
#endif
    }
}

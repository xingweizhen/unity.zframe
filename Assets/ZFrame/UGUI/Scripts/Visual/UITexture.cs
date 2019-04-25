using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    using Asset;
    using Tween;

    public class UITexture : RawImage, ITweenable
    {
        public static Texture LoadTexture(string path, bool warnIfMissing)
        {
#if UNITY_EDITOR
            if (AssetLoader.Instance == null) {
                string bundleName, assetName;
                AssetLoader.GetAssetpath(path, out bundleName, out assetName);
                var paths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, assetName);
                if (paths != null && paths.Length > 0) {
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(paths[0]);
                }

                return null;
            }
#endif
            return AssetLoader.Instance.Load(typeof(Texture), path, warnIfMissing) as Texture;
        }

        [SerializeField, AssetRef(type: typeof(Texture))]
        private string m_TexPath;
        public string texPath { get { return m_TexPath; } }

        private Texture m_OverrideTex;

        public override Texture mainTexture {
            get { return m_OverrideTex ? m_OverrideTex : texture; }
        }

        [SerializeField]
        private Image.Type m_Type;
        public Image.Type type {
            get { return m_Type; }
            set {
                m_Type = value;
            }
        }

        public bool grayscale {
            get { return material == UGUITools.ToggleGrayscale(material, true); }
            set { material = UGUITools.ToggleGrayscale(material, value); }
        }

        private static readonly DelegateObjectLoaded OnTextureLoaded = (a, o, p) => {
            var uiTex = (UITexture)p;
            if (string.CompareOrdinal(a, uiTex.m_TexPath) != 0) return;
            
            uiTex.SetOverrideTex(o as Texture);
            if (uiTex.m_OverrideTex == null) {
                LogMgr.W("Load <Texture> Fail! path = \"{0}\"", a);
            }
        };

        private void InitTexture()
        {
            if (!string.IsNullOrEmpty(m_TexPath)) {
                var tex = LoadTexture(m_TexPath, false);
                if (tex == null) {
                    m_OverrideTex = null;
                    SetVerticesDirty();
                    SetMaterialDirty();
#if UNITY_EDITOR
                    if (AssetLoader.Instance == null) {
                        LogMgr.W("{0}: Load <Texture> Fail! path = '{1}'", this.GetHierarchy(), m_TexPath);
                        return;
                    }
#endif
                    AssetLoader.Instance.LoadAsync(typeof(Texture), m_TexPath, LoadMethod.Cache, OnTextureLoaded, this);
                } else {
                    OnTextureLoaded(m_TexPath, tex, this);
                }
            } else {
                SetOverrideTex(null);
            }
        }

        private void SetOverrideTex(Texture tex)
        {
            if (m_OverrideTex != tex) {
                m_OverrideTex = tex;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        public void SetTexture(string path)
        {
            m_TexPath = path;
            InitTexture();
        }

        protected void ImageTypeChanged()
        {
            var tex = mainTexture;
            if (tex == null) return;

            switch (type) {
                case Image.Type.Tiled: {
                        var uv = uvRect;
                        Vector2 size = rectTransform.rect.size;
                        uv.size = new Vector2(size.x / tex.width, size.y / tex.height);
                        uvRect = uv;
                    }
                    break;
                case Image.Type.Filled: {
                        var uv = uvRect;
                        Vector2 size = rectTransform.rect.size;
                        float uvx = 0, uvy = 0;
                        var uvw = Mathf.Min(1f, size.x / tex.width);
                        var uvh = Mathf.Min(1f, size.y / tex.height);
                        var pivot = rectTransform.pivot;
                        if (pivot.x > 0.5f) {
                            uvx = 1 - uvw;
                        } else if (pivot.x == 0.5f) {
                            uvx = (1 - uvw) / 2f;
                        }

                        if (pivot.y > 0.5f) {
                            uvy = 1 - uvh;
                        } else if (pivot.y == 0.5f) {
                            uvy = (1 - uvh) / 2f;
                        }
                        uvRect = new Rect(uvx, uvy, uvw, uvh);
                    }
                    break;
                case Image.Type.Simple:
                    // 不会修改uvRect
                    break;
                default:
                    LogMgr.W("{0} is not support for UITexture.", type);
                    break;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            enabled = true;
        }

        protected override void Start()
        {
#if UNITY_EDITOR
            UGUITools.AutoUIRoot(this);
#endif
            base.Start();
        }

        private void ResetTexture()
        {
            if (string.IsNullOrEmpty(m_TexPath)) {
                SetOverrideTex(null);
            } else if (m_OverrideTex == null) {
                InitTexture();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetTexture();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            ImageTypeChanged();
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (mainTexture) {
                base.OnPopulateMesh(toFill);
            } else {
                toFill.Clear();
            }
        }

        protected override void OnCanvasHierarchyChanged()
        {
            if (isActiveAndEnabled && canvas && canvas.enabled) {
                ResetTexture();
            }
            base.OnCanvasHierarchyChanged();
        }

        private Vector2 GetUVOffset() { return uvRect.position; }
        private void SetUVOffset(Vector2 position)
        {
            var uv = uvRect;
            uv.position = position;
            uvRect = uv;
        }

        public ZTweener Tween(object from, object to, float duration)
        {
            ZTweener tw = null;
            if (to is Color) {
                tw = this.TweenColor((Color)to, duration);
                if (from is Color) {
                    tw.StartFrom((Color)from);
                }
            } else if (to is float) {
                tw = this.TweenAlpha((float)to, duration);
                if (from is float) {
                    var fromColor = color;
                    fromColor.a = (float)from;
                    color = fromColor;
                    tw.StartFrom(color);
                }
            } else if (to is Vector2) {
                tw = this.Tween(GetUVOffset, SetUVOffset, (Vector2)to, duration);
                if (from is Vector2) {
                    tw.StartFrom((Vector2)from);
                }
            } else if (to is Vector4) {
            }
            if (tw != null) tw.SetTag(this);
            return tw;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!AssetBundleLoader.I) {
                InitTexture();
            }
            ImageTypeChanged();
        }
#endif

    }

}

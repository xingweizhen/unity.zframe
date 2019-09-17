using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using System.Collections;
using System.Collections.Generic;
//using NoToLua = XLua.BlackListAttribute;

namespace ZFrame.UGUI
{
    using Asset;
    using Tween;

    public class UISprite : Image, ITweenable, ITweenable<float>, ITweenable<Color>
    {
        private static readonly Dictionary<string, SpriteAtlas> LoadedAtlas = new Dictionary<string, SpriteAtlas>();

        private static SpriteAtlas GetAtlas(string atlasPath, string atlasName)
        {
            SpriteAtlas ret = null;
            if (LoadedAtlas.TryGetValue(atlasName, out ret)) {
                if (ret != null) return ret;
                LoadedAtlas.Remove(atlasName);
            }

            AbstractAssetBundleRef abRef;
            if (AssetLoader.Instance.TryGetAssetBundle(atlasPath, out abRef)) {
                ret = abRef.Load(atlasName, typeof(SpriteAtlas)) as SpriteAtlas;
                if (ret != null) LoadedAtlas.Add(atlasName, ret);
            }

            return ret;
        }

        //[NoToLua]
        public static SpriteAtlas LoadAtlas(string atlasName, Object context)
        {
            SpriteAtlas ret = null;
            
            var atlasRoot = UGUITools.settings.atlasRoot;
            var atlasRef = UGUITools.settings.atlasRef;
            if (atlasRef) {
                atlasName = UGUITools.settings.atlasRef.GetRef(atlasName);
            }
            
            if (AssetLoader.Instance == null) {
#if UNITY_EDITOR
                var atlasPath = string.Format("{0}{1}/{1}", atlasRoot, atlasName);
                ret = AssetLoader.EditorLoadAsset(null, atlasPath) as SpriteAtlas;
#endif
            } else {
                ret = GetAtlas((atlasRoot + atlasName).ToLower(), atlasName);
            }

            if (context && ret == null) {
                var com = context as Component;
                LogMgr.W(context, "{0}: Load Atlas[{1}] fail!", 
                    com ? com.GetHierarchy() : string.Empty, atlasName);
            }

            return ret;
        }

        //[NoToLua]
        public static Sprite LoadSprite(string path, Object context)
        {
            var atlasName = SystemTools.GetDirPath(path);
            var spriteName = System.IO.Path.GetFileName(path);
            var atlas = LoadAtlas(atlasName, context);
            return atlas ? atlas.GetSprite(spriteName) : null;
        }

        protected SpriteAtlas m_Atlas;
        public SpriteAtlas atlas {
            get { return m_Atlas; }
            set {
                if (m_Atlas != value) {
                    m_Atlas = value;
                    if (value) {
                        overrideSprite = value.GetSprite(m_SpriteName);
                    }
                }
            }
        }

        [SerializeField]
        protected string m_AtlasName;
        public string atlasName {
            get { return m_Atlas ? m_Atlas.name : m_AtlasName; }
            set {
                m_AtlasName = value;
                atlas = LoadAtlas(value, this);
            }
        }

        [SerializeField]
        protected string m_SpriteName;
        public string spriteName {
            get { return m_SpriteName; }
            set {
                m_SpriteName = value;
                if (m_Atlas) overrideSprite = m_Atlas.GetSprite(value);
            }
        }
        
        public bool grayscale {
            get { return material == UGUITools.ToggleGrayscale(material, true); }
            set { material = UGUITools.ToggleGrayscale(material, value); }
        }

        private void OnSpriteLoaded(string a, object o, object param)
        {
            overrideSprite = o as Sprite;
            if (!overrideSprite) {
                LogMgr.W(this, "{0}: Load <Sprite> Fail! path = '{1}'", this.GetHierarchy(null), a);
            }
        }
                
        public void SetSprite(string path, bool warnIfMissing)
        {
            if (!string.IsNullOrEmpty(path)) {
                m_AtlasName = SystemTools.GetDirPath(path);
                m_SpriteName = System.IO.Path.GetFileName(path);
            } else {
                m_AtlasName = null;
                m_SpriteName = null;
            }

            if (string.IsNullOrEmpty(m_AtlasName)) {
                m_Atlas = null;
                overrideSprite = null;
            } else if (string.IsNullOrEmpty(m_SpriteName)) {
                overrideSprite = null;
            } else {
                m_Atlas = LoadAtlas(m_AtlasName, warnIfMissing ? this : null);
                if (m_Atlas) {
                    overrideSprite = m_Atlas.GetSprite(m_SpriteName);
                    if (overrideSprite == null && warnIfMissing) {
                        LogMgr.W("Load <Sprite:{0}> Fail!", path);
                    }
                }
            }
        }
        public void SetSprite(string path)
        {   
#if UNITY_EDITOR
            SetSprite(path, false);
#else
            SetSprite(path, true);
#endif
        }

        protected void AutoLoadSprite()
        {
            if ((overrideSprite == null || overrideSprite.texture == null) && m_Atlas && !string.IsNullOrEmpty(m_SpriteName)) {
                overrideSprite = m_Atlas.GetSprite(m_SpriteName);
                if (overrideSprite == null) {
                    LogMgr.W(this, "Load <Sprite:{0}/{1}> Fail! @ {2}", m_AtlasName, m_SpriteName, this.GetHierarchy());
#if UNITY_EDITOR
                    if (Application.isPlaying)
#endif
                        m_SpriteName = null;
                }
            }
        }

        protected void InitAtlasSprite()
        {
            if (m_Atlas == null && !string.IsNullOrEmpty(m_AtlasName)) {
                m_Atlas = LoadAtlas(m_AtlasName, null);
                if (m_Atlas == null) {
                    LogMgr.D(this, "{0}: Atlas[{1}] NOT loaded.", this.GetHierarchy(), atlasName);
                    overrideSprite = null;
                }
            }

            AutoLoadSprite();
        }

        //[NoToLua]
        public void Load(string path, DelegateObjectLoaded onLoaded = null, object param = null)
        {
            if (string.IsNullOrEmpty(path) || path.EndsWith('/')) {
                overrideSprite = null;
                if (onLoaded != null) {
                    onLoaded.Invoke(path, null, param);
                }
            } else {
                var loadedCfg = OnSpriteLoaded + onLoaded;
                AssetLoader.Instance.LoadAsync(typeof(Sprite), path, LoadMethod.Cache, loadedCfg, param);
            }
        }

        public override void SetNativeSize()
        {
            if (overrideSprite) {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, overrideSprite.rect.width);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, overrideSprite.rect.height);
            }
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (overrideSprite) {
                base.OnPopulateMesh(toFill);                
            } else {
                toFill.Clear();
            }
        }

        protected override void OnCanvasHierarchyChanged()
        {
            if (isActiveAndEnabled && canvas && canvas.enabled) {
                InitAtlasSprite();
            }

            base.OnCanvasHierarchyChanged();
        }

        #region Tweenable
        public object Tween(float to, float duration)
        {
            return this.TweenFill(to, duration).SetTag(this);
        }

        public object Tween(float from, float to, float duration)
        {
            return this.TweenFill(from, to, duration).SetTag(this);
        }

        public object Tween(Color to, float duration)
        {
            return this.TweenColor(to, duration).SetTag(this);
        }

        public object Tween(Color from, Color to, float duration)
        {
            return this.TweenColor(from, to, duration).SetTag(this);
        }

        public object Tween(object from, object to, float duration)
        {
            if (to is Color) {
                return from is Color ?
                    Tween((Color)from, (Color)to, duration) :
                    Tween((Color)to, duration);
            }

            if (to is float) {
                return from is float ?
                    Tween((float)from, (float)to, duration) : 
                    Tween((float)to, duration);
            }

            return null;
        }
        #endregion

        #region 不规则热点
        private PolygonCollider2D m_Polygon;

        public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            var valid = base.IsRaycastLocationValid(screenPoint, eventCamera);
            if (valid && m_Polygon != null) {
                Vector3 spV3 = new Vector3(screenPoint.x, screenPoint.y, rectTransform.position.z - eventCamera.transform.position.z);
                Vector2 point = eventCamera.ScreenToWorldPoint(spV3);
                valid = m_Polygon.OverlapPoint(point);
            }
            return valid;
        }
        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();
            InitAtlasSprite();
        }

        protected override void Start()
        {
#if UNITY_EDITOR
            UGUITools.AutoUIRoot(this);
#endif
            base.Start();
            m_Polygon = GetComponent<PolygonCollider2D>();
        }

#if UNITY_EDITOR
        private void Update()
        {
            AutoLoadSprite();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            AutoLoadSprite();
        }
#endif

    }
}

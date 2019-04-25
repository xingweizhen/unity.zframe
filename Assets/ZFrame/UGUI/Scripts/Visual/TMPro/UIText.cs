using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZFrame.UGUI
{
    using Asset;
    using Tween;

#if USING_TMP
    [RequireComponent(typeof(TMPro.TextMeshProUGUI))]
    public class UIText : UIBehaviour, ITweenable, ITweenable<Color>, ITweenable<float>, ILabel
    {
        private string m_Lang;

        private void ApplyFont(string fontAssetPath)
        {
            string fontBundle, fontMat;
            AssetLoader.GetAssetpath(fontAssetPath, out fontBundle, out fontMat);

            m_Lang = UILabel.LOC.currentLang;
            fontBundle = fontBundle + '.' + m_Lang;

#if UNITY_EDITOR
            if (AssetsMgr.A == null) {
                var fontPath = string.Format("{0}/FONT", fontBundle);
                font = AssetLoader.EditorLoadAsset(typeof(TMPro.TMP_FontAsset), fontPath) as TMPro.TMP_FontAsset;
                if (!string.IsNullOrEmpty(fontMat)) {
                    var matPath = string.Format("{0}/{1}", fontBundle, fontMat);
                    var mat = AssetLoader.EditorLoadAsset(typeof(Material), matPath) as Material;
                    if (mat != null) {
                        m_Label.fontSharedMaterial = mat;
                    } else {
                        LogMgr.W("字体材质{0}:{1}不存在。[{2}/{3}]", fontBundle, fontMat, this.GetHierarchy(), name);
                    }
                }

                return;
            }
#endif

            AbstractAssetBundleRef fontAsset;
            if (AssetsMgr.A.Loader.TryGetAssetBundle(fontBundle, out fontAsset)) {
                font = fontAsset.Load("FONT", typeof(TMPro.TMP_FontAsset)) as TMPro.TMP_FontAsset;
                if (!string.IsNullOrEmpty(fontMat)) {
                    var mat = fontAsset.Load(fontMat, typeof(Material)) as Material;
                    if (mat != null) {
                        m_Label.fontSharedMaterial = mat;
                    } else {
                        LogMgr.W("字体材质{0}:{1}不存在。[{2}/{3}]", fontBundle, fontMat, this.GetHierarchy(), name);
                    }
                }

                AssetsMgr.AssignEditorShader(m_Label.fontSharedMaterial);
            } else {
                LogMgr.W("{0}未加载。[{1}/{2}]", fontBundle, this.GetHierarchy(), name);
            }
        }

        private TMPro.TextMeshProUGUI __Label;

        private TMPro.TextMeshProUGUI m_Label {
            get {
                if (__Label == null) {
                    __Label = GetComponent(typeof(TMPro.TextMeshProUGUI)) as TMPro.TextMeshProUGUI;
                }

                return __Label;
            }
        }

        public event TextChanged onTextChanged;

        [SerializeField] private string m_FontName;

        public string fontName {
            get { return m_FontName; }
            set {
                if (font == null || string.CompareOrdinal(value, m_FontName) != 0) {
                    m_FontName = value;
                    ApplyFont(value);
                }
            }
        }

        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("localize")]
        private bool m_Localized;

        public bool localized {
            get { return m_Localized; }
            set { m_Localized = value; }
        }

        [SerializeField, HideInInspector, UnityEngine.Serialization.FormerlySerializedAs("m_Text")]
        private string m_RawText;

        public RectTransform rectTransform {
            get { return m_Label.rectTransform; }
        }

        private TMPro.TMP_FontAsset m_Font;
        public TMPro.TMP_FontAsset font {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return m_Font ? m_Font : m_Label.font;
#endif
                return m_Font;
            }
            set { m_Label.font = m_Font = value; }
        }

        public float fontSize {
            get { return m_Label.fontSize; }
            set { m_Label.fontSize = value; }
        }

        public TMPro.FontStyles fontStyle {
            get { return m_Label.fontStyle; }
            set { m_Label.fontStyle = value; }
        }

        public string text {
            get { return m_Label.text; }

            set {
                if (string.CompareOrdinal(m_Lang, UILabel.LOC.currentLang) != 0) InitFont();

                m_Label.text = value;
                if (onTextChanged != null) {
                    onTextChanged.Invoke(value);
                }

                // 动态设置
                //UpdateLoc();
            }
        }

        public string rawText {
            get {
                return m_RawText;
            }
        }

        public Color color {
            get { return m_Label.color; }
            set { m_Label.color = value; }
        }

        public float alpha {
            get { return m_Label.alpha; }
            set { m_Label.alpha = value; }
        }

        public bool raycastTarget {
            get { return m_Label.raycastTarget; }
            set { m_Label.raycastTarget = value; }
        }

        public TMPro.TextAlignmentOptions alignment {
            get { return m_Label.alignment; }
            set { m_Label.alignment = value; }
        }

        public void InitFont()
        {
            if (string.IsNullOrEmpty(m_FontName)) {
                m_FontName = "fonts/mainfont/";
            }

            ApplyFont(m_FontName);
        }

        protected void InitText()
        {
            if (string.CompareOrdinal(m_Lang, UILabel.LOC.currentLang) != 0) {
                InitFont();
                UpdateLoc();
            }
        }

        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            if (Application.isPlaying)
                if (string.IsNullOrEmpty(m_RawText))
                    m_RawText = m_Label.text;
#endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InitText();
        }

        protected override void OnCanvasHierarchyChanged()
        {
            if (isActiveAndEnabled) {
                InitText();
            }

            base.OnCanvasHierarchyChanged();            
        }

        private void UpdateLoc()
        {
            if (!localized) return;

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif

            var getText = m_RawText;
            if (UILabel.LOC) {
                var txt = UILabel.LOC.Get(m_RawText);
                if (txt != null) {
                    getText = txt;
                } else {
                    if (Application.isPlaying) {
                        LogMgr.W("本地化获取失败：Lang = {0}, Key = {1} @ {2}",
                            UILabel.LOC.currentLang, m_RawText, transform.GetHierarchy(null));
                    }
                }
            }

            m_Label.text = getText;
        }
        
        public void SetVisible(bool visible)
        {
            m_Label.canvasRenderer.cull = !visible;
        }

        public LinkInfo FindLink(Vector3 screenPos, Camera camera)
        {
            var index = TMPro.TMP_TextUtilities.FindIntersectingLink(m_Label, screenPos, camera);
            if (index < 0) return LinkInfo.Empty;

            var info = m_Label.textInfo.linkInfo[index];
            return new LinkInfo() {
                linkId = info.GetLinkID(), linkText = info.GetLinkText(),
            };
        }
        
        #region Tweenable

        public ZTweener Tween(object from, object to, float duration)
        {
            if (to is Color) {
                return from is Color ? Tween((Color)from, (Color)to, duration) : Tween((Color)to, duration);
            }

            if (to is float) {
                return from is float ? Tween((float)from, (float)to, duration) : Tween((float)to, duration);
            }

            return null;
        }

        public ZTweener Tween(Color to, float duration)
        {
            return m_Label.TweenColor(to, duration).SetTag(this);
        }

        public ZTweener Tween(Color from, Color to, float duration)
        {
            return m_Label.TweenColor(from, to, duration).SetTag(this);
        }

        private void SetVisibleCharacters(float n)
        {
            m_Label.maxVisibleCharacters = (int)n;
        }

        private float GetVisibleCharacters()
        {
            return m_Label.maxVisibleCharacters;
        }

        public ZTweener Tween(float to, float duration)
        {
            if (to < 0) to = m_Label.GetParsedText().Length;
            return this.Tween(GetVisibleCharacters, SetVisibleCharacters, to, duration);
        }

        public ZTweener Tween(float from, float to, float duration)
        {
            SetVisibleCharacters(from);
            return Tween(to, duration);
        }

        #endregion


        public static implicit operator TMPro.TextMeshProUGUI(UIText ui)
        {
            return ui.m_Label;
        }

//#if UNITY_EDITOR
//        protected override void OnValidate()
//        {
//            base.OnValidate();
//            if (!Application.isPlaying) {
//                if (!string.IsNullOrEmpty(m_RawText))
//                    UpdateLoc();
//            }
//        }
//#endif
    }
#endif
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
//using NoToLua = XLua.BlackListAttribute;
using System.Text.RegularExpressions;

namespace ZFrame.UGUI
{
    using Asset;
    using Tween;
    
    public struct LinkInfo
    {
        public string linkId, linkText;

        public static readonly LinkInfo Empty = new LinkInfo();
    }

    class LinkData
    {
        public string linkId, linkText;
        public int index, len;
        public readonly List<Rect> boxes = new List<Rect>();
    }

    public class UILabel : Text, ILabel, ITweenable, ITweenable<float>, ITweenable<Color>, ITweenable<string>
    {
        public const string NBS = "\u00A0";
        
        private static readonly Dictionary<string, Font> LoadedFonts = new Dictionary<string, Font>();

        public enum LinkStyle { None, Normal, Underline };

        private static Font GetFont(string fontPath)
        {
            Font ret = null;
            if (LoadedFonts.TryGetValue(fontPath, out ret)) {
                if (ret != null) return ret;
                LoadedFonts.Remove(fontPath);
            }

            AbstractAssetBundleRef abRef;
            if (AssetLoader.Instance.TryGetAssetBundle(fontPath, out abRef)) {
                ret = abRef.Load(fontPath, typeof(Font)) as Font;
                if (ret != null) LoadedFonts.Add(fontPath, ret);
            }

            return ret;
        }
        
        private const string OMIT_STR = "...";
        private static readonly Regex LinkRegex = new Regex(@"<link=(.+?)>(.*?)</link>");
        private static StringBuilder LinkBuilder = new StringBuilder();
        protected static readonly UIVertex[] m_TempVerts = new UIVertex[4];
        
        private readonly List<LinkData> m_Links = new List<LinkData>();

        private string m_Lang;

        private static Localization _Localization;
        //[NoToLua]
        public static Localization LOC {
            get {
#if UNITY_EDITOR                
                var defLang = UGUITools.settings.defaultLang;
                if (!Application.isPlaying && (_Localization == null || _Localization.currentLang != defLang)) {
                    _Localization = (Localization)AssetLoader.EditorLoadAsset(typeof(Localization), 
                        UGUITools.settings.locAssetPath);
                    if (_Localization) {
                        _Localization.Reset();
                        _Localization.currentLang = defLang;
                    }
                }
#endif
                return _Localization;
            }
            set { _Localization = value; }
        }

        public event TextChanged onTextChanged;

        [SerializeField]
        [Tooltip("截掉溢出部分文本，并用...代替")]
        private bool m_Ellipsis;
        private string m_EllipsisText;
        public bool omited { get { return !string.IsNullOrEmpty(m_EllipsisText); } }

        [SerializeField]
        private string m_FontPath;

        [SerializeField, NamedProperty("HyperLink Style")]
        private LinkStyle m_LinkStyle;

        public LinkStyle linkStyle {
            get { return m_LinkStyle; }
            set {
                if (m_LinkStyle != value) {
                    m_LinkStyle = value;
                    SetVerticesDirty();
                }
            }
        }

        [SerializeField]
        private bool m_Localized;
        public bool localized { get { return m_Localized; } set { m_Localized = value; } }
        
        public override float preferredHeight {
            get {
                return string.IsNullOrEmpty(text) ? 0 : base.preferredHeight;
            }
        }

        /// <summary>
        /// 是否用Non-Breaking Space代替普通空格
        /// </summary>
        [SerializeField, NamedProperty("NBS")]
        [Tooltip("是否用Non-Breaking Space代替普通空格")]
        private bool m_NonBreakingSpace = false;
        
        private void UpdateEllipsis()
        {
            m_EllipsisText = null;
            if (m_Ellipsis) {
                var settings = GetGenerationSettings(Vector2.zero);
                var generator = cachedTextGeneratorForLayout;
                var width = generator.GetPreferredWidth(m_Text, settings) / pixelsPerUnit;
                var rectWidth = rectTransform.rect.width;
                if (width > rectWidth) {
                    var omitWidth = generator.GetPreferredWidth(OMIT_STR, settings) / pixelsPerUnit;
                    var clampWidth = rectWidth - omitWidth;

                    var length = m_Text.Length - 1;
                    for (int i = length; i > 0; --i) {
                        var omitText = m_Text.Substring(0, i);
                        var testWidth = generator.GetPreferredWidth(omitText, settings) / pixelsPerUnit;
                        if (testWidth <= clampWidth) {
                            m_EllipsisText = omitText + OMIT_STR;
                            return;
                        }
                    }
                    m_EllipsisText = OMIT_STR;
                    return;
                }

                settings = GetGenerationSettings(new Vector2(GetPixelAdjustedRect().size.x, 0.0f));
                var height = generator.GetPreferredHeight(m_Text, settings) / pixelsPerUnit;
                var rectHeight = rectTransform.rect.height;
                if (height > rectHeight) {
                    var omitText = m_Text;
                    for (; ; ) {
                        var idx = omitText.LastIndexOf('\n');
                        if (idx < 0) break;
                        omitText = omitText.Substring(0, idx);
                        var omitHeight = generator.GetPreferredHeight(omitText, settings) / pixelsPerUnit;
                        if (omitHeight <= rectHeight) {
                            break;
                        }
                    }
                    m_EllipsisText = omitText;
                }
            }
        }

        private void UpdateLoc()
        {
            var getText = m_RawText;
            if (localized) {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return;
                }
#endif
                if (LOC) {
                    if (string.IsNullOrEmpty(getText)) {
                        return;
                    } else {
                        var txt = LOC.Get(getText);
                        if (!string.IsNullOrEmpty(txt)) {
                            getText = txt;
                        } else {
                            if (Application.isPlaying) {
                                LogMgr.W("本地化获取失败：{0}[{1}]@({2})",
                                    LOC.currentLang, getText, rectTransform.GetHierarchy(null));
                            }
                        }
                    }
                }
            }

            base.text = getText;
        }

        public override string text {
            get { return !string.IsNullOrEmpty(m_EllipsisText) ? m_EllipsisText : m_Text; }
            set {
                if (m_Text != value) {
                    base.text = value;
                    UpdateEllipsis();
                    if (onTextChanged != null) {
                        onTextChanged.Invoke(m_RawText);
                    }
                }
            }
        }

        [SerializeField]
        private string m_RawText;

        public string rawText {
            get { return m_RawText; }
            set { m_RawText = value; }
        }

        public LinkInfo FindLink(Vector3 screenPos, Camera cam)
        {
            Vector2 point;
            if (m_LinkStyle > LinkStyle.None && RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, cam, out point)) {
                for (int i = 0; i < m_Links.Count; ++i) {
                    var link = m_Links[i];
                    var boxes = link.boxes;
                    for (var j = 0; j < boxes.Count; ++j) {
                        if (boxes[j].Contains(point)) {
                            return new LinkInfo() { linkId = link.linkId, linkText = link.linkText };
                        }
                    }
                }
            }

            return LinkInfo.Empty;
        }
        
        protected override void Start()
        {
#if UNITY_EDITOR
            UGUITools.AutoUIRoot(this);
#endif
            base.Start();
        }
        
        protected void InitText()
        {
            if (font == null && !string.IsNullOrEmpty(m_FontPath)) {
                font = GetFont(m_FontPath);
            }
            
            if (localized && LOC && m_Lang != LOC.currentLang) {
                m_Lang = LOC.currentLang;
                UpdateLoc();
                UpdateEllipsis();
            }
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

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            UpdateEllipsis();
        }

        protected string m_GenText;
        protected void GenLinkData()
        {
            m_Links.Clear();
            m_GenText = text;
            if (supportRichText && m_LinkStyle > LinkStyle.None) {
                LinkBuilder.Length = 0;
                var startIdx = 0;
                var matches = LinkRegex.Matches(text);
                if (matches.Count > 0) {
                    for (int i = 0; i < matches.Count; i++) {
                        var match = matches[i];
                        var linkId = match.Groups[1].Value;
                        var linkText = match.Groups[2].Value;
                        m_Links.Add(new LinkData {
                            linkId = linkId, linkText = linkText,
                            index = match.Index, len = match.Length,
                        });
                        LinkBuilder.Append(text.Substring(startIdx, match.Index - startIdx)).Append(linkText);
                        startIdx = match.Index + match.Length;
                    }

                    LinkBuilder.Append(text.Substring(startIdx, text.Length - startIdx));

                    var offset = 0;
                    for (var i = 0; i < m_Links.Count; ++i) {
                        var data = m_Links[i];
                        var len = data.linkText.Length;
                        data.index -= offset;
                        offset += data.len - len - 1;
                        data.len = len;
                        m_Links[i] = data;
                    }

                    m_GenText = LinkBuilder.ToString();
                }
            }
        }
        
        private void AddPartOfUnderLine(VertexHelper toFill, Rect rect, Color color, Vector2 uvtl, Vector2 uvtr, Vector2 uvbr, Vector2 uvbl)
        {
            var zOff = 0f;
            float posXMin = rect.xMin, posXMax = rect.xMax, posYMin = rect.yMin, posYMax = rect.yMax;

            m_TempVerts[0] = UIVertex.simpleVert;
            m_TempVerts[0].color = color;
            m_TempVerts[0].uv0 = uvtl;
            m_TempVerts[0].position = new Vector3(posXMin, posYMax, zOff);

            m_TempVerts[1] = m_TempVerts[0];
            m_TempVerts[1].uv0 = uvtr;
            m_TempVerts[1].position = new Vector3(posXMax, posYMax, zOff);

            m_TempVerts[2] = m_TempVerts[0];
            m_TempVerts[2].uv0 = uvbr;
            m_TempVerts[2].position = new Vector3(posXMax, posYMin, zOff);

            m_TempVerts[3] = m_TempVerts[0];
            m_TempVerts[3].uv0 = uvbl;
            m_TempVerts[3].position = new Vector3(posXMin, posYMin, zOff);

            toFill.AddUIVertexQuad(m_TempVerts);
        }

        protected void AddUnderLine(VertexHelper toFill, Rect rect, Color color, ref CharacterInfo u)
        {
            Vector2 uvTopLeft = u.uvTopLeft, uvTopRight = u.uvTopRight, uvBottomRight = u.uvBottomRight, uvBottomLeft = u.uvBottomLeft;
            var thickness = fontSize * 0.2f;

            var uvEdge = (uvTopRight.y - uvTopLeft.y) / 3f;
            uvTopLeft.y += uvEdge;
            uvTopRight.y -= uvEdge;
            AddPartOfUnderLine(toFill, new Rect(rect.position, new Vector2(rect.width, thickness)),
                color, uvTopLeft, uvTopRight, uvBottomRight, uvBottomLeft);
        }

        protected void GenLinkBounds(VertexHelper toFill)
        {
            if (m_Links.Count == 0) return;

            if (font.dynamic) {
                font.RequestCharactersInTexture("_", fontSize);
            }
            CharacterInfo charInf;
            font.GetCharacterInfo('_', out charInf, fontSize);

            var maxVertCount = toFill.currentVertCount;

            for (int i = 0; i < m_Links.Count; ++i) {
                var link = m_Links[i];
                int startIdx = link.index * 4;
                if (startIdx >= maxVertCount) return;

                int endIdx = (link.index + link.len) * 4;

                UIVertex vert = UIVertex.simpleVert;
                toFill.PopulateUIVertex(ref vert, startIdx);
                var color = vert.color;
                var bounds = new Bounds(vert.position, Vector3.zero);

                var lastX = vert.position.x;
                for (int j = startIdx + 1; j < Mathf.Min(maxVertCount, endIdx); ++j) {
                    toFill.PopulateUIVertex(ref vert, j);
                    var pos = vert.position;
                    if (pos.x < lastX - 1) {
                        // 新增包围盒
                        var boundsSize = bounds.size;
                        if (boundsSize.x > 0 && boundsSize.y > 0) {
                            link.boxes.Add(new Rect(bounds.min, bounds.size));
                        }
                        bounds = new Bounds(pos, Vector3.zero);
                    } else {
                        // 扩展包围盒
                        bounds.Encapsulate(pos);
                    }
                    if (j % 4 == 0) lastX = pos.x;
                }
                link.boxes.Add(new Rect(bounds.min, bounds.size));

                if (m_LinkStyle == LinkStyle.Underline) {
                    // 绘制下划线
                    for (var j = 0; j < link.boxes.Count; ++j) {
                        AddUnderLine(toFill, link.boxes[j], color, ref charInf);
                    }
                }
            }
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (font == null)
                return;

            var genText = m_GenText;
            if (m_NonBreakingSpace) {
                genText = genText.Replace(" ", NBS);
            }

            // We don't care if we the font Texture changes while we are doing our Update.
            // The end result of cachedTextGenerator will be valid for this instance.
            // Otherwise we can get issues like Case 619238.
            m_DisableFontTextureRebuiltCallback = true;

            Vector2 extents = rectTransform.rect.size;

            var settings = GetGenerationSettings(extents);
            cachedTextGenerator.PopulateWithErrors(genText, settings, gameObject);

            // Apply the offset to the vertices
            IList<UIVertex> verts = cachedTextGenerator.verts;
            toFill.Clear();
            if (verts.Count > 0) {
                float unitsPerPixel = 1 / pixelsPerUnit;
                //Last 4 verts are always a new line... (\n)
#if UNITY_2018_3_OR_NEWER
                int vertCount = verts.Count;
#else
                int vertCount = verts.Count - 4;
#endif

                Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
                roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;                
                if (roundingOffset != Vector2.zero) {
                    for (int i = 0; i < vertCount; ++i) {
                        int tempVertsIndex = i & 3;
                        m_TempVerts[tempVertsIndex] = verts[i];
                        m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                        m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                        m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                        if (tempVertsIndex == 3)
                            toFill.AddUIVertexQuad(m_TempVerts);
                    }
                } else {
                    for (int i = 0; i < vertCount; ++i) {
                        int tempVertsIndex = i & 3;
                        m_TempVerts[tempVertsIndex] = verts[i];
                        m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                        if (tempVertsIndex == 3)
                            toFill.AddUIVertexQuad(m_TempVerts);
                    }
                }
            }
            m_DisableFontTextureRebuiltCallback = false;

            GenLinkBounds(toFill);
        }

        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();

            GenLinkData();
        }

        public object Tween(float to, float duration)
        {
            return this.TweenAlpha(to, duration).SetTag(this);
        }

        public object Tween(float from, float to, float duration)
        {
            return this.TweenAlpha(from, to, duration).SetTag(this);
        }

        public object Tween(Color to, float duration)
        {
            return this.TweenColor(to, duration).SetTag(this);
        }

        public object Tween(Color from, Color to, float duration)
        {
            return this.TweenColor(from, to, duration).SetTag(this);
        }

        public object Tween(string to, float duration)
        {
            return this.TweenString(to, duration).SetTag(this);
        }

        public object Tween(string from, string to, float duration)
        {
            return this.TweenString(from, to, duration).SetTag(this);
        }

        public object Tween(object from, object to, float duration)
        {
            var s = to as string;
            if (s != null) {
                return from == null ? Tween(s, duration) : Tween((string)from, to, duration);                
            }

            if (to is Color) {
                return from is Color ? Tween((Color)from, (Color)to, duration) : Tween((Color)to, duration);
            }
            if (to is float) {
                return from is float ? Tween((float)from, (float)to, duration) : Tween((float)to, duration);
            }

            return null;
        }

        //#if UNITY_EDITOR
        //        protected override void OnValidate()
        //        {
        //            base.OnValidate();
        //
        //            if (!Application.isPlaying) {
        //                if (!string.IsNullOrEmpty(m_RawText)) {
        //                    UpdateLoc();
        //                    UpdateOmit();
        //                }
        //            }
        //        }
        //#endif
    }
}

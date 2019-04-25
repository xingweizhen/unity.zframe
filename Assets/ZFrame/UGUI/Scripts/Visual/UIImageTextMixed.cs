using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ZFrame.UGUI
{
    using Asset;

    public class UIImageTextMixed : UILabel
    {
        private static Dictionary<SpriteAtlas, Material> AtlasMats = new Dictionary<SpriteAtlas, Material>();
        private static readonly Regex ImgRegex = new Regex("<sprite=(.+?) />");
        private static readonly List<ImageInfo> ImgQue = new List<ImageInfo>();

        private Material GetMaterial(SpriteAtlas atlas)
        {
            Material mat = null;
            if (atlas != null && !AtlasMats.TryGetValue(atlas, out mat)) {
                mat = new Material(Shader.Find("UI/ImageText"));
                var sps = new Sprite[1];
                atlas.GetSprites(sps);
                if (AssetBundleLoader.I) {
                    mat.SetTexture("_AtlasTex", sps[0].texture);
                } else {
#if UNITY_EDITOR
                    var tex2d = UnityEditor.Sprites.SpriteUtility.GetSpriteTexture(sps[0], true);
                    mat.SetTexture("_AtlasTex", tex2d);
#endif
                }
            }

            return mat;
        }

        struct ImageInfo
        {
            public float width, height;
            public Vector4 uv;
            public int index;
            public int len;
        }
        
        [SerializeField, AssetRef(type: typeof(SpriteAtlas))]
        private string m_AtlasPath;

        private SpriteAtlas m_Atlas;
        public SpriteAtlas atlas {
            get {
                if (m_Atlas == null && !string.IsNullOrEmpty(m_AtlasPath)) {
                    if (AssetLoader.Instance == null) {
#if UNITY_EDITOR
                        m_Atlas = AssetLoader.EditorLoadAsset(null, m_AtlasPath) as SpriteAtlas;
#endif
                    } else {
                        string bundleName, assetName;
                        AssetLoader.GetAssetpath(m_AtlasPath, out bundleName, out assetName);
                        m_Atlas = UISprite.LoadAtlas(assetName, null);
                    }
                }
                return m_Atlas;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            material = GetMaterial(atlas);
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (font == null)
                return;

            var genText = m_GenText;
            if (supportRichText) {
                var matches = ImgRegex.Matches(genText);
                for (int i = 0; i < matches.Count; i++) {
                    var spriteName = matches[i].Groups[1].Value;
                    var sprite = atlas.GetSprite(spriteName);
                    ImageInfo info = new ImageInfo {
                        index = matches[i].Index,
                        len = matches[i].Length,
                    };

                    if (sprite) {
                        var rect = sprite.textureRect;
                        info.width = rect.width;
                        info.height = rect.height;
#if UNITY_EDITOR
                        if (AssetBundleLoader.I == null) {
                            var uvs = UnityEditor.Sprites.SpriteUtility.GetSpriteUVs(sprite, true);
                            float minx = 1, maxx = 0, miny = 1, maxy = 0;
                            foreach (var uv in uvs) {
                                if (uv.x < minx) minx = uv.x;
                                if (uv.x > maxx) maxx = uv.x;
                                if (uv.y < miny) miny = uv.y;
                                if (uv.y > maxy) maxy = uv.y;
                            }
                            info.uv = new Vector4(minx, miny, maxx, maxy);
                        } else
#endif
                        {
                            info.uv = new Vector4(
                                rect.min.x / sprite.texture.width,
                                rect.min.y / sprite.texture.height,
                                rect.max.x / sprite.texture.width,
                                rect.max.y / sprite.texture.height);
                        }
                    }

                    ImgQue.Add(info);
                }
                genText = ImgRegex.Replace(genText, "图");
                var offset = 0;
                for (var i = 0; i < ImgQue.Count; ++i) {
                    var info = ImgQue[i];
                    info.index -= offset;
                    offset += info.len - 1;
                    info.len = 1;
                    ImgQue[i] = info;
                }
            }

            // We don't care if we the font Texture changes while we are doing our Update.
            // The end result of cachedTextGenerator will be valid for this instance.
            // Otherwise we can get issues like Case 619238.
            m_DisableFontTextureRebuiltCallback = true;

            Vector2 extents = rectTransform.rect.size;

            var settings = GetGenerationSettings(extents);
            cachedTextGenerator.Populate(genText, settings);

            Rect inputRect = rectTransform.rect;

            // get the text alignment anchor point for the text in local space
            Vector2 textAnchorPivot = GetTextAnchorPivot(alignment);
            Vector2 refPoint = Vector2.zero;
            refPoint.x = Mathf.Lerp(inputRect.xMin, inputRect.xMax, textAnchorPivot.x);
            refPoint.y = Mathf.Lerp(inputRect.yMin, inputRect.yMax, textAnchorPivot.y);

            // Determine fraction of pixel to offset text mesh.
            Vector2 roundingOffset = PixelAdjustPoint(refPoint) - refPoint;

            // Apply the offset to the vertices
            IList<UIVertex> verts = cachedTextGenerator.verts;
            float unitsPerPixel = 1 / pixelsPerUnit;
            //Last 4 verts are always a new line...
            int vertCount = verts.Count - 4;

            toFill.Clear();
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
                for (int i = 0, n = 0; i < vertCount; ++i) {                    
                    int index = i / 4;
                    if (n < ImgQue.Count && ImgQue[n].index == index) {
                        var info = ImgQue[n++];
                        // 0   1
                        // 3   2
                        m_TempVerts[0] = verts[i];
                        m_TempVerts[1] = verts[i + 1];
                        m_TempVerts[2] = verts[i + 2];
                        m_TempVerts[3] = verts[i + 3];

                        if (info.width > 0 && info.height > 0) {
                            var vertSize = m_TempVerts[1].position - m_TempVerts[3].position;
                            var ratio = info.width / info.height;
                            if (ratio > 1) {
                                var offset = (vertSize.y - vertSize.y / ratio) / 2;
                                var pos = m_TempVerts[0].position;
                                pos.y -= offset;
                                m_TempVerts[0].position = pos * unitsPerPixel;

                                pos = m_TempVerts[1].position;
                                pos.y -= offset;
                                m_TempVerts[1].position = pos * unitsPerPixel;

                                pos = m_TempVerts[2].position;
                                pos.y += offset;
                                m_TempVerts[2].position = pos * unitsPerPixel;

                                pos = m_TempVerts[3].position;
                                pos.y += offset;
                                m_TempVerts[3].position = pos * unitsPerPixel;
                            } else if (ratio < 1) {
                                var offset = (vertSize.x - vertSize.x * ratio) / 2;
                                var pos = m_TempVerts[0].position;
                                pos.x += offset;
                                m_TempVerts[0].position = pos * unitsPerPixel;

                                pos = m_TempVerts[1].position;
                                pos.x -= offset;
                                m_TempVerts[1].position = pos * unitsPerPixel;

                                pos = m_TempVerts[2].position;
                                pos.x -= offset;
                                m_TempVerts[2].position = pos * unitsPerPixel;

                                pos = m_TempVerts[3].position;
                                pos.x += offset;
                                m_TempVerts[3].position = pos * unitsPerPixel;
                            }
                        }

                        var uv = info.uv;
                        m_TempVerts[0].uv1 = new Vector2(uv.x, uv.w);
                        m_TempVerts[1].uv1 = new Vector2(uv.z, uv.w);
                        m_TempVerts[2].uv1 = new Vector2(uv.z, uv.y);
                        m_TempVerts[3].uv1 = new Vector2(uv.x, uv.y);
                        toFill.AddUIVertexQuad(m_TempVerts);

                        i += 4 * info.len - 1;
                    } else {
                        int tempVertsIndex = i & 3;
                        m_TempVerts[tempVertsIndex] = verts[i];
                        m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                        if (tempVertsIndex == 3)
                            toFill.AddUIVertexQuad(m_TempVerts);
                    }
                }                
            }
            m_DisableFontTextureRebuiltCallback = false;
            ImgQue.Clear();
            GenLinkBounds(toFill);
        }
    }
}

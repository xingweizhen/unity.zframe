using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace ZFrame.UGUI
{
    public class UIMixedText : UILabel
    {
        private static readonly char[] SplitArr = new char[] { '=', ' ' };
        protected static readonly Regex ImgRegex = new Regex(@"<quad (.+?)/>");

        private readonly List<int> m_ImgIndex = new List<int>();
        private readonly List<Image> m_ImgPool = new List<Image>();

        private Text m_SubText;

        private void GenQuadImages()
        {
            m_ImgIndex.Clear();
            m_ImgPool.RemoveAll(img => img == null);
            if (m_ImgPool.Count == 0) {
                GetComponentsInChildren(m_ImgPool);
            }

            var matches = ImgRegex.Matches(m_Text);
            for (int i = 0; i < matches.Count; i++) {
                var match = matches[i];
                var paramStr = match.Groups[1].Value;
                var paramArr = paramStr.Split(SplitArr, System.StringSplitOptions.RemoveEmptyEntries);
                Sprite sprite = null;
                float spriteSize = fontSize;
                // povit
                float px = 0.5f, py = 0.5f;
                Color color = Color.white;
                for (var j = 0; j < paramArr.Length / 2; ++j) {
                    var key = paramArr[j * 2];
                    var value = paramArr[j * 2 + 1];
                    switch (key) {
                        case "name":
                            sprite = UISprite.LoadSprite(value, null);
                            break;
                        case "size":
                            float.TryParse(value, out spriteSize);
                            break;
                        case "px":
                            float.TryParse(value, out px);
                            break;
                        case "py":
                            float.TryParse(value, out py);
                            break;
                        case "color":
                            ColorUtility.TryParseHtmlString(value, out color);
                            break;
                    }
                }

                Image img = null;
                if (m_ImgIndex.Count == m_ImgPool.Count) {
                    var resources = new DefaultControls.Resources();
                    var go = DefaultControls.CreateImage(resources);
                    go.layer = gameObject.layer;
                    var rt = go.transform as RectTransform;
                    if (rt) {
                        rt.SetParent(rectTransform);
                        rt.localPosition = Vector3.zero;
                        rt.localRotation = Quaternion.identity;
                        rt.localScale = Vector3.one;
                        rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.anchorMax = rt.anchorMin;
                    }

                    img = go.GetComponent<Image>();
                    img.preserveAspect = true;
                    img.raycastTarget = false;
                    img.type = Image.Type.Simple;

                    m_ImgPool.Add(img);
                } else {
                    img = m_ImgPool[m_ImgIndex.Count];
                }

                img.sprite = sprite;
                img.color = color;
                img.rectTransform.sizeDelta = new Vector2(spriteSize, spriteSize);
                img.rectTransform.pivot = new Vector2(px, py);
                img.enabled = true;

                var picIndex = match.Index; // + match.Length -  1; 
                var endIndex = picIndex * 4 + 3;
                m_ImgIndex.Add(endIndex);
            }

            for (var i = m_ImgIndex.Count; i < m_ImgPool.Count; i++) {
                if (m_ImgPool[i]) {
                    m_ImgPool[i].enabled = false;
                }
            }
        }

        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
            GenQuadImages();
            //if (m_SubText == null) {
            //    var resources = new DefaultControls.Resources();
            //    var go = DefaultControls.CreateText(resources);
            //    go.layer = gameObject.layer;
            //    var rt = go.transform as RectTransform;
            //    if (rt) {
            //        rt.SetParent(rectTransform);
            //        rt.localPosition = Vector3.zero;
            //        rt.localRotation = Quaternion.identity;
            //        rt.localScale = Vector3.one;
            //        rt.anchorMin = Vector2.zero;
            //        rt.anchorMax = Vector2.one;
            //    }
            //    m_SubText = go.GetComponent(typeof(Text)) as Text;
            //}
            //if (m_SubText) {
            //    m_SubText.text = m_GenText;
            //    m_SubText.font = font;
            //    m_SubText.fontSize = fontSize;
            //    m_SubText.lineSpacing = lineSpacing;
            //    m_SubText.supportRichText = supportRichText;
            //    m_SubText.alignment = alignment;
            //    m_SubText.alignByGeometry = alignByGeometry;
            //    m_SubText.horizontalOverflow = horizontalOverflow;
            //    m_SubText.verticalOverflow = verticalOverflow;
            //    m_SubText.resizeTextForBestFit = resizeTextForBestFit;
            //    m_SubText.color = color;
            //    m_SubText.material = material;
            //    m_SubText.raycastTarget = false;
            //    m_SubText.transform.SetAsLastSibling();
            //}
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);

            m_DisableFontTextureRebuiltCallback = true;
            var maxVertCount = toFill.currentVertCount;
            var offset = Vector2.Scale(rectTransform.rect.size, rectTransform.pivot - new Vector2(0.5f, 0.5f));
            var offset3 = new Vector3(offset.x, offset.y, 0);
            for (var i = 0; i < m_ImgIndex.Count; ++i) {
                var endIndex = m_ImgIndex[i];
                if (endIndex < maxVertCount) {
                    var img = m_ImgPool[i];
                    var rt = img.rectTransform;
                    var size = rt.sizeDelta;

                    var vert = UIVertex.simpleVert;
                    toFill.PopulateUIVertex(ref vert, endIndex);
                    rt.anchoredPosition = vert.position + offset3;
                    rt.anchoredPosition += size / 2;

                    for (int j = endIndex - 3; j < endIndex; ++j) {
                        toFill.SetUIVertex(vert, j);
                    }
                }
            }
            m_DisableFontTextureRebuiltCallback = false;
        }
    }
}

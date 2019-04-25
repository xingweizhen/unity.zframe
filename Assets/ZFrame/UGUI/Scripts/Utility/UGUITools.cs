using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    public static class UGUITools
    {
        private static Settings _Settings;
        public static Settings settings {
            get {
                if (_Settings == null) {
                    _Settings =Resources.Load("UGUISettings", typeof(Settings)) as Settings; 
                }

                return _Settings;
            }
        }

#if UNITY_EDITOR
        static public GameObject SelectedRoot(bool createIfMissing)
        {
            GameObject go = Selection.activeGameObject;

            if (go && (!go.activeInHierarchy || !go.GetComponentInParent<Canvas>())) go = null;

            if (go == null) {
                var cv = GameObject.FindObjectOfType<Canvas>();
                if (cv) {
                    if (cv.transform.parent as RectTransform) {
                        cv = null;
                    }
                }
                go = cv ? cv.gameObject : null;
            }

            if (createIfMissing && go == null) {
                var uiLyaer = LayerMask.NameToLayer("UI");

                var root = new GameObject("UIROOT");
                root.transform.position = Vector3.zero;

                var goCam = new GameObject("UICamera", typeof(Camera));
                goCam.transform.SetParent(root.transform);
                goCam.transform.localPosition = Vector3.zero;
                var camera = goCam.GetComponent<Camera>();
                camera.depth = 1;
                camera.clearFlags = CameraClearFlags.Depth;
                camera.cullingMask = 1 << uiLyaer;
                camera.fieldOfView = 30;

                go = new GameObject("UICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                go.transform.SetParent(root.transform);
                go.transform.localPosition = Vector3.zero;
                go.AddComponent<Grid>().cellSize = new Vector3(10, 10, 10);

                var cv = go.GetComponent<Canvas>();
                cv.renderMode = RenderMode.ScreenSpaceCamera;
                cv.worldCamera = camera;

                var cvScl = go.GetComponent<CanvasScaler>();
                cvScl.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cvScl.referenceResolution = settings.defRes;
                cvScl.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                cvScl.matchWidthOrHeight = 0;

                var goEvt = new GameObject("EventSystems", typeof(EventSystem));
                goEvt.transform.SetParent(root.transform);
                goEvt.AddComponent(typeof(StandaloneInputModule));

                root.SetLayerRecursively(uiLyaer);
            }
            return go;
        }

        public static void AutoUIRoot(Graphic graphic)
        {
            if (!Application.isPlaying) {
                var canvas = graphic.GetComponentInParent<Canvas>();
                if (!canvas) {
                    var root = graphic.rectTransform.root;
                    root.SetParent(SelectedRoot(true).transform, false);
                    UnityEditor.Selection.activeTransform = root;
                }
            }
        }
#endif

        const string P_GRAYSCALE = "_Grayscale";
        const string UI_GRAYSCALE = "UI_GRAYSCALE";
        /// <summary>
        /// 灰度图共用材质
        /// </summary>
        private static Material s_GrayscaleMat;
        private static Material grayscaleMat {
            get {
                if (s_GrayscaleMat == null) {
                    s_GrayscaleMat = new Material(Shader.Find("UI/Grayscale"));
                }
                return s_GrayscaleMat;
            }
        }

        private class GrayscaleMatPair
        {
            public readonly Material baseMat, grayscaleMat;
            public GrayscaleMatPair(Material baseMat, Material grayscaleMat)
            {
                this.baseMat = baseMat; this.grayscaleMat = grayscaleMat;
            }
        }
        private static List<GrayscaleMatPair> s_GrayscaleMatPairs = new List<GrayscaleMatPair>();
        public static Material ToggleGrayscale(Material material, bool grayscale)
        {
            if (material && material.HasProperty(P_GRAYSCALE)) {
                GrayscaleMatPair matPair = null;
                foreach (var pair in s_GrayscaleMatPairs) {
                    if (pair.baseMat == material || pair.grayscaleMat == material) {
                        matPair = pair; break;
                    }
                }

                if (matPair == null) {
                    Material baseMat, grayMat;
                    if (material.IsKeywordEnabled(UI_GRAYSCALE)) {
                        grayMat = material;
                        baseMat = new Material(grayMat);
                        baseMat.DisableKeyword(UI_GRAYSCALE);
                    } else {
                        baseMat = material;
                        grayMat = new Material(baseMat);
                        grayMat.EnableKeyword(UI_GRAYSCALE);
                    }
                    matPair = new GrayscaleMatPair(baseMat, grayMat);
                    s_GrayscaleMatPairs.Add(matPair);
                }
                return grayscale ? matPair.grayscaleMat : matPair.baseMat;
            }

            return grayscale ? grayscaleMat : null;
        }
        
        public static bool IsGrayscale(Material material)
        {
            if (material && material.HasProperty(P_GRAYSCALE)) {
                return material.IsKeywordEnabled(UI_GRAYSCALE);
            } else {
                return material == grayscaleMat;
            }
        }

        /// <summary>
        /// 图片模糊公用材质
        /// </summary>
        private static Material s_BlurMat;
        public static Material blurMat {
            get {
                if (s_BlurMat == null) {
                    s_BlurMat = new Material(Shader.Find("UI/Blur"));
                }
                return s_BlurMat;
            }
        }
        
        public static void SetGrayscale(GameObject go, bool grayscale)
        {
            if (go) {
                var list = ListPool<Component>.Get();
                go.GetComponentsInChildren(typeof(UISprite), list);
                foreach (UISprite sp in list) {
                    sp.grayscale = grayscale;
                }

                ListPool<Component>.Release(list);
            }
        }

        /// <summary>
        /// 限制在屏幕内位置
        /// </summary>
        public static Vector2 InsideScreenPosition(this RectTransform self, RectTransform screenRect = null)
        {
            var canvas = self.GetComponentInParent<Canvas>();
            while (!canvas.isRootCanvas) {
                canvas = canvas.transform.parent.GetComponentInParent<Canvas>();
            }
            var parent = self.parent as RectTransform;
            if (canvas && parent) {
                var canvasRect = screenRect != null ? screenRect : canvas.GetComponent<RectTransform>();
                var scrSiz = canvasRect.rect.size / 2;// RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect);
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, self);
                var v2Anchored = self.anchoredPosition;

                var v2Max = bounds.max;
                var v2Min = bounds.min;

                if (v2Max.x > scrSiz.x) {
                    v2Anchored.x -= (v2Max.x - scrSiz.x);
                }
                if (v2Max.y > scrSiz.y) {
                    v2Anchored.y -= (v2Max.y - scrSiz.y);
                }
                if (v2Min.x < -scrSiz.x) {
                    v2Anchored.x -= (scrSiz.x + v2Min.x);
                }
                if (v2Min.y < -scrSiz.y) {
                    v2Anchored.y -= (scrSiz.y + v2Min.y);
                }

                return v2Anchored;
            }
            return self.anchoredPosition;
        }

        /// <summary>
        /// 计算锚点对齐位置
        /// </summary>
        public static Vector2 AnchorPosition(this Canvas canvas, RectTransform from, Vector2 pivotFrom, RectTransform to, Vector2 pivotTo, Vector2 offset)
        {
            var camera = canvas.worldCamera;

            // 源的锚点必须为中心
            var v2Center = new Vector2(0.5f, 0.5f);
            from.anchorMax = v2Center;
            from.anchorMin = v2Center;
            
            Vector2 v2To = from.anchoredPosition;
            var rect = from.parent as RectTransform;
            var v2Screen = RectTransformUtility.WorldToScreenPoint(camera, to.position);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, v2Screen, camera, out v2To)) {
                var pivotOffset = new Vector2(to.pivot.x - pivotTo.x, to.pivot.y - pivotTo.y);
                var toSize = to.rect.size;
                v2To.x -= toSize.x * pivotOffset.x;
                v2To.y -= toSize.y * pivotOffset.y;
                v2To.x += offset.x;
                v2To.y += offset.y;

                pivotOffset = new Vector2(from.pivot.x - pivotFrom.x, from.pivot.y - pivotFrom.y);

                var fromSize = from.rect.size;
                v2To.x += fromSize.x * pivotOffset.x;
                v2To.y += fromSize.y * pivotOffset.y;
            }

            return v2To;
        }

        public static CanvasGroup FindCanvasGroup(this UIBehaviour bvr)
        {
            var cv = bvr.GetComponentInParent<CanvasGroup>();
            while (cv != null) {
                if (cv.ignoreParentGroups) break;

                var parent = cv.transform.parent;
                var top = parent ? parent.GetComponentInParent<CanvasGroup>() : null;
                if (top) {
                    cv = top;
                } else break;
            }

            return cv;
        }

        public static PointerEventData GenEventData(EventSystem eventSystem)
        {
            var data = new PointerEventData(eventSystem);
            var module = eventSystem.currentInputModule;
            data.position = module.input.mousePosition;

            return data;
        }

        public static void PlayUISfx(string sfxName, string defSfx)
        {
            AudioManager.Audio.PlayUISfx(sfxName, defSfx);
        }
    }
}

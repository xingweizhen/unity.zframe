using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif
using ILuaState = System.IntPtr;

namespace ZFrame.Lua
{
    using UGUI;
    using Tween;
    using Asset;
    
    public static class LibUGUI
    {
        public const string LIB_NAME = "libugui.cs";

        public enum StretchAxis
        {
            None,
            Horizontal,
            Vertical,
            HorizontalAndVertical,
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int OpenLib(ILuaState lua)
        {
            lua.NewTable();
            lua.SetDict("FindGraphic", FindGraphic);
            lua.SetDict("FindEventHandler", FindEventHandler);

            lua.SetDict("CreateWindow", CreateWindow);
            lua.SetDict("CloseWindow", CloseWindow);
            lua.SetDict("SetWindowDepth", SetWindowDepth);
            lua.SetDict("Select", Select);
            lua.SetDict("Deselect", Deselect);

            // Localization
            lua.SetDict("GetSupportLanguages", GetSupportLanguages);
            lua.SetDict("SetLocalize", SetLocalize);
            lua.SetDict("GetLoc", GetLoc);

            // RectTransform
            lua.SetDict("SetAnchoredPos", SetAnchoredPos);
            lua.SetDict("GetAnchoredPos", GetAnchoredPos);
            lua.SetDict("SetSizeDelta", SetSizeDelta);
            lua.SetDict("SetAnchor", SetAnchor);
            lua.SetDict("SetPivot", SetPivot);
            lua.SetDict("SetAnchoredSize", SetAnchoredSize);
            lua.SetDict("SetAspectRadio", SetAspectRadio);
            lua.SetDict("GetRectSize", GetRectSize);
            lua.SetDict("AnchorPresets", AnchorPresets);
            lua.SetDict("AnchorStretch", AnchorStretch);

            // Graphic & Visual
            lua.SetDict("SetCanvasSorting", SetCanvasSorting);
            lua.SetDict("SetVisible", SetVisible);
            lua.SetDict("SetAlpha", SetAlpha);
            lua.SetDict("SetColor", SetColor);
            lua.SetDict("SetGradient", SetGradient);
            lua.SetDict("SetText", SetText);
            lua.SetDict("ValidText", ValidText);
            lua.SetDict("SetSprite", SetSprite);
            lua.SetDict("SetTexture", SetTexture);
            lua.SetDict("SetInteractable", SetInteractable);
            lua.SetDict("SetBlocksRaycasts", SetBlocksRaycasts);
            lua.SetDict("SetGrayscale", SetGrayscale);
            lua.SetDict("SetFlip", SetFlip);
            lua.SetDict("SetShadow", SetShadow);
            lua.SetDict("SetOutline", SetOutline);
            lua.SetDict("IsVisible", IsVisible);

            // Layout
            lua.SetDict("SetMinSize", SetMinSize);
            lua.SetDict("SetPreferredSize", SetPreferredSize);
            lua.SetDict("SetFlexibleSize", SetFlexibleSize);
            lua.SetDict("SetAspectSize", SetAspectSize);
            lua.SetDict("SetNativeSize", SetNativeSize);
            lua.SetDict("DOSiblingByFar", DOSiblingByFar);
            lua.SetDict("DOAnchor", DOAnchor);
            lua.SetDict("SetLoopCap", SetLoopCap);
            lua.SetDict("SetScrollValue", SetScrollValue);
            lua.SetDict("RebuildLayout", RebuildLayout);

            // Event
            lua.SetDict("ExecuteEvent", ExecuteEvent);
            lua.SetDict("SetEventData", SetEventData);

            // Util
            lua.SetDict("GetTogglesOn", GetTogglesOn);
            lua.SetDict("AllTogglesOff", AllTogglesOff);
            lua.SetDict("IsScreenPointInRect", IsScreenPointInRect);
            lua.SetDict("InsideScreen", InsideScreen);
            lua.SetDict("ScreenPoint2Local", ScreenPoint2Local);
            lua.SetDict("World2ScreenPoint", World2ScreenPoint);

            // Tween
            lua.SetDict("DOFade", DOFade);
            lua.SetDict("DOTween", DOTween);
            lua.SetDict("MakeSequence", MakeSequence);
            lua.SetDict("DOShake", DOShake);
            lua.SetDict("KillTween", KillTween);
            lua.SetDict("StopTween", StopTween);
            lua.SetDict("CompleteTween", CompleteTween);
            lua.SetDict("WaitForTween", WaitForTween);

            lua.SetDict("Overlay", Overlay);
            lua.SetDict("Follow", Follow);
            lua.SetDict("GetPkgName", GetPkgName);

            // Layout Controll
            lua.SetDict("SetArray", SetArray);

            lua.SetDict("InitGroup", InitGroup);
            lua.SetDict("SetGroupIdx", SetGroupIdx);
            lua.SetDict("GetGroupIdx", GetGroupIdx);

            lua.SetDict("SetStateSprite", SetStateSprite);

            // api
            lua.SetDict("CopyToClipboard", CopyToClipboard);

            //lua.SetDict("Indicate", Indicate);

            // Draw Line
            //lua.SetDict("DrawLine", DrawLine);
            //lua.SetDict("DrawLine3D", DrawLine3D);
            //lua.SetDict("DestroyLine", DestroyLine);

            return 1;
        }

        #region 界面结构 & 其他

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int FindGraphic(ILuaState lua)
        {
            var parent = lua.ToComponent(1, typeof(Transform)) as Transform;
            var childName = lua.OptString(2, null);
            if (parent) {
                var child = string.IsNullOrEmpty(childName) ? parent : parent.FindByName(childName);
                if (child) {
                    lua.PushLightUserData(child.GetComponent(typeof(Graphic)));
                    return 1;
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int FindEventHandler(ILuaState lua)
        {
            var parent = lua.ToComponent(1, typeof(Transform)) as Transform;
            var childName = lua.OptString(2, null);
            if (parent) {
                var child = string.IsNullOrEmpty(childName) ? parent : parent.FindByName(childName);
                if (child) {
                    lua.PushLightUserData(child.GetComponent(typeof(IEventSystemHandler)));
                    return 1;
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int CreateWindow(ILuaState lua)
        {
            var uimgr = UIManager.Instance;

            int index = lua.OptInteger(2, 0);
            int canvasId = lua.OptInteger(3, 0);
            var luaT = lua.Type(1);
            if (luaT == LuaTypes.LUA_TSTRING) {
                string prefab = lua.ChkString(1);
                GameObject go = uimgr.CreateWindow(prefab, index, canvasId);
                lua.PushLightUserData(go);
                return 1;
            } else if (luaT == LuaTypes.LUA_TUSERDATA || luaT == LuaTypes.LUA_TLIGHTUSERDATA) {
                var prefab = lua.ToGameObject(1);
                if (prefab) {
                    GameObject go = uimgr.CreateWindow(prefab, index, canvasId);
                    lua.PushLightUserData(go);
                    return 1;
                }
            }

            lua.L_ArgError(1, string.Format("string or GameObject excepted, got {0}", luaT));
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int CloseWindow(ILuaState lua)
        {
            var luaT = lua.Type(1);
            if (luaT == LuaTypes.LUA_TSTRING) {
                string prefab = lua.ChkString(1);
                UIWindow go = UIWindow.FindByName(prefab);
                if (go) {
                    ObjectPoolManager.DestroyPooled(go.gameObject, 0f);
                }
            } else if (luaT == LuaTypes.LUA_TUSERDATA || luaT == LuaTypes.LUA_TLIGHTUSERDATA) {
                var prefab = lua.ToGameObject(1);
                if (prefab) {
                    ObjectPoolManager.DestroyPooled(prefab, 0f);
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetWindowDepth(ILuaState lua)
        {
            var lc = UIWindow.FindByName(lua.ToString(1));
            if (lc != null) {
                UIManager.UpdateSiblingIndex(lc, lua.ToInteger(2));
            }

            return 0;
        }

        private static IEnumerator SelectGameObject(GameObject go, bool click)
        {
            yield return null;
            if (go.activeInHierarchy && !EventSystem.current.alreadySelecting) {
                EventSystem.current.SetSelectedGameObject(go);

                if (click) {
                    var pinterClick = go.GetComponent(typeof(IPointerClickHandler)) as IPointerClickHandler;
                    if (pinterClick != null) {
                        pinterClick.OnPointerClick(new PointerEventData(EventSystem.current));
                    }
                }
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int Select(ILuaState lua)
        {
            var toSelect = lua.ToGameObject(1);
            if (toSelect) {
                var click = lua.OptBoolean(2, false);
                UIManager.Instance.StartCoroutine(SelectGameObject(toSelect, click));
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int Deselect(ILuaState lua)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetSupportLanguages(ILuaState lua)
        {
            var loc = UILabel.LOC;
            if (loc) {
                lua.CreateTable(loc.langs.Length - 1, 0);
                for (var i = 1; i < loc.langs.Length; ++i) {
                    lua.PushInteger(i);
                    lua.CreateTable(0, 2);
                    lua.SetDict("lang", loc.langs[i]);
                    lua.SetDict("name", loc.Get("@LANG", i));
                    lua.SetTable(-3);
                }

                return 1;
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetLocalize(ILuaState lua)
        {
            var forced = lua.OptBoolean(3, false);
            if (UILabel.LOC == null || forced) {
                UILabel.LOC = AssetLoader.Instance.Load(typeof(ScriptableObject), UGUITools.settings.locAssetPath) as Localization;
            }

            UILabel.LOC.defLang = lua.ToString(2);
            if (UILabel.LOC != null) {
                UILabel.LOC.currentLang = lua.ToString(1);

                lua.PushString(UILabel.LOC.currentLang);
                return 1;
            }

            return 0;

        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetLoc(ILuaState lua)
        {
            var key = lua.ToString(1);
            var lang = lua.OptString(2, UILabel.LOC.currentLang);
            lua.PushString(UILabel.LOC.Get(key, lang));
            return 1;
        }

        #endregion


        #region <RectTransform>属性

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetAnchoredPos(ILuaState lua)
        {
            var rectTransform = lua.ToComponent<RectTransform>(1);
            if (rectTransform) {
                var klass = lua.Class(2);
                if (klass == UnityEngine_Vector3.CLASS) {
                    rectTransform.anchoredPosition3D = lua.ToVector3(2);
                } else if (klass == UnityEngine_Vector2.CLASS) {
                    rectTransform.anchoredPosition = lua.ToVector2(2);
                } else {
                    var v3 = rectTransform.anchoredPosition3D;
                    v3.x = lua.OptSingle(2, v3.x);
                    v3.y = lua.OptSingle(3, v3.y);
                    v3.z = lua.OptSingle(4, v3.z);
                    rectTransform.anchoredPosition3D = v3;
                }
            } else LogMgr.W("SetPos Fail: target is NULL");

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetAnchoredPos(ILuaState lua)
        {
            var rect = lua.ToComponent<RectTransform>(1);
            if (rect) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                lua.PushX(rect.anchoredPosition3D);
                return 1;
            } else return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetSizeDelta(ILuaState lua)
        {
            var rectTransform = lua.ToComponent<RectTransform>(1);
            if (rectTransform) {
                var sizeDelta = rectTransform.sizeDelta;
                sizeDelta.x = lua.OptSingle(2, sizeDelta.x);
                sizeDelta.y = lua.OptSingle(3, sizeDelta.y);
                rectTransform.sizeDelta = sizeDelta;
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetAnchor(ILuaState lua)
        {
            var rect = lua.ToComponent<RectTransform>(1);
            var index = 2;

            var luaT = lua.Type(index);
            if (luaT == LuaTypes.LUA_TTABLE) {
                rect.anchorMin = lua.ToVector2(index++);
            } else if (luaT == LuaTypes.LUA_TNUMBER) {
                var anchorMin = rect.anchorMin;
                anchorMin.x = lua.ToSingle(index++);
                anchorMin.y = lua.ToSingle(index++);
                rect.anchorMin = anchorMin;
            } else {
                index += 1;
            }

            luaT = lua.Type(index);
            if (luaT == LuaTypes.LUA_TTABLE) {
                rect.anchorMax = lua.ToVector2(index++);
            } else if (luaT == LuaTypes.LUA_TNUMBER) {
                var anchorMax = rect.anchorMax;
                anchorMax.x = lua.ToSingle(index++);
                anchorMax.y = lua.ToSingle(index++);
                rect.anchorMax = anchorMax;
            } else {
                index += 1;
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetPivot(ILuaState lua)
        {
            var rect = lua.ToComponent<RectTransform>(1);
            var count = lua.GetTop();
            if (count == 2) {
                rect.pivot = lua.ToVector2(2);
            } else {
                rect.pivot = new Vector2(lua.ToSingle(2), lua.ToSingle(3));
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetAnchoredSize(ILuaState lua)
        {
            var rect = lua.ToComponent<RectTransform>(1);
            if (lua.Type(2) == LuaTypes.LUA_TNUMBER) {
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, lua.ToSingle(2));
            }

            if (lua.Type(3) == LuaTypes.LUA_TNUMBER) {
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, lua.ToSingle(3));
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetAspectRadio(ILuaState lua)
        {
            var rectTransform = lua.ToComponent<RectTransform>(1);
            var aspectMode = (AspectRatioFitter.AspectMode)lua.ToEnumValue(2, typeof(AspectRatioFitter.AspectMode));

            var aspectFitter = rectTransform.GetComponent<AspectRatioFitter>();
            if (aspectFitter) {
                var aspectRadio = lua.OptSingle(3, aspectFitter.aspectRatio);
                aspectFitter.aspectRatio = aspectRadio;
                aspectFitter.aspectMode = aspectMode;
            } else {

                var aspectRadio = lua.ToSingle(3);
                if (aspectMode == AspectRatioFitter.AspectMode.WidthControlsHeight) {
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rectTransform.rect.size.x / aspectRadio);
                } else if (aspectMode == AspectRatioFitter.AspectMode.HeightControlsWidth) {
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.size.y * aspectRadio);
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetRectSize(ILuaState lua)
        {
            var rect = lua.ToComponent<RectTransform>(1);
            bool bImmediate = lua.OptBoolean(2, false);
            if (rect) {
                if (bImmediate) {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                }

                lua.PushX(rect.rect.size);
                return 1;
            } else return 0;
        }

        /// <summary>
        /// 同时设置锚点(anchorMin anchorMax)和轴心(pivot)
        /// </summary>
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int AnchorPresets(ILuaState lua)
        {
            var rectTransform = lua.ToComponent<RectTransform>(1);
            if (rectTransform) {
                var x = lua.ToSingle(2);
                var y = lua.ToSingle(3);
                var posX = lua.OptSingle(4, 0);
                var posY = lua.OptSingle(5, 0);
                var anchor = new Vector2(x, y);
                rectTransform.anchorMin = anchor;
                rectTransform.anchorMax = anchor;
                rectTransform.pivot = anchor;
                rectTransform.anchoredPosition = new Vector2(posX, posY);
            }

            return 0;
        }

        /// <summary>
        /// 设置扩展的锚点(anchorMin anchorMax)和轴心(pivot)
        /// </summary>
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int AnchorStretch(ILuaState lua)
        {
            var rectTransform = lua.ToComponent<RectTransform>(1);
            if (rectTransform) {
                var axis = (StretchAxis)lua.ToEnumValue(2, typeof(StretchAxis));
                var pos = lua.OptSingle(3, 0.5f);
                var anchoredPos = Vector2.zero;
                var deltaX = lua.OptSingle(4, 0);
                var deltaY = lua.OptSingle(5, 0);
                var offset = lua.OptSingle(6, 0);
                switch (axis) {
                    case StretchAxis.Horizontal:
                        rectTransform.anchorMin = new Vector2(0, pos);
                        rectTransform.anchorMax = new Vector2(1, pos);
                        anchoredPos.y = offset;
                        break;
                    case StretchAxis.Vertical:
                        rectTransform.anchorMin = new Vector2(pos, 0);
                        rectTransform.anchorMax = new Vector2(pos, 1);
                        anchoredPos.x = offset;
                        break;
                    case StretchAxis.HorizontalAndVertical:
                        rectTransform.anchorMin = new Vector2(0, 0);
                        rectTransform.anchorMax = new Vector2(1, 1);
                        break;
                    case StretchAxis.None:
                        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                        break;
                    default: return 0;
                }

                rectTransform.pivot = (rectTransform.anchorMin + rectTransform.anchorMax) / 2;
                rectTransform.sizeDelta = new Vector2(deltaX, deltaY);
                rectTransform.anchoredPosition = anchoredPos;
            }

            return 0;
        }

        #endregion

        #region 图元属性

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetCanvasSorting(ILuaState lua)
        {
            var canvas = lua.ToComponent(1, typeof(Canvas)) as Canvas;
            if (canvas) {
                if (lua.IsNoneOrNil(2)) {
                    // 重置UIWindow的Canvas排序
                    var wnd = canvas.GetComponent(typeof(UIWindow)) as UIWindow;
                    if (wnd) wnd.ResetCanvasSorting();
                } else {
                    if (lua.Type(3) == LuaTypes.LUA_TSTRING) {
                        canvas.sortingLayerName = lua.ToString(3);
                    } else {
                        canvas.sortingLayerID = lua.OptInteger(3, 0);
                    }

                    canvas.sortingOrder = lua.OptInteger(4, 0);
                    canvas.overrideSorting = lua.ToBoolean(2);
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetVisible(ILuaState lua)
        {
            bool dirty = false;
            var visible = lua.ToBoolean(2);

            var canvas = lua.ToComponent(1, typeof(Canvas)) as Canvas;
            if (canvas && canvas.enabled != visible) {
                dirty = true;
                canvas.enabled = visible;
                goto RETURN;
            }

            var graphic = lua.ToComponent(1, typeof(Graphic)) as Graphic;
            if (graphic && graphic.canvasRenderer.cull == visible) {
                dirty = true;
                graphic.canvasRenderer.cull = !visible;
                if (visible) graphic.SetVerticesDirty();
            }

            RETURN:
            lua.PushBoolean(dirty);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetAlpha(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go) {
                var alpha = lua.ToSingle(2);

                var cvGrp = go.GetComponent<CanvasGroup>();
                if (cvGrp) {
                    cvGrp.alpha = alpha;
                    return 0;
                }

                var graphic = go.GetComponent<Graphic>();
                if (graphic) {
                    var c = graphic.color;
                    c.a = alpha;
                    graphic.color = c;
                    return 0;
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetColor(ILuaState lua)
        {
            var all = lua.OptBoolean(3, false);
            if (all) {
                var go = lua.ToGameObject(1);
                if (go) {
                    var color = lua.ToColor(2);
                    var graphics = ListPool<Component>.Get();
                    go.GetComponentsInChildren(typeof(Graphic), graphics);
                    foreach (var graphic in graphics) {
                        ((Graphic)graphic).color = color;
                    }

                    ListPool<Component>.Release(graphics);
                }
            } else {
                var graphic = lua.ToComponent<Graphic>(1);
                if (graphic) graphic.color = lua.ToColor(2);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetGradient(ILuaState lua)
        {
            var gradient = lua.ToComponent<ZFrame.UGUI.Gradient>(1);
            if (gradient) {
                var c1 = lua.ToColor(2);
                var c2 = lua.ToColor(3);
                gradient.SetColor(c1, c2);
            }

            return 0;
        }

        /// <summary>
        /// function(Component|GameObjct, string@text, [string@path])
        /// </summary>
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetText(ILuaState lua)
        {
            var text = lua.ToComponent(1, typeof(ILabel)) as ILabel;
            if (text != null) text.text = lua.ToLuaString(2);

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int ValidText(ILuaState lua)
        {
            //var label = lua.ToComponent<Text>(1);
//        var text = lua.ChkString(2);
//        var strbld = new System.Text.StringBuilder();
//        for (int i = 0; i < text.Length; ++i) {
//            var c = text[i];
//            if (c >= '\u0000' && c <= '\uFFFF') {
//                strbld.Append(c);
//            } else {
//                strbld.Append('*');
//            }
//        }
//        lua.PushString(strbld.ToString());
//        return 1;
            return 0;
        }

        private static readonly DelegateObjectLoaded OnSpriteAtlasLoaded = (a, o, p) => {
            var img = p as Image;
            if (img) {
                var sp = p as UISprite;
                if (sp) {
                    sp.atlas = o as SpriteAtlas;
                } else { }
            }
        };

        /// <summary>
        /// function(Component|GameObjct, string|Sprite, [string@path])
        /// </summary>
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetSprite(ILuaState lua)
        {
            var img = lua.ToComponent<Image>(1);
            var obj = lua.ToAnyObject(2);

            var path = obj as string;
            if (path != null) {
                var sp = img as UISprite;
                if (sp) {
                    sp.SetSprite(path, false);
                    if (!sp.overrideSprite) {
                        AssetLoader.Instance.LoadAsync(typeof(SpriteAtlas),
                            string.Format("Atlas/{0}/{0}", sp.atlasName),
                            LoadMethod.Default, OnSpriteAtlasLoaded, sp);
                    }
                } else if (img) {
                    img.overrideSprite = UISprite.LoadSprite(path, null);
                    if (!img.overrideSprite) {
                        // TODO...
                    }
                }
            } else {
                if (img) img.overrideSprite = obj as Sprite;
            }

            return 0;
        }

        /// <summary>
        /// function(Component|GameObjct, string|Sprite, [string@path], [function])
        /// </summary>
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetTexture(ILuaState lua)
        {
            var img = lua.ToComponent<RawImage>(1);
            var obj = lua.ToAnyObject(2);

            var path = obj as string;
            if (path != null) {
                var tex = img as UITexture;
                if (tex) {
                    tex.SetTexture(path);
                } else {
                    img.texture = UITexture.LoadTexture(path, true);
                }
            } else {
                if (img) img.texture = obj as Texture;
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetInteractable(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go == null) return 0;

            var interactable = lua.ToBoolean(2);

            var list = ListPool<Component>.Get();
            go.GetComponents(typeof(UISelectable), list);
            foreach (UISelectable sel in list) {
                sel.interactable = interactable;
            }

            ListPool<Component>.Release(list);

            var cvGrp = go.GetComponent(typeof(CanvasGroup)) as CanvasGroup;
            if (cvGrp) {
                cvGrp.interactable = interactable;
                return 0;
            }

            var select = go.GetComponent(typeof(Selectable)) as Selectable;
            if (select) {
                select.interactable = interactable;
                return 0;
            }

            return 0;
        }
        
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetBlocksRaycasts(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go == null) return 0;

            var blocks = lua.ToBoolean(2);

            var cvGrp = go.GetComponent(typeof(CanvasGroup)) as CanvasGroup;
            if (cvGrp) {
                cvGrp.blocksRaycasts = blocks;
                return 0;
            }

            var gh = go.GetComponent(typeof(Graphic)) as Graphic;
            if (gh) {
                gh.raycastTarget = blocks;
                return 0;
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetGrayscale(ILuaState lua)
        {
            UGUITools.SetGrayscale(lua.ToGameObject(1), lua.ToBoolean(2));
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetFlip(ILuaState lua)
        {
            var img = lua.ToComponent<Image>(1);
            if (img) {
                var hori = lua.ToBoolean(2);
                var vert = lua.ToBoolean(3);
                var flip = img.gameObject.NeedComponent<Flippable>();
                flip.horizontal = hori;
                flip.vertical = vert;
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetShadow(ILuaState lua)
        {
            var text = lua.ToComponent<Text>(1);
            if (text) {
                var x = lua.ToSingle(2);
                var y = lua.ToSingle(3);
                var c = lua.ToColor(4);
                var shadow = text.gameObject.NeedComponent<Shadow>();
                shadow.effectColor = c;
                shadow.effectDistance = new Vector2(x, y);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetOutline(ILuaState lua)
        {
            var text = lua.ToComponent<Text>(1);
            if (text) {
                var x = lua.ToSingle(2);
                var y = lua.ToSingle(3);
                var c = lua.ToColor(4);
                var outline = text.gameObject.NeedComponent<Outline>();
                outline.effectColor = c;
                outline.effectDistance = new Vector2(x, y);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int IsVisible(ILuaState lua)
        {
            var trans = lua.ToComponent(1, typeof(Transform)) as Transform;
            if (trans != null) {
                var canvas = trans.GetComponentInParent(typeof(Canvas)) as Canvas;
                lua.PushBoolean(canvas && canvas.isActiveAndEnabled);
                return 1;
            }

            return 0;
        }

        #endregion

        #region 界面布局

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetMinSize(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go) {
                var layout = go.NeedComponent<LayoutElement>();
                layout.minWidth = lua.OptSingle(2, layout.minWidth);
                layout.minHeight = lua.OptSingle(3, layout.minHeight);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetPreferredSize(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go) {
                var layout = go.NeedComponent<LayoutElement>();
                layout.preferredWidth = lua.OptSingle(2, layout.preferredWidth);
                layout.preferredHeight = lua.OptSingle(3, layout.preferredHeight);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetFlexibleSize(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go) {
                var layout = go.NeedComponent<LayoutElement>();
                layout.flexibleWidth = lua.OptSingle(2, layout.flexibleWidth);
                layout.flexibleHeight = lua.OptSingle(3, layout.flexibleHeight);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetAspectSize(ILuaState lua)
        {
            var layout = lua.ToComponent<SpriteSizeFitter>(1);
            if (layout) {
                var aspectMode = (AspectRatioFitter.AspectMode)lua.ToEnumValue(2, typeof(AspectRatioFitter.AspectMode));
                var value = lua.ToSingle(3);
                if (aspectMode == AspectRatioFitter.AspectMode.WidthControlsHeight) {
                    layout.width = value;
                } else if (aspectMode == AspectRatioFitter.AspectMode.HeightControlsWidth) {
                    layout.height = value;
                }

                return 0;
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetNativeSize(ILuaState lua)
        {
            var img = lua.ToComponent<Image>(1);
            if (img) img.SetNativeSize();
            return 0;
        }

        /// <summary>
        /// 根据距离对<Transform>进行兄弟排列
        /// </summary>
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int DOSiblingByFar(ILuaState lua)
        {
            var parent = lua.ToComponent<Transform>(1);
            var center = lua.ToVector3(2);
            if (parent) {
                var origin = center;
                var list = new List<Transform>();
                for (int i = 0; i < parent.childCount; ++i) {
                    list.Add(parent.GetChild(i));
                }

                list.Sort((Transform a, Transform b) => {
                    var x = Vector3.Distance(origin, a.position);
                    var y = Vector3.Distance(origin, b.position);
                    var ret = (int)((x - y) * 10000);
                    return -ret;
                });
                SiblingByFar.DOSibling(list, 0);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int DOAnchor(ILuaState lua)
        {
            var from = lua.ToComponent<RectTransform>(1);
            var pivotF = lua.ToVector2(2);
            var to = lua.ToComponent<RectTransform>(3);
            var pivotT = lua.ToVector2(4);
            var offset = lua.ToVector2(5);
            var set = lua.OptBoolean(6, true);
            if (from && from.gameObject.activeInHierarchy && to && to.gameObject.activeInHierarchy) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(from);
                Vector2 v2Ret = from.anchoredPosition;
                var autoAnchor = from.GetComponent(typeof(UIAutoAnchor)) as UIAutoAnchor;
                if (autoAnchor) {
                    autoAnchor.SetAnchor(pivotF, to, pivotT, offset);
                } else {
                    var cv = from.GetComponentInParent<Canvas>();
                    v2Ret = cv.AnchorPosition(from, pivotF, to, pivotT, offset);
                    if (set) {
                        from.anchoredPosition = v2Ret;
                    }
                }

                lua.PushX(v2Ret);
                return 1;
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetLoopCap(ILuaState lua)
        {
            var loop = lua.ToComponent(1, typeof(ILoopLayout)) as ILoopLayout;
            if (loop != null) {
                if (lua.OptBoolean(3, false)) loop.ResetLayout();
                loop.SetTotalItem(lua.ToInteger(2), lua.OptBoolean(3, false));
                LayoutRebuilder.ForceRebuildLayoutImmediate(loop.transform as RectTransform);
                lua.PushBoolean(true);
            } else {
                lua.PushBoolean(false);
            }

            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetScrollValue(ILuaState lua)
        {
            var scroll = lua.ToComponent(1, typeof(ScrollRect)) as ScrollRect;
            if (scroll) {
                if (scroll.horizontal) {
                    scroll.horizontalNormalizedPosition = lua.ToSingle(2);
                } else if (scroll.vertical) {
                    scroll.verticalNormalizedPosition = 1 - lua.ToSingle(2);
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int RebuildLayout(ILuaState lua)
        {
            var rect = lua.ToComponent(1, typeof(RectTransform)) as RectTransform;
            if (rect) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }

            Canvas.ForceUpdateCanvases();
            return 0;
        }

        #endregion


        #region Event

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int ExecuteEvent(ILuaState lua)
        {
            var target = lua.ToGameObject(1);
            var select = target.GetComponent(typeof(Selectable)) as Selectable;
            if (select != null && !select.IsInteractable()) return 0;

            var type = (TriggerType)lua.ToEnumValue(2, typeof(TriggerType));
            var eventData = lua.ToUserData(3) as BaseEventData ?? UGUITools.GenEventData(EventSystem.current);

            switch (type) {
                case TriggerType.PointerClick: {
                    var ret = ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerClickHandler);
                    lua.PushBoolean(ret);
                    return 1;
                }
                case TriggerType.PointerDown: {
                    var ret = ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerDownHandler);
                    lua.PushBoolean(ret);
                    return 1;
                }
                case TriggerType.PointerUp: {
                    var ret = ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerUpHandler);
                    lua.PushBoolean(ret);
                    return 1;
                }
                case TriggerType.BeginDrag: {
                    var ret = ExecuteEvents.Execute(target, eventData, ExecuteEvents.beginDragHandler);
                    lua.PushBoolean(ret);
                    return 1;
                }
                case TriggerType.Drag: {
                    var ret = ExecuteEvents.Execute(target, eventData, ExecuteEvents.dragHandler);
                    lua.PushBoolean(ret);
                    return 1;
                }
                case TriggerType.EndDrag: {
                    var ret = ExecuteEvents.Execute(target, eventData, ExecuteEvents.endDragHandler);
                    lua.PushBoolean(ret);
                    return 1;
                }
                default:
                    lua.PushBoolean(true);
                    return 1;
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetEventData(ILuaState lua)
        {
            var sender = lua.ToComponent(1, typeof(IEventSender)) as IEventSender;
            if (sender != null) {
                var type = (TriggerType)lua.ToEnumValue(2, typeof(TriggerType));
                var name = (UIEvent)lua.ToEnumValue(3, typeof(UIEvent));
                var param = lua.ToString(4);
                sender.SetEvent(type, name, param);
                if (type == TriggerType.Longpress)
                    ((Component)sender).gameObject.NeedComponent(typeof(UILongpress));
            }

            return 0;
        }

        #endregion

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetTogglesOn(ILuaState lua)
        {
            var tglGrp = lua.ToComponent<ToggleGroup>(1);
            if (tglGrp == null) return 0;

            using (var itor = tglGrp.ActiveToggles().GetEnumerator()) {
                lua.NewTable();
                int i = 0;
                while (itor.MoveNext()) {
                    lua.PushInteger(++i);
                    lua.PushLightUserData(itor.Current);
                    lua.SetTable(-3);
                }
            }

            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int AllTogglesOff(ILuaState lua)
        {
            var tglGrp = lua.ToComponent<ToggleGroup>(1);
            if (tglGrp) {
                var allowSwitchOff = tglGrp.allowSwitchOff;
                tglGrp.allowSwitchOff = true;
                foreach (var tgl in tglGrp.ActiveToggles()) {
                    var transition = tgl.toggleTransition;
                    tgl.toggleTransition = Toggle.ToggleTransition.None;
                    tgl.isOn = false;
                    tgl.toggleTransition = transition;
                }

                tglGrp.allowSwitchOff = allowSwitchOff;
                //tglGrp.SetAllTogglesOff();
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int IsScreenPointInRect(ILuaState lua)
        {
            var screenPoint = lua.ToVector2(1);
            var rect = lua.ToComponent<RectTransform>(2);
            var camera = rect.GetComponentInParent<Canvas>().worldCamera;
            var ret = RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, camera);
            lua.PushBoolean(ret);
            return 1;
        }

        /// <summary>
        /// 限制一个UI对象在屏幕内
        /// </summary>
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int InsideScreen(ILuaState lua)
        {
            var rect = lua.ToComponent(1, typeof(RectTransform)) as RectTransform;
            if (rect) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                var screenRect = lua.ToComponent(2, typeof(RectTransform)) as RectTransform;

                if (screenRect == null) {
                    var canvas = rect.GetComponentInParent(typeof(Canvas)) as Canvas;
                    while (canvas && !canvas.isRootCanvas) {
                        canvas = canvas.transform.parent.GetComponentInParent(typeof(Canvas)) as Canvas;
                    }

                    if (canvas) screenRect = canvas.GetComponent(typeof(RectTransform)) as RectTransform;
                }

                if (screenRect) {
                    //InsideScreenPosition(screenRect);
                    Vector2 pos;
                    var inside = rect.CheckInside(screenRect, out pos);
                    if (!inside) {
                        var set = lua.OptBoolean(3, true);
                        if (set) rect.anchoredPosition = pos;
                    } else pos = rect.anchoredPosition;

                    lua.PushX(pos);
                    lua.PushBoolean(inside);
                    return 2;
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int ScreenPoint2Local(ILuaState lua)
        {
            var screenPoint = lua.ToVector2(1);
            var rect = lua.ToComponent<RectTransform>(2);
            Camera camera = null;
            for (Transform t = rect; t != null;) {
                var canvas = t.GetComponentInParent<Canvas>();
                if (canvas && canvas.worldCamera) {
                    camera = canvas.worldCamera;
                    break;
                }

                t = canvas.transform.parent;
            }

            Vector2 ret;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, camera, out ret);
            lua.PushX(ret);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int World2ScreenPoint(ILuaState lua)
        {
            Camera camera = null;
            Vector3 worldPos;
            if (lua.IsClass(1, UnityEngine_Vector3.CLASS)) {
                worldPos = lua.ToVector3(1);
                camera = lua.ToComponent(2, typeof(Camera)) as Camera;
            } else {
                var rect = lua.ToComponent(1, typeof(Transform)) as Transform;
                worldPos = rect.position;
                for (Transform t = rect; t != null;) {
                    var canvas = t.GetComponentInParent<Canvas>();
                    if (canvas && canvas.worldCamera) {
                        camera = canvas.worldCamera;
                        break;
                    }

                    t = canvas.transform.parent;
                }
            }

            lua.PushX(RectTransformUtility.WorldToScreenPoint(camera, worldPos));
            return 1;
        }

        #region 缓动接口

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int DOFade(ILuaState lua)
        {
            GameObject go = lua.ToGameObject(1);
            var group = lua.ToEnumValue(2, typeof(FadeGroup));
            var func = lua.ToLuaFunction(3);
            bool reset = lua.OptBoolean(4, false);
            bool foward = lua.OptBoolean(5, true);
            if (go && go.activeInHierarchy) {
                ZTween.Stop(go);
                var tw = group != null ? FadeTool.DOFade(go, (FadeGroup)group, reset, foward) : FadeTool.DOFade(go, reset, foward);
                if (func != null) {
                    if (tw != null) {
                        tw.CompleteWith(_ => {
                            func.Action(go);
                            func.Dispose();
                        });
                    } else {
                        UIManager.Instance.StartCoroutine(LibUnity.LuaInvoke(UIManager.Instance, func, 0, go));
                    }
                }
            }

            return 0;
        }

        private static ITweenable ToTweenable(object tweenObject, object tweenType)
        {
            if (tweenType == null) return tweenObject as ITweenable;

            var com = tweenObject as Component;
            GameObject go = com != null ? com.gameObject : tweenObject as GameObject;
            if (go && go.activeInHierarchy) {
                var tweenName = tweenType as string;
                if (tweenName != null) {
                    var tweenable = go.GetComponent(tweenName) as ITweenable;
                    if (tweenable == null) {
                        switch (tweenName) {
                            case "Position":
                                tweenable = go.NeedComponent(typeof(TweenPosition)) as ITweenable;
                                break;
                            case "PositionW":
                                tweenable = go.NeedComponent(typeof(TweenPositionW)) as ITweenable;
                                break;
                            case "Rotation":
                                tweenable = go.NeedComponent(typeof(TweenRotation)) as ITweenable;
                                break;
                            case "EulerAngles":
                                tweenable = go.NeedComponent(typeof(TweenEulerAngles)) as ITweenable;
                                break;
                            case "Scale":
                                tweenable = go.NeedComponent(typeof(TweenScaling)) as ITweenable;
                                break;
                            case "Transform":
                                tweenable = go.NeedComponent(typeof(TweenTransform)) as ITweenable;
                                break;
                            case "Alpha":
                                tweenable = go.NeedComponent(typeof(TweenAlpha)) as ITweenable;
                                break;
                            case "Size":
                                tweenable = go.NeedComponent(typeof(TweenSize)) as ITweenable;
                                break;
                            default: break;
                        }
                    }

                    return tweenable;
                }

                var tweenerType = tweenType as System.Type;
                if (tweenerType != null) {
                    return go.NeedComponent(tweenerType) as ITweenable;
                }
            }

            return null;
        }

        private static ZTweener ToTweener<T>(this ILuaState lua, object tweenObj,
            I2V.Index2Value<T> indexTo, int fromIdx, int toIdx, float duration)
        {
            var twaTmpl = tweenObj as ITweenable<T>;
            if (twaTmpl != null) {
                T fTo;
                indexTo.Invoke(lua, toIdx, out fTo);
                if (lua.IsNoneOrNil(fromIdx)) {
                    return twaTmpl.Tween(fTo, duration);
                }

                T fFrom;
                indexTo.Invoke(lua, fromIdx, out fFrom);
                return twaTmpl.Tween(fFrom, fTo, duration);
            }

            return null;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int DOTween(ILuaState lua)
        {

            object tweenType = lua.ToAnyObject(1);
            object tweenObj = lua.ToUnityObject(2);

            var tweenable = ToTweenable(tweenObj, tweenType);
            if (tweenable == null) return 0;

            ZTweener tw = null;
            float duration = lua.GetNumber(5, "duration", 0f);

            // First try no-boxing process
            LuaTypes lt = lua.Type(4);
            switch (lt) {
                case LuaTypes.LUA_TNUMBER:
                    tw = lua.ToTweener(tweenObj, I2V.ToSingle, 3, 4, duration);
                    break;
                case LuaTypes.LUA_TSTRING:
                    tw = lua.ToTweener(tweenObj, I2V.Tostring, 3, 4, duration);
                    break;
                case LuaTypes.LUA_TTABLE:
                    switch (lua.Class(4)) {
                        case UnityEngine_Bounds.CLASS:
                            tw = lua.ToTweener(tweenObj, I2V.ToBounds, 3, 4, duration);
                            break;
                        case UnityEngine_Color.CLASS:
                            tw = lua.ToTweener(tweenObj, I2V.ToColor, 3, 4, duration);
                            break;
                        case UnityEngine_Quaternion.CLASS:
                            tw = lua.ToTweener(tweenObj, I2V.ToQuaternion, 3, 4, duration);
                            break;
                        case UnityEngine_Vector2.CLASS:
                            tw = lua.ToTweener(tweenObj, I2V.ToVector2, 3, 4, duration);
                            break;
                        case UnityEngine_Vector3.CLASS:
                            tw = lua.ToTweener(tweenObj, I2V.ToVector3, 3, 4, duration);
                            break;
                        case UnityEngine_Vector4.CLASS:
                            tw = lua.ToTweener(tweenObj, I2V.ToVector4, 3, 4, duration);
                            break;
                    }

                    break;
            }

            // Fallback to boxing process... 
            if (tw == null) {
                object to = lua.ToAnyObject(4);
                object from = lua.ToAnyObject(3);
                tw = tweenable.Tween(from, to, duration);
            }

            if (tw != null) {
                Ease ease = (Ease)lua.GetEnum(5, "ease", Ease.Linear);
                float delay = lua.GetNumber(5, "delay", 0f);
                int loops = (int)lua.GetNumber(5, "loops", 0);
                LoopType loopType = (LoopType)lua.GetEnum(5, "loopType", LoopType.Restart);
                UpdateType updateType = (UpdateType)lua.GetEnum(5, "updateType", UpdateType.Normal);
                bool ignoreTimescale = lua.GetBoolean(5, "ignoreTimescale", true);
                float timeScale = lua.GetNumber(5, "timeScale", 1f);

                LuaFunction onStart = lua.GetFunction(5, "start");
                LuaFunction onUpdate = lua.GetFunction(5, "update");
                LuaFunction onComplete = lua.GetFunction(5, "complete");

                tw.timeScale = timeScale;
                tw.EaseBy(ease).DelayFor(delay)
                    .LoopFor(loops, loopType)
                    .SetUpdate(updateType, ignoreTimescale);

                if (onStart != null) {
                    tw.StartWith((_tw) => onStart.Action(tw.target));
                } else tw.StartWith(null);

                if (onUpdate != null) {
                    tw.UpdateWith((_tw) => onUpdate.Action(_tw.elapsed, tw.target));
                } else tw.UpdateWith(null);

                if (onComplete != null) {
                    tw.CompleteWith((_tw) => onComplete.Action(_tw.target));
                } else tw.CompleteWith(null);

                lua.PushLightUserData(tw);
                return 1;
            }

            LogMgr.W("{0} Free tween fail: {1} of {2}", lua.DebugCurrentLine(2), tweenType, tweenObj);
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int MakeSequence(ILuaState lua)
        {
            var args = lua.ToParamsObject<ZTweener>(2, lua.GetTop() - 1);
            var loops = lua.GetInteger(1, "loops");
            var loopType = (LoopType)lua.GetEnum(1, "loopType", LoopType.Restart);
            var updateType = (UpdateType)lua.GetEnum(1, "updateType", UpdateType.Normal);
            var ignoreTimescale = lua.GetBoolean(1, "ignoreTimescale", true);

            var seq = ZTween.MakeSequence(args);
            seq.LoopFor(loops, loopType)
                .SetUpdate(updateType, ignoreTimescale);

            lua.PushLightUserData(seq);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int DOShake(ILuaState lua)
        {
            var cam = lua.ToComponent<Camera>(1);
            var duration = lua.ToSingle(2);
            var strength = (float)lua.OptNumber(3, 3f);
            var vibrato = lua.OptInteger(4, 10);
            lua.PushLightUserData(cam.ShakePosition(duration, strength, vibrato));
            return 1;
        }

        /// <summary>
        /// 停止缓动
        /// </summary>
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int KillTween(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go) {
                var tweens = ListPool<Component>.Get();
                go.GetComponents(typeof(ITweenable), tweens);
                foreach (var tw in tweens) ZTween.Stop(tw);
                ListPool<Component>.Release(tweens);
            }

            return 0;
        }

        /// <summary>
        /// 停止指定tag或者id的Tween
        /// </summary>
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int StopTween(ILuaState lua)
        {
            var obj = lua.ToAnyObject(1);
            var complete = lua.OptBoolean(2, false);
            ZTween.Stop(obj, complete);
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int CompleteTween(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go) {
                var tweens = ListPool<Component>.Get();
                go.GetComponents(typeof(ITweenable), tweens);
                foreach (var tw in tweens) ZTween.Stop(tw, true);
                ListPool<Component>.Release(tweens);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int WaitForTween(ILuaState lua)
        {
            var obj = lua.ToUserData(1);
            var tweener = obj as ZTweener;
            if (tweener != null) {
                lua.PushLightUserData(tweener.WaitForCompletion());
            } else {
                LogMgr.D("WaitForTween: {0} is NOT a tweener!", obj);
                lua.PushNil();
            }

            return 1;
        }

        #endregion

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int Overlay(ILuaState lua)
        {
            Transform trans = lua.ToComponent<Transform>(1);
            if (trans) {
                var depthOfView = lua.OptSingle(3, trans.position.z);
                if (lua.IsClass(2, UnityEngine_Vector3.CLASS)) {
                    var tarV3 = lua.ToVector3(2);
                    var tarCam = lua.ToUnityObject(3) as Camera;
                    if (tarCam == null) tarCam = Camera.main;
                    if (tarCam == null) {
                        lua.PushBoolean(false);
                        return 1;
                    }

                    trans.Overlay(tarV3, tarCam, depthOfView);
                } else {
                    Transform tarTrans = lua.ToComponent<Transform>(2);
                    Camera tarCam = tarTrans.gameObject.FindCameraForLayer();
                    if (tarCam == null) {
                        lua.PushBoolean(false);
                        return 1;
                    }

                    trans.Overlay(tarTrans.position, tarCam, depthOfView);
                }
            }

            lua.PushBoolean(true);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int Follow(ILuaState lua)
        {
            var trans = lua.ToComponent(1, typeof(Transform)) as Transform;
            if (trans) {
                if (lua.IsClass(2, UnityEngine_Vector3.CLASS)) {
                    var cam = lua.ToComponent(3, typeof(Camera)) as Camera;
                    if (cam == null) cam = Camera.main;
                    var follow = UIFollowTarget.Follow(trans.gameObject, lua.ToVector3(2), cam);
                    lua.PushLightUserData(follow);
                    return 1;
                } else {
                    Transform target = lua.ToComponent<Transform>(2);
                    var rectTrans = trans as RectTransform;
                    var rectTarget = target as RectTransform;
                    if (rectTarget) {
                        // 场景对象跟随UI
                        var follow = trans.gameObject.NeedComponent<FollowUITarget>();
                        var smoothTime = (float)lua.OptNumber(4, 0f);
                        if (smoothTime > 0f) follow.smoothTime = smoothTime;
                        var alwaysSmooth = lua.OptBoolean(5, false);
                        follow.alwaysSmooth = alwaysSmooth;
                        follow.followTarget = rectTarget;
                        follow.depthOfView = lua.ToSingle(3);
                        lua.PushLightUserData(follow);
                        return 1;
                    } else if (rectTrans) {
                        var cam = lua.ToComponent(3, typeof(Camera)) as Camera;
                        // UI跟随场景对象
                        var follow = UIFollowTarget.Follow(trans.gameObject, target, cam);
                        lua.PushLightUserData(follow);
                        return 1;
                    }
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetPkgName(ILuaState lua)
        {
            var com = lua.ToComponent<LuaComponent>(1);
            string pkgName = com ? com.GetPackName() : null;
            if (pkgName != null) {
                lua.PushString(pkgName);
            } else {
                lua.PushNil();
            }

            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetArray(ILuaState lua)
        {
            var root = lua.ToComponent<Transform>(1);
            if (root) {
                root.gameObject.SetActive(true);
                var childCount = root.childCount;
                var n = lua.ToInteger(2);
                var max = lua.OptInteger(3, childCount);
                for (int i = 0; i < childCount; ++i) {
                    var elm = root.GetChild(i);
                    elm.gameObject.SetActive(i < max);
                    var sp = elm.GetComponent<Graphic>();
                    if (sp) {
                        sp.material = UGUITools.ToggleGrayscale(sp.material, i >= n);
                    } else {
                        elm.gameObject.SetActive(i < n);
                    }
                }
            } else {
                LogMgr.W("SetArray: 不存在的根节点");
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int InitGroup(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go) {
                lua.PushX(go.NeedComponent(typeof(UIGroup)));
                return 1;
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetGroupIdx(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go) {
                var index = lua.ToInteger(2);
                var grp = go.GetComponentInParent(typeof(UIGroup)) as UIGroup;
                if (grp && grp.SetIndex(go, index)) return 0;

                ((UIEntry)go.NeedComponent(typeof(UIEntry))).index = index;
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetGroupIdx(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go) {
                var grp = go.GetComponentInParent(typeof(UIGroup)) as UIGroup;
                if (grp != null) {
                    var parent = grp.transform;
                    var trans = go.transform;
                    while (trans && trans.parent != parent) {
                        trans = trans.parent;
                    }

                    var index = grp.GetIndex(trans);
                    if (index >= 0) {
                        lua.PushInteger(index);
                        return 1;
                    }
                }

                var ent = go.GetComponentInParent(typeof(UIEntry)) as UIEntry;
                if (ent != null && ent.index >= 0) {
                    lua.PushInteger(ent.index);
                    return 1;
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetStateSprite(ILuaState lua)
        {
            var sprTrans = lua.ToComponent(1, typeof(SpriteTransition)) as SpriteTransition;
            if (sprTrans != null) {
                var index = (SelectingState)lua.ToEnumValue(2, typeof(SelectingState));
                var spritepath = lua.ToString(3);
                sprTrans.SetStateSprite(index, spritepath);
            }

            return 0;
        }


        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int CopyToClipboard(ILuaState lua)
        {
            string copyText = lua.ToString(1);
            UniClipboard.SetText(copyText);
            //GUIUtility.systemCopyBuffer = copyText;
            return 0;
        }

        /*
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int Indicate(ILuaState lua)
        {
            var trans = lua.ToComponent(1, typeof(RectTransform)) as RectTransform;
            if (trans) {
                var target = lua.ToComponent(2, typeof(Transform)) as Transform;
                var indicator = trans.GetComponent(typeof(CurvedWorldUIIndicator)) as CurvedWorldUIIndicator;            
                if (target) {
                    if (indicator == null) {
                        indicator = trans.gameObject.AddComponent(typeof(CurvedWorldUIIndicator)) as CurvedWorldUIIndicator;
                    }
                    var cam = lua.ToComponent(3, typeof(Camera)) as Camera;
                    indicator.SetTarget(target, cam);
                    lua.PushLightUserData(indicator);
                    return 1;
                } else {
                    if(indicator) indicator.enabled = false;
                }
            }
            return 0;
        }
        */

        /*
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int DrawLine(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go) {
                var line2D = go.NeedComponent<VectorObject2D>();
                var line = line2D.vectorLine;
                if (line == null) {
                    line = new VectorLine(go.name, new List<Vector2>(), 1);
                    line2D.SetVectorLine(line, null, null);
                }
                line.points2.Clear();
                lua.PushNil();
                while (lua.Next(2)) {
                    var point = lua.ToVector2(-1);
                    line.points2.Add(point);
                    lua.Pop(1);
                }
            }
    
            return 0;
        }
    
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int DrawLine3D(ILuaState lua)
        {
            var name = lua.ChkString(1);
            var list = new List<Vector3>();
    
            lua.PushValue(2);
            lua.PushNil();
            while (lua.Next(-2)) {
                list.Add(lua.ToVector3(-1));
                lua.Pop(1);
            }
            lua.Pop(1);
    
            var duration = lua.OptSingle(3, -1f);
    
            VectorLine line;
            if (lua.IsTable(4)) {
                var nPoint = lua.GetValue(I2V.ToInteger, 4, "nPoint", 0);
                var points = nPoint == 0 ? list : new List<Vector3>(nPoint);
                line = new VectorLine(name, points, lua.GetNumber(4, "width")) {
                    lineType = (LineType)lua.GetEnum(4, "lineType", LineType.Continuous),
                    joins = (Joins)lua.GetEnum(4, "joins", Joins.None),
                    color = lua.GetValue(I2V.ToColor, 4, "color", Color.white),
                    textureScale = lua.GetNumber(4, "textureScale", 0f),
                };
                var gocam = lua.GetValue(I2V.ToGameObject, 4, "camera");
                if (gocam) VectorLine.SetCamera3D(gocam);
    
                var matLib = lua.GetString(4, "matLib");
                if (!string.IsNullOrEmpty(matLib)) {
                    var matName = lua.GetString(4, "matName");
                    if (!string.IsNullOrEmpty(matName)) {
                        var mat = ObjectLibrary.Load(matLib, matName) as Material;
                        line.material = mat;
                        line.texture = mat.mainTexture;
                    }
                }
                var texLib = lua.GetString(4, "texLib");
                if (!string.IsNullOrEmpty(texLib)) {
                    var texName = lua.GetString(4, "texName");
                    if (!string.IsNullOrEmpty(texName)) {
                        line.texture = ObjectLibrary.Load(matLib, texName) as Texture;
                    }
                }
    
                var make = lua.GetString(4, "make", "spline");
                switch (make) {
                    case "rect": {
                            if (list.Count == 2) {
                                var index = (int)lua.GetNumber(4, "index", 0);
                                line.MakeRect(list[0], list[1], index);
                            } else {
                                LogMgr.E("MakeRect needs exactly 2 points in the curve points array");
                            }
                        }
                        break;
                    case "rounded": {
                            if (list.Count == 2) {
                                var cornerRadius = lua.GetNumber(4, "cornerRadius", 1);
                                var cornerSegments = (int)lua.GetNumber(4, "cornerSegments", 1);
                                var index = (int)lua.GetNumber(4, "index", 0);
                                line.MakeRoundedRect(list[0], list[1], cornerRadius, cornerSegments, index);
                            } else {
                                LogMgr.E("MakeRoundedRect needs exactly 2 points in the curve points array");
                            }
                        }
                        break;
                    case "ellipse": {
                            var origin = lua.GetValue(I2V.ToVector3, 4, "origin", Vector3.zero);
                            var upVector = lua.GetValue(I2V.ToVector3, 4, "upVector", Vector3.up);
                            var xRadius = lua.GetNumber(4, "xRadius", 1);
                            var yRadius = lua.GetNumber(4, "yRadius", xRadius);
                            var startDegrees = lua.GetNumber(4, "startDegrees", 0);
                            var endDegrees = lua.GetNumber(4, "endDegrees", 360);
                            var segments = (int)lua.GetNumber(4, "segments", line.GetSegmentNumber());
                            var pointRotation = lua.GetNumber(4, "pointRotation", 0);
                            var index = (int)lua.GetNumber(4, "index", 0);
                            line.MakeEllipse(origin, upVector, xRadius, yRadius, startDegrees, endDegrees, segments, pointRotation, index);
                        }
                        break;
                    case "spline": {
                            var segments = (int)lua.GetNumber(4, "segments", line.GetSegmentNumber());
                            var index = (int)lua.GetNumber(4, "index", 0);
                            var loop = lua.GetBoolean(4, "loop", false);
                            line.MakeSpline(list.ToArray(), segments, index, loop);
    
                        }
                        break;
                    case "curve": {
                            if (list.Count == 4) {
                                var segments = (int)lua.GetNumber(4, "segments", line.GetSegmentNumber());
                                line.MakeCurve(list[0], list[1], list[2], list[3], segments);
                            } else {
                                LogMgr.E("MakeCurve needs exactly 4 points in the curve points array");
                            }
    
                        }
                        break;
                }
            } else {
                line = new VectorLine(name, list, 1);
            }
            if (duration < 0) {
                line.Draw3D();
            } else {
                line.Draw3DAuto(duration);
            }
    
            lua.PushLightUserData(line);
            lua.CreateTable(line.points3.Count, 0);
            for (int i = 0; i < line.points3.Count; ++i) {
                lua.SetValue(I2V.PushVector3, -1, i + 1, line.points3[i]);
            }
    
            return 2;
        }
    
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int DestroyLine(ILuaState lua)
        {
            if (lua.IsString(1)) {
                if (LineManager.Instance) {
                    LineManager.Instance.DestroyLine(lua.ToString(1));
                } else {
    
                }
            } else {
                var line = lua.ToUserData(1) as VectorLine ;
                if (line != null) {
                    VectorLine.Destroy(ref line);
                    MetaMethods.GC(lua);
                }
            }
            return 0;
        }
         */
    }
}

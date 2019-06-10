using System.Collections;
using System.Collections.Generic;
using XLua;

public static class WrapDefines
{
    [LuaCallCSharp]
    public static List<System.Type> WrapClasses {
        get {
            return new List<System.Type>() {
                typeof(System.Object),
                typeof(System.Type),

                // Unity - Object
                typeof(UnityEngine.WWW),
                typeof(UnityEngine.Object),
                typeof(UnityEngine.Shader),
                typeof(UnityEngine.Renderer),
                typeof(UnityEngine.Collider),
                typeof(UnityEngine.Texture),
                typeof(UnityEngine.GameObject),
                typeof(UnityEngine.Transform),
                typeof(UnityEngine.RectTransform),
                typeof(UnityEngine.Component),
                typeof(UnityEngine.Behaviour),
                typeof(UnityEngine.MonoBehaviour),
                typeof(UnityEngine.Camera),
                typeof(UnityEngine.Animator),
                typeof(UnityEngine.Animation),
                typeof(UnityEngine.Material),
                typeof(UnityEngine.Projector),

                // Unity - NavMesh
                typeof(UnityEngine.AI.NavMeshAgent),
                typeof(UnityEngine.AI.NavMeshObstacle),
        
                // Unity - Static
                typeof(UnityEngine.Time),
                typeof(UnityEngine.Application),
                typeof(UnityEngine.PlayerPrefs),
                typeof(UnityEngine.SystemInfo),
                typeof(UnityEngine.Screen),
                typeof(UnityEngine.RenderSettings),
                typeof(UnityEngine.QualitySettings),
                                    
                // network
                typeof(ZFrame.NetEngine.TcpClientHandler),
                //typeof(clientlib.net.NetMsg),

                typeof(GTime),
            
                // UGUI - Base
                typeof(UnityEngine.Canvas),
                typeof(UnityEngine.CanvasGroup),
                typeof(UnityEngine.EventSystems.UIBehaviour),
                //typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.BaseEventData),
                typeof(UnityEngine.EventSystems.PointerEventData),
                typeof(UnityEngine.UI.Graphic),
                typeof(UnityEngine.UI.MaskableGraphic),
                typeof(UnityEngine.UI.Text),
                typeof(UnityEngine.UI.Image),
                typeof(UnityEngine.UI.RawImage),

                typeof(UnityEngine.UI.Selectable),
                typeof(UnityEngine.UI.Button),
                typeof(UnityEngine.UI.Toggle),
                //typeof(UnityEngine.UI.InputField),
                typeof(UnityEngine.UI.Slider),
                typeof(UnityEngine.UI.Dropdown),
                typeof(UnityEngine.UI.Dropdown.OptionData),
                typeof(UnityEngine.UI.Scrollbar),
                typeof(UnityEngine.UI.ScrollRect),
                typeof(UnityEngine.UI.InputField),
                //typeof(UnityEngine.UI.LayoutGroup),
                //typeof(UnityEngine.UI.GridLayoutGroup),

                // UGUI - Override
                typeof(ZFrame.UGUI.UILabel),
                typeof(ZFrame.UGUI.UISprite),
                typeof(ZFrame.UGUI.UITexture),
                typeof(ZFrame.UGUI.UIButton),
                typeof(ZFrame.UGUI.UIToggle),
                typeof(ZFrame.UGUI.UISlider),
                typeof(ZFrame.UGUI.UIDropdown),
                //typeof(ZFrame.UGUI.UIScrollView),
                //typeof(ZFrame.UGUI.FollowUITarget),
                typeof(ZFrame.UGUI.UIDragged),
                typeof(ZFrame.UGUI.UIProgress),
                typeof(ZFrame.UGUI.UISliding),
                //typeof(ZFrame.UGUI.UIText),
                typeof(ZFrame.UGUI.UISelectable),

                //typeof(TMPro.TMP_InputField),
                
                typeof(ZFrame.UGUI.UIGroup),

                // Tweener
                typeof(ZFrame.Tween.ZTweener),
                typeof(ZFrame.Tween.BaseTweener),

                // Other
                typeof(CMD5),

                typeof(RadioWave),

            };
        }
    }

    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>() {
        
        new List<string>() { "UnityEngine.MonoBehaviour", "runInEditMode" },
        new List<string>() { "UnityEngine.Texture", "imageContentsHash" },
        new List<string>() { "UnityEngine.UI.Graphic", "OnRebuildRequested" },
        new List<string>() { "UnityEngine.UI.Text", "OnRebuildRequested" },
        new List<string>() { "UnityEngine.WWW", "GetMovieTexture" },
        new List<string>() { "UnityEngine.QualitySettings", "streamingMipmapsRenderersPerFrame" },

        new List<string>() { "ZFrame.UGUI.UIButton", "OnPointerClick", "UnityEngine.EventSystems.PointerEventData" },
        new List<string>() { "ZFrame.UGUI.UIButton", "SetEvent", "ZFrame.UGUI.TriggerType", "ZFrame.UGUI.UIEvent", "System.String" },
        
        new List<string>() { "ZFrame.UGUI.UIToggle", "OnPointerClick", "UnityEngine.EventSystems.PointerEventData" },
        new List<string>() { "ZFrame.UGUI.UIToggle", "SetEvent", "ZFrame.UGUI.TriggerType", "ZFrame.UGUI.UIEvent", "System.String" },
        
        new List<string>() { "ZFrame.UGUI.UIDropdown", "SetEvent", "ZFrame.UGUI.TriggerType", "ZFrame.UGUI.UIEvent", "System.String" },
        new List<string>() { "ZFrame.UGUI.UIInput", "SetEvent", "ZFrame.UGUI.TriggerType", "ZFrame.UGUI.UIEvent", "System.String" },
        new List<string>() { "ZFrame.UGUI.UIScrollView", "SetEvent", "ZFrame.UGUI.TriggerType", "ZFrame.UGUI.UIEvent", "System.String" },
        new List<string>() { "ZFrame.UGUI.UISlider", "SetEvent", "ZFrame.UGUI.TriggerType", "ZFrame.UGUI.UIEvent", "System.String" },
        new List<string>() { "ZFrame.UGUI.UILoopGrid", "SetEvent", "ZFrame.UGUI.TriggerType", "ZFrame.UGUI.UIEvent", "System.String" },
        new List<string>() { "ZFrame.UGUI.UIProgress", "SetEvent", "ZFrame.UGUI.TriggerType", "ZFrame.UGUI.UIEvent", "System.String" },
        
        new List<string>() { "ZFrame.UGUI.UISprite", "Load", "System.String", "ZFrame.Asset.DelegateObjectLoaded", "System.Object" },
        new List<string>() { "ZFrame.UGUI.UISprite", "LoadAtlas", "System.String", "UnityEngine.Object" },
        new List<string>() { "ZFrame.UGUI.UISprite", "LoadSprite", "System.String", "UnityEngine.Object" },
        
        new List<string>() { "ZFrame.UGUI.UISliding", "OnBeginDrag", "UnityEngine.EventSystems.PointerEventData" },
        new List<string>() { "ZFrame.UGUI.UISliding", "OnEndDrag", "UnityEngine.EventSystems.PointerEventData" },
        new List<string>() { "ZFrame.UGUI.UISliding", "OnDrag", "UnityEngine.EventSystems.PointerEventData" },
        
        new List<string>() { "ZFrame.UGUI.UITransmissionClick", "OnPointerClick", "UnityEngine.EventSystems.PointerEventData" },
        
        new List<string>() { "ZFrame.UGUI.UILabel", "LOC" },
        new List<string>() { "ZFrame.UGUI.UILabel", "rawText" },
        
        new List<string>() { "ZFrame.Tween.ZTweener", "tween" },
    };
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using ILuaState = System.IntPtr;

namespace ZFrame.Lua
{
    using UGUI;
    [AddComponentMenu("LUA/Lua Component")]
    public class LuaComponent : UIWindow
    {
        public const string PKG = "PKG";
        public const string UI = "ui";
        public const string PACK_WINDOW = "framework/ui/window";
        public static Component LastSender;

        [HideInInspector, NamedProperty("Lua脚本")]
        public string luaScript;
#if UNITY_EDITOR
        private const string SHOW_VIEW = "show_view";
        private const string FUNC_RECYCLE = "on_recycle";
        [HideInInspector] public List<string> LocalMethods = new List<string> {SHOW_VIEW, FUNC_RECYCLE};
#endif

        public static ILuaState lua {
            get { return LuaScriptMgr.Instance.L; }
        }

        public static string GetPackName(string scriptPath)
        {
            if (string.IsNullOrEmpty(scriptPath)) return null;

            var pntIndex = scriptPath.LastIndexOf('.');
            var pkgName = pntIndex > -1 ? scriptPath.Remove(pntIndex) : scriptPath;

            return pkgName;
        }

        public string GetPackName()
        {
            return GetPackName(luaScript);
        }

        protected override void Awake()
        {
            base.Awake();
            gameObject.NeedComponent(typeof(GraphicRaycaster));
        }

        protected override void Start()
        {
            base.Start();
            gameObject.SetEnable(typeof(GraphicRaycaster), true);

            lua.GetGlobal(PKG, PACK_WINDOW, "on_open");
            var b = lua.BeginPCall();
            lua.PushLightUserData(gameObject);
            lua.PushString(GetPackName());
            lua.ExecPCall(2, 0, b);
        }

        public override void OnRecycle()
        {
            if (LuaScriptMgr.Instance) {
                lua.GetGlobal(PKG, PACK_WINDOW, "on_close");
                var b = lua.BeginPCall();
                lua.PushLightUserData(gameObject);
                lua.PushString(GetPackName());
                lua.ExecPCall(2, 0, b);
            }

            gameObject.SetEnable(typeof(GraphicRaycaster), false);
            base.OnRecycle();
        }

        public override void SendEvent(Component sender, UIEvent eventName, string eventParam, object data = null)
        {
            if (enabled) SendUIEvent(cachedName, sender, eventName, eventParam, data);
        }

        public static void SendUIEvent(string wndName, Component sender, UIEvent eventName, string eventParam, object data = null)
        {
            if (eventName != UIEvent.Close && string.IsNullOrEmpty(eventParam)) {
                // 无效的消息
                return;
            }

#if UNITY_ASSERTIONS

#endif
#if UNITY_EDITOR
            if (LuaScriptMgr.Instance == null) return;
#endif

            if (sender is IEventSender) {
                LastSender = sender;
            }

            switch (eventName) {
                case UIEvent.Auto:
                case UIEvent.Send: {
                    lua.GetGlobal(PKG, PACK_WINDOW, "on_event");
                    var b = lua.BeginPCall();
                    lua.PushString(wndName);
                    lua.PushString(eventParam);
                    lua.PushLightUserData(sender);
                    lua.PushAnyObject(data);
                    lua.ExecPCall(4, 0, b);
                    break;
                }
                case UIEvent.Open:
                    OpenWindow(eventParam);
                    break;
                case UIEvent.PopWindow:
                    OpenWindow(eventParam, UIManager.Instance.GetTopDepth() + 1);
                    break;
                case UIEvent.Show:
                    ShowWindow(eventParam);
                    break;
                case UIEvent.Close: {
                    lua.GetGlobal(UI, "close");
                    var b = lua.BeginPCall();
                    lua.PushString(wndName);
                    lua.ExecPCall(1, 0, b);
                    break;
                }
                default: break;
            }
        }

        public static void OpenWindow(string path, int depth = 0)
        {
            lua.GetGlobal(UI, "open");
            var b = lua.BeginPCall();
            lua.PushString(path);
            lua.PushInteger(depth);
            lua.ExecPCall(2, 0, b);
        }

        public static void ShowWindow(string path, int depth = 0)
        {
            lua.GetGlobal(UI, "show");
            var b = lua.BeginPCall();
            lua.PushString(path);
            lua.PushInteger(depth);
            lua.ExecPCall(2, 0, b);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ZFrame
{
    using Platform;

    public class AppToLua : MonoSingleton<AppToLua>
    {
        [SerializeField] private string m_Notify2Lua;
        [SerializeField] private string m_SDK2Lua;

        protected override void Awaking()
        {
            base.Awaking();

            Application.lowMemory += OnLowMemory;
            NotifyMgr.onAppPause += OnAppPause;
            NotifyMgr.onAppFocus += OnAppFocus;
            SDKManager.onSDKMessage += OnSDKMessage;
        }

        protected override void Destroying()
        {
            base.Destroying();

            Application.lowMemory -= OnLowMemory;
            NotifyMgr.onAppPause -= OnAppPause;
            NotifyMgr.onAppFocus -= OnAppFocus;
            SDKManager.onSDKMessage -= OnSDKMessage;
        }

        private void CallMethod(string package, string method)
        {
            var lua = LuaScriptMgr.Instance.L;
            lua.GetGlobal("PKG", package, method);
            lua.Func(0);
        }

        private void CallMethod(string package, string method, bool value)
        {
            var lua = LuaScriptMgr.Instance.L;
            lua.GetGlobal("PKG", package, method);
            var b = lua.BeginPCall();
            lua.PushBoolean(value);
            lua.ExecPCall(1, 0, b);
        }

        private void CallMethod(string package, string method, string value)
        {
            var lua = LuaScriptMgr.Instance.L;
            lua.GetGlobal("PKG", package, method);
            var b = lua.BeginPCall();
            lua.PushString(value);
            lua.ExecPCall(1, 0, b);
        }

        private void OnLowMemory()
        {
            CallMethod(m_Notify2Lua, "on_mem_warning");
        }

        private void OnAppPause(bool pause)
        {
            CallMethod(m_Notify2Lua, "on_app_pause", pause);
        }

        private void OnAppFocus(bool focused)
        {
            CallMethod(m_Notify2Lua, "on_app_focus", focused);
        }

        private void OnSDKMessage(string message)
        {
            CallMethod(m_SDK2Lua, "sdk_message", message);
        }
    }
}

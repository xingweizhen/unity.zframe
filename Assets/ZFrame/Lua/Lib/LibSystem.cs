using UnityEngine;
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
    using Platform;
    public static class LibSystem
    {
        public const string LIB_NAME = "libsystem.cs";
        private static float s_SecondsOffset;
        private static long s_ZeroTicks;

        public static long GetNowMSeconds()
        {
            var now = System.DateTime.Now;
            now = now.AddSeconds(s_SecondsOffset);
            return (now.Ticks - s_ZeroTicks) / 10000;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int OpenLib(ILuaState lua)
        {
            lua.NewTable();

            lua.SetDict("GetHashCode", GetHashCode);
            lua.SetDict("DateTimeSeconds", DateTimeSeconds);
            lua.SetDict("SyncServerTime", SyncServerTime);

            // 位运算
            lua.SetDict("BitOr", BitOr);
            lua.SetDict("BitAnd", BitAnd);
            lua.SetDict("BitXor", BitXor);

            lua.SetDict("StringFmt", StringFmt);
            lua.SetDict("NumberFmt", NumberFmt);
            lua.SetDict("GetMacAddr", GetMacAddr);
            lua.SetDict("GetAvailableStorage", GetAvailableStorage);

            lua.SetDict("SetAppTitle", SetAppTitle);
            lua.SetDict("ProcessingData", ProcessingData);
            lua.SetDict("SysCall", SysCall);
            lua.SetDict("RequestPermission", RequestPermission);

            s_ZeroTicks = new System.DateTime(1970, 1, 1, 8, 0, 0).Ticks;
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetHashCode(ILuaState lua)
        {
            object o = lua.ToAnyObject(1);
            lua.PushInteger(o.GetHashCode());
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int DateTimeSeconds(ILuaState lua)
        {
            System.Object o = lua.ToUserData(1);
            System.DateTime now = o != null ? (System.DateTime)o : System.DateTime.Now;
            System.DateTime origin = new System.DateTime(1970, 1, 1, 8, 0, 0);
            var tick = now.Ticks - origin.Ticks;
            var span = new System.TimeSpan(tick);
            lua.PushNumber(span.TotalSeconds);

            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int SyncServerTime(ILuaState lua)
        {
            s_SecondsOffset = -lua.ToSingle(1);
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int BitOr(ILuaState lua)
        {
            var a = lua.ChkInteger(1);
            var b = lua.ChkInteger(2);
            lua.PushInteger(a | b);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int BitAnd(ILuaState lua)
        {
            var a = lua.ChkInteger(1);
            var b = lua.ChkInteger(2);
            lua.PushInteger(a & b);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int BitXor(ILuaState lua)
        {
            var a = lua.ChkInteger(1);
            var b = lua.ChkInteger(2);
            lua.PushInteger(a ^ b);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int StringFmt(ILuaState lua)
        {
            lua.PushString(lua.ToCSFormatString(1));
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int NumberFmt(ILuaState lua)
        {
            var num = lua.ToNumber(1);
            var fmt = lua.ToString(2);
            lua.PushString(num.ToString(fmt));
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int GetMacAddr(ILuaState lua)
        {
            string macAddr = null;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS
            var nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            for (int i = 0; i < nics.Length; ++i) {
                var ni = nics[i];
                if (ni.Description == "en0") {
                    macAddr = ni.GetPhysicalAddress().ToString();
                    break;
                } else {
                    macAddr = ni.GetPhysicalAddress().ToString();
                    if (string.IsNullOrEmpty(macAddr)) continue;
                    break;
                }
            }

#elif UNITY_ANDROID
        try {
			macAddr = SDKManager.Instance.plat.Call<string>("com.rongygame.util", "GetMacAddress");
        } catch { }
#endif

            if (macAddr == null) macAddr = "00:00:00:00";
            lua.PushString(macAddr);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetAvailableStorage(ILuaState lua)
        {
            lua.PushInteger(1024 * 1024 * 1024);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetAppTitle(ILuaState lua)
        {
            try {
                var plat = SDKManager.Instance.plat as Standalone;
                if (plat != null) {
                    var productName = Application.productName;
                    var version = ZFrame.Asset.VersionMgr.AssetVersion.version;
#if RELEASE
                var appTitle = string.Format("{0} r{1}", productName, version);
#else
                    var appTitle = string.Format("{0} {1}", productName, version);
#endif
                    var title = lua.OptString(1, string.Empty);

                    if (string.IsNullOrEmpty(title)) {
                        plat.SetAppTitle(appTitle);
                    } else {
                        plat.SetAppTitle(string.Format("{0} - {1}", appTitle, title));
                    }
                }
            } catch (System.Exception e) {
                LogMgr.E(e.Message);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int ProcessingData(ILuaState lua)
        {
            string json = lua.ChkString(1);
            var ret = SDKManager.Instance.plat.ProcessingData(json);
            lua.PushString(ret ?? string.Empty);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SysCall(ILuaState lua)
        {
            string json = lua.ChkString(1);
            var jsonObj = TinyJSON.JSON.Load(json);
            string className = jsonObj["class"];
            string methodName = jsonObj["method"];
            string paramStr = jsonObj["param"];
            lua.PushString(SDKManager.Instance.plat.Call<string>(className, methodName, paramStr));
            LogMgr.I("SysCall: " + json);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int RequestPermission(ILuaState lua)
        {
            var permission = lua.ToString(1);
            var ret = SDKManager.Instance.plat.RequestPermission(permission);
            lua.PushBoolean(ret);
            return 1;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaAPI = XLua.LuaDLL.Lua;
using ILuaState = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;

namespace XLua
{
    public partial class StaticLuaCallbacks
    {
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int loadfile(ILuaState L)
        {
            string fileName = L.ToString(1);
            var oldTop = L.GetTop();
            L.PushErrorFunc();
            if (L.LoadFile(fileName) == LuaThreadStatus.LUA_OK) {
                return 1;
            }
            L.ToTranslator().luaEnv.ThrowExceptionFromError(oldTop);
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int dofile(ILuaState L)
        {
            string fileName = L.ToString(1);
            return L.DoFile(fileName);
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int lgetmetatable(ILuaState L)
        {
            string name = L.ToString(1);
            L.L_GetMetaTable(name);
            return 1;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int lsetmetatable(ILuaState L)
        {
            L.SetMetaTable(1);
            L.PushValue(1);
            return 1;
        }
        
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int print(ILuaState L)
        {
            try
            {
                int n = LuaAPI.lua_gettop(L);
                string s = System.String.Empty;

                if (0 != LuaAPI.xlua_getglobal(L, "tostring"))
                {
                    return LuaAPI.luaL_error(L, "can not get tostring in print:");
                }

                for (int i = 1; i <= n; i++)
                {
                    LuaAPI.lua_pushvalue(L, -1);  /* function to be called */
                    LuaAPI.lua_pushvalue(L, i);   /* value to print */
                    if (0 != LuaAPI.lua_pcall(L, 1, 1, 0))
                    {
                        return LuaAPI.lua_error(L);
                    }
                    s += LuaAPI.lua_tostring(L, -1);

                    if (i != n) s += "\t";

                    LuaAPI.lua_pop(L, 1);  /* pop result */
                }
                
                LogMgr.D("{0}: {1}", L.DebugCurrentLine(2), s);
                return 0;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in print:" + e);
            }
        }
    }
}

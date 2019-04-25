using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
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
    }
}

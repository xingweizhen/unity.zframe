using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ULUA

#else
namespace XLua
{
    using LuaDLL;

    public partial class LuaFunction : LuaBase
    {
        public System.IntPtr GetState() { return luaEnv.L; }
            
        public int BeginPCall()
        {
            var L = luaEnv.L;
            int oldTop = Lua.lua_gettop(L);
            Lua.load_error_func(L, luaEnv.errorFuncRef);
            Lua.lua_getref(L, luaReference);
            return oldTop;
        }
        public bool PCall(int oldTop, int nArgs)
        {
            var L = luaEnv.L;
            if (L.PCall(nArgs, -1, - nArgs - 2) != LuaThreadStatus.LUA_OK) {
                luaEnv.ThrowExceptionFromError(oldTop);
                L.SetTop(oldTop);
                return false;
            }

            return true;
           
        }
        public void EndPCall(int oldTop)
        {
            luaEnv.L.SetTop(oldTop);
        }

        public void Action<T1, T2, T3>(T1 a1, T2 a2, T3 a3)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
            var L = luaEnv.L;
            var translator = luaEnv.translator;
            int oldTop = Lua.lua_gettop(L);
            int errFunc = Lua.load_error_func(L, luaEnv.errorFuncRef);
            Lua.lua_getref(L, luaReference);
            translator.PushByType(L, a1);
            translator.PushByType(L, a2);
            translator.PushByType(L, a3);
            int error = Lua.lua_pcall(L, 3, 0, errFunc);
            if (error != 0)
                luaEnv.ThrowExceptionFromError(oldTop);
            Lua.lua_settop(L, oldTop);
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }
    }
}
#endif

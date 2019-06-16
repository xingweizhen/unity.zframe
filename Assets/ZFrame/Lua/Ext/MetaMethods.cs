using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
using LuaDLL = XLua.LuaDLL.Lua;
#endif
using ILuaState = System.IntPtr;

public static class MetaMethods
{
    public static LuaCSFunction GC = new LuaCSFunction(StaticLuaCallbacks.LuaGC);

    public static LuaCSFunction Index = new LuaCSFunction(__index);
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int __index(ILuaState L)
    {
        var top = L.GetTop();
        // 从元表取出field的值
        string field = L.ToString(2);
        L.GetMetaTable(1);
        L.PushString(field);
        L.RawGet(-2);
        L.Remove(-2);

        var luaT = L.Type(-1);
        if (luaT == LuaTypes.LUA_TTABLE) {
            // 该field是个属性
            L.RawGetI(-1, 1);
            L.PushValue(1);
            try {
                L.PCall(1, 1, 0);
                L.Remove(-2);
            } catch (System.Exception e) {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
                L.SetTop(top);
                return 0;
            }
        } else if (luaT == LuaTypes.LUA_TFUNCTION) {
            // 函数直接返回
        } else {
            L.L_Error(string.Format("field or property {0} does not exist, get {1}", field, luaT));
            L.Pop(1);
            return 0;
        }

        return 1;
    }

    public static LuaCSFunction NewIndex = new LuaCSFunction(__newindex);
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int __newindex(ILuaState L)
    {
        var top = L.GetTop();
        // 从元表取出field的值
        string field = L.ToString(2);
        L.GetMetaTable(1);
        L.PushString(field);
        L.RawGet(-2);
        L.Remove(-2);

        var luaT = L.Type(-1);
        if (luaT == LuaTypes.LUA_TTABLE) {
            // 该field只能是个属性
            L.RawGetI(-1, 2);
            L.PushValue(1);
            try {
                L.PCall(1, 1, 0);
                L.Remove(-2);
            } catch (System.Exception e) {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
                L.SetTop(top);
                return 0;
            }
        } else {
            L.L_Error(string.Format("field or property {0} does not exist, get {1}", field, luaT));
            L.Pop(1);
            return 0;
        }

        return 1;
    }
    
    //private static LuaCSFunction closure = new LuaCSFunction(__closure);
    //private static int __closure(ILuaState L)
    //{
    //    var top = L.GetTop();
    //    var ptr = LuaDLL.lua_tocfunction(L, LuaDLL.xlua_upvalueindex(1));
    //    var func = Marshal.GetDelegateForFunctionPointer(ptr, typeof(LuaCSFunction)) as LuaCSFunction;
    //    try {
    //        return func.Invoke(L);
    //    } catch (System.Exception e) {
    //        Debug.LogError(e.Message + "\n" + e.StackTrace);
    //        return top;
    //    }
    //}

    public static void PushCSharpProtectedFunction(this ILuaState self, LuaCSFunction value)
    {
        //self.PushCSharpFunction(value);
        //LuaDLL.lua_pushstdcallcfunction(self, closure, 1);
        LuaDLL.lua_pushstdcallcfunction(self, value, 0);
    }

}

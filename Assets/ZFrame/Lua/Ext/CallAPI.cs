using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using ILuaState = System.IntPtr;

public static class CallAPI
{
    public static int BeginPCall(this ILuaState self)
    {
        var b = self.GetTop();
        self.PushErrorFunc();
        self.Insert(b);

        return b;
    }

    public static bool ExecPCall(this ILuaState self, int nArgs, int nResult, int b)
    {
        if (self.PCall(nArgs, nResult, b) == LuaThreadStatus.LUA_OK) {
            self.Remove(b);
            return true;
        } else {
            try {
                var translator = self.ToTranslator();
                translator.luaEnv.ThrowExceptionFromError(b - 1);
            } catch (System.Exception e) {
                Debug.LogException(e);
            }
            return false;
        }
    }

    /// <summary>
    /// 调用栈顶函数，无参数
    /// </summary>
    /// <param name="self">Lua状态及指针</param>
    /// <param name="nResult">返回值数量.</param>
    public static bool Func(this ILuaState self, int nResult)
    {
        var b = self.BeginPCall();
        return self.ExecPCall(0, nResult, b);
    }

    /// <summary>
    /// 调用栈顶函数，传入一个参数
    /// </summary>
    /// <param name="self">Lua状态及指针</param>
    /// <param name="nResult">返回值数量.</param>
    /// <param name="arg0">参数1.</param>
    public static bool Func<T>(this ILuaState self, int nResult, T arg0)
    {
        var b = self.BeginPCall();
        self.PushByType(arg0);
        return self.ExecPCall(1, nResult, b);
    }

    /// <summary>
    /// 调用栈顶函数，传入两个参数
    /// </summary>
    /// <param name="self">Lua状态及指针</param>
    /// <param name="nResult">返回值数量.</param>
    /// <param name="arg0">参数1.</param>
    /// <param name="arg1">参数2.</param>
    public static bool Func<T1, T2>(this ILuaState self, int nResult, T1 arg0, T2 arg1)
    {
        var translator = self.ToTranslator();
        var b = self.BeginPCall();
        translator.PushByType(self, arg0);
        translator.PushByType(self, arg1);
        return self.ExecPCall(2, nResult, b);
    }

    /// <summary>
    /// 调用栈顶函数，传入三个参数
    /// </summary>
    /// <param name="self">Lua状态及指针</param>
    /// <param name="nResult">返回值数量.</param>
    /// <param name="arg0">参数1.</param>
    /// <param name="arg1">参数2.</param>
    /// <param name="arg2">参数3.</param>
    public static bool Func<T1, T2, T3>(this ILuaState self, int nResult, T1 arg0, T2 arg1, T3 arg2)
    {
        var translator = self.ToTranslator();
        var b = self.BeginPCall();
        translator.PushByType(self, arg0);
        translator.PushByType(self, arg1);
        translator.PushByType(self, arg2);
        return self.ExecPCall(3, nResult, b);
    }

    /// <summary>
    /// 调用栈顶函数，传入四个参数
    /// </summary>
    /// <param name="self">Lua状态及指针</param>
    /// <param name="nResult">返回值数量.</param>
    /// <param name="arg0">参数1.</param>
    /// <param name="arg1">参数2.</param>
    /// <param name="arg2">参数3.</param>
    /// <param name="arg3">参数4.</param>
    public static bool Func<T1, T2, T3, T4>(this ILuaState self, int nResult, T1 arg0, T2 arg1, T3 arg2, T4 arg3)
    {
        var translator = self.ToTranslator();
        var b = self.BeginPCall();
        translator.PushByType(self, arg0);
        translator.PushByType(self, arg1);
        translator.PushByType(self, arg2);
        translator.PushByType(self, arg3);
        return self.ExecPCall(4, nResult, b);
    }

    /// <summary>
    /// 调用栈顶函数，传入五个参数
    /// </summary>
    /// <param name="self">Lua状态及指针</param>
    /// <param name="nResult">返回值数量.</param>
    /// <param name="arg0">参数1.</param>
    /// <param name="arg1">参数2.</param>
    /// <param name="arg2">参数3.</param>
    /// <param name="arg3">参数4.</param>
    /// <param name="arg4">参数5.</param>
    public static bool Func<T1, T2, T3, T4, T5>(this ILuaState self, int nResult, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4)
    {
        var translator = self.ToTranslator();
        var b = self.BeginPCall();
        translator.PushByType(self, arg0);
        translator.PushByType(self, arg1);
        translator.PushByType(self, arg2);
        translator.PushByType(self, arg3);
        translator.PushByType(self, arg4);
        return self.ExecPCall(5, nResult, b);
    }

}

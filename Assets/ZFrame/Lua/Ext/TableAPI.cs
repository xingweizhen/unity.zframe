using UnityEngine;
using System.Collections;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
using LuaState = XLua.LuaEnv;
#endif
using ILuaState = System.IntPtr;

public static class TableAPI
{
    public const string META_CLASS = "__name";
        
    public static void SetDict(this ILuaState self, string key, float value)
    {
        self.PushString(key);
        self.PushNumber(value);
        self.RawSet(-3);
    }

    public static void SetDict(this ILuaState self, string key, string value)
    {
        self.PushString(key);
        self.PushString(value);
        self.RawSet(-3);
    }

    public static void SetDict(this ILuaState self, string key, bool value)
    {
        self.PushString(key);
        self.PushBoolean(value);
        self.RawSet(-3);
    }

    public static void SetDict(this ILuaState self, string key, TinyJSON.Variant json)
    {
        self.PushString(key);
        self.PushX(json);
        self.RawSet(-3);
    }

    public static void SetDict(this ILuaState self, string key, LuaCSFunction func)
    {
        self.PushString(key);
        self.PushCSharpFunction(func);
        self.RawSet(-3);
    }
    
    #region Table - Number Value
    public static float GetNumber(this ILuaState self, int index, string key, float def = 0)
    {
        self.GetField(index, key);
        var ret = self.OptSingle(-1, def);
        self.Pop(1);
        return ret;
    }

    public static float GetNumber(this ILuaState self, int index, int n, float def = 0)
    {
        self.GetField(index, n);
        var ret = self.OptSingle(-1, def);
        self.Pop(1);
        return ret;
    }

    public static int GetInteger(this ILuaState self, int index, string key, int def = 0)
    {
        self.GetField(index, key);
        var ret = self.OptInteger(-1, def);
        self.Pop(1);
        return ret;
    }

    public static int GetInteger(this ILuaState self, int index, int n, int def = 0)
    {
        self.GetField(index, n);
        var ret = self.OptInteger(-1, def);
        self.Pop(1);
        return ret;
    }


    public static void SetNumber(this ILuaState self, int index, string key, float value)
    {
        self.AbsIndex(ref index);
        self.PushString(key);
        self.PushNumber(value);
        self.SetTable(index);
    }
   
    public static void SetNumber(this ILuaState self, int index, int n, float value)
    {
        self.AbsIndex(ref index);
        self.PushInteger(n);
        self.PushNumber(value);
        self.SetTable(index);
    }
    #endregion

    #region Table - String Value
    public static string GetString(this ILuaState self, int index, string key, string def = null)
    {
        self.GetField(index, key);
        var ret = self.OptString(-1, def);
        self.Pop(1);
        return ret;
    }

    public static string GetString(this ILuaState self, int index, int n, string def = null)
    {
        self.GetField(index, n);
        var ret = self.OptString(-1, def);
        self.Pop(1);
        return ret;
    }

    public static void SetString(this ILuaState self, int index, string key, string value)
    {
        self.AbsIndex(ref index);
        self.PushString(key);
        self.PushString(value);
        self.SetTable(index);
    }

    public static void SetString(this ILuaState self, int index, int n, string value)
    {
        self.AbsIndex(ref index);
        self.PushInteger(n);
        self.PushString(value);
        self.SetTable(index);
    }

    public static void RawSetString(this ILuaState self, int index, string key, string value)
    {
        self.AbsIndex(ref index);
        self.PushString(key);
        self.PushString(value);
        self.RawSet(index);
    }
    #endregion
    
    #region Table - Unity Object
    public static void SetUObj(this ILuaState self, int index, string key, Object value)
    {
        if (index < 0) index = self.GetTop() + 1 + index;

        self.PushString(key);
        self.PushX(value);
        self.SetTable(index);
    }

    public static void SetUObjI(this ILuaState self, int index, int n, Object value)
    {
        if (index < 0) index = self.GetTop() + 1 + index;

        self.PushInteger(n);
        self.PushX(value);
        self.SetTable(index);
    }
    #endregion

    public static bool GetBoolean(this ILuaState self, int index, string key, bool def = false)
    {
        self.GetField(index, key);
        var ret = self.OptBoolean(-1, def);
        self.Pop(1);
        return ret;
    }

    public static object GetEnum(this ILuaState self, int index, string key, System.Enum def)
    {
        self.GetField(index, key);
        var ret = self.ToEnumValue(-1, def);
        self.Pop(1);
        return ret;
    }

    public static LuaFunction GetFunction(this ILuaState self, int index, string key)
    {
        self.GetField(index, key);
        var ret = self.ToLuaFunction(-1);
        self.Pop(1);
        return ret;
    }

    public static object GetAny(this ILuaState self, int index, string key)
    {
        self.GetField(index, key);
        var ret = self.ToAnyObject(-1);
        self.Pop(1);
        return ret;
    }

    public static void SetValue<T>(this ILuaState self, I2V.Value2Top<T> pushValue, int index, string key, T value)
    {
        self.AbsIndex(ref index);
        self.PushString(key);
        pushValue(self, value);
        self.SetTable(index);
    }

    public static void SetValue<T>(this ILuaState self, I2V.Value2Top<T> pushValue, int index, int n, T value)
    {
        self.AbsIndex(ref index);
        self.PushInteger(n);
        pushValue(self, value);
        self.SetTable(index);
    }

    public static T GetValue<T>(this ILuaState self, I2V.Index2Value<T> indexTo, int index, string key, T def = default(T))
    {
        T ret = def;
        self.GetField(index, key);
        if (!self.IsNil(-1)) {
            indexTo(self, -1, out ret);
        }
        self.Pop(1);
        return ret;
    }
       

    public static T GetValue<T>(this ILuaState self, I2V.Index2Value<T> indexTo, int index, int n, T def = default(T))
    {
        T ret = def;
        self.GetField(index, n);
        if (!self.IsNil(-1)) {
            indexTo(self, -1, out ret);
        }
        self.Pop(1);
        return ret;
    }

}

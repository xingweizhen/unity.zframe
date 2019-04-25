using UnityEngine;
using System.Collections;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
using LuaDLL = XLua.LuaDLL.Lua;
#endif
using ILuaState = System.IntPtr;

//namespace ToLua.Wrap
//{
public static class UnityEngine_Vector2
{
    public const string CLASS = "UnityEngine.Vector2";

    public static void PushX(this ILuaState self, Vector2 value)
    {
        self.CreateTable(0, 2);
        self.SetDict("x", value.x);
        self.SetDict("y", value.y);
        self.L_GetMetaTable(CLASS);
        self.SetMetaTable(-2);
    }
    
    public static Vector2 ToVector2(this ILuaState self, int index)
    {
        var luaT = self.Type(index);
        switch (luaT) {
            case LuaTypes.LUA_TTABLE:
                float x = self.GetNumber(index, "x");
                float y = self.GetNumber(index, "y");
                return new Vector2(x, y);
            case LuaTypes.LUA_TNONE:
            case LuaTypes.LUA_TNIL:
                return Vector2.zero;
            default:
                LogMgr.W("Can't convert a {0} to a UnityEngine.Vector2. Fallback to Vector2.zero.", luaT);
                goto case LuaTypes.LUA_TNIL;
        }
    }

    public static bool IsVector2(this ILuaState self, int index)
    {
        return self.IsClass(index, CLASS);
    }

    public static void Update(ILuaState L, int index, Vector2 v2)
    {
        L.SetNumber(index, "x", v2.x);
        L.SetNumber(index, "y", v2.y);
    }

    public static void Wrap(ILuaState L)
    {
        L.WrapRegister(typeof(Vector2), _CreateVector2, new LuaMethod[] {
            new LuaMethod("Set", Set),
            new LuaMethod("Scale", Scale),
            new LuaMethod("Normalize", Normalize),
            new LuaMethod("SqrMagnitude", SqrMagnitude),
        }, new LuaField[] {
            new LuaField("normalized", get_normalized, null),
            new LuaField("magnitude", get_magnitude, null),
            new LuaField("sqrMagnitude", get_sqrMagnitude, null),
        }, new LuaMethod[] {
            new LuaMethod("Lerp", Lerp),
            new LuaMethod("LerpUnclamped", LerpUnclamped),
            new LuaMethod("Reflect", Reflect),
            new LuaMethod("MoveTowards", MoveTowards),
            new LuaMethod("SmoothDamp", SmoothDamp),
            new LuaMethod("Dot", Dot),
            new LuaMethod("Angle", Angle),
            new LuaMethod("Distance", Distance),
            new LuaMethod("ClampMagnitude", ClampMagnitude),
            new LuaMethod("Min", Min),
            new LuaMethod("Max", Max),
        }, new LuaField[] {
            new LuaField("kEpsilon", get_kEpsilon, null),
            new LuaField("zero", get_zero, null),
            new LuaField("one", get_one, null),
            new LuaField("up", get_up, null),
            new LuaField("down", get_down, null),
            new LuaField("left", get_left, null),
            new LuaField("right", get_right, null),
        }, new LuaMethod[] {
            new LuaMethod("__add", Lua_Add),
            new LuaMethod("__sub", Lua_Sub),
            new LuaMethod("__mul", Lua_Mul),
            new LuaMethod("__div", Lua_Div),
            new LuaMethod("__unm", Lua_Neg),
            new LuaMethod("__eq", Lua_Eq),
            new LuaMethod("__tostring", __tostring),
        });
    }
    
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int __tostring(ILuaState L)
    {
        L.PushString(L.ToVector2(1).ToString());
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int _CreateVector2(ILuaState L)
    {
        float x = L.ToSingle(2);
        float y = L.ToSingle(3);
        L.PushX(new Vector2(x, y));
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int GetClassType(ILuaState L)
    {
        int count = L.GetTop();
        if (count == 0) {
            L.PushLightUserData(typeof(Vector2));
            return 1;
        } else {
            return MetaMethods.GetType(L);
        }
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_kEpsilon(ILuaState L)
    {
        L.PushNumber(Vector2.kEpsilon);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_normalized(ILuaState L)
    {
        L.PushX(L.ToVector2(1).normalized);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_magnitude(ILuaState L)
    {
        L.PushNumber(L.ToVector2(1).magnitude);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_sqrMagnitude(ILuaState L)
    {
        L.PushNumber(L.ToVector2(1).sqrMagnitude);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_zero(ILuaState L)
    {
        L.PushX(Vector2.zero);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_one(ILuaState L)
    {
        L.PushX(Vector2.one);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_up(ILuaState L)
    {
        L.PushX(Vector2.up);
        return 1;
    }
    
    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_down(ILuaState L)
    {
#if UNITY_5
        L.PushUData(Vector2.down);
#else
        L.PushX(new Vector2(0, -1));
#endif
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_left(ILuaState L)
    {
#if UNITY_5
        L.PushUData(Vector2.left);
#else
        L.PushX(new Vector2(-1, 0));
#endif
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_right(ILuaState L)
    {
        L.PushX(Vector2.right);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Set(ILuaState L)
    {
        L.ChkArgsCount(3);
        Update(L, 1, new Vector2(L.ToSingle(2), L.ToSingle(3)));
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lerp(ILuaState L)
    {
        L.ChkArgsCount(3);
        Vector2 arg0 = L.ToVector2(1);
        Vector2 arg1 = L.ToVector2(2);
        var arg2 = (float)L.ChkNumber(3);
        Vector2 o = Vector2.Lerp(arg0, arg1, arg2);
        L.PushX(o);
        return 1;
    }
    
    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int LerpUnclamped(ILuaState L)
    {
        L.ChkArgsCount(3);
        Vector2 arg0 = L.ToVector2(1);
        Vector2 arg1 = L.ToVector2(2);
        var arg2 = (float)L.ChkNumber(3);
        Vector2 o = Vector2.LerpUnclamped(arg0, arg1, arg2);
        L.PushX(o);
        return 1;
    }
    
    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Reflect(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector2 arg0 = L.ToVector2(1);
        Vector2 arg1 = L.ToVector2(2);
        Vector2 o = Vector2.Reflect(arg0, arg1);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int MoveTowards(ILuaState L)
    {
        L.ChkArgsCount(3);
        Vector2 arg0 = L.ToVector2(1);
        Vector2 arg1 = L.ToVector2(2);
        var arg2 = (float)L.ChkNumber(3);
        Vector2 o = Vector2.MoveTowards(arg0, arg1, arg2);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Scale(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector2 obj = L.ToVector2(2);
        Vector2 arg0 = L.ToVector2(2);
        obj.Scale(arg0);
        Update(L, 1, obj);
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Normalize(ILuaState L)
    {
        L.ChkArgsCount(1);
        Vector2 obj = L.ToVector2(1);
        obj.Normalize();
        Update(L, 1, obj);
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int ToString(ILuaState L)
    {
        int count = LuaDLL.lua_gettop(L);

        if (count == 1) {
            Vector2 obj = L.ToVector2(2);
            string o = obj.ToString();
            L.PushString(o);
            return 1;
        } else if (count == 2) {
            Vector2 obj = L.ToVector2(2);
            var arg0 = L.ToLuaString(2);
            string o = obj.ToString(arg0);
            L.PushString(o);
            return 1;
        } else {
            LuaDLL.luaL_error(L, "invalid arguments to method: Vector2.ToString");
        }

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Dot(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector2 arg0 = L.ToVector2(1);
        Vector2 arg1 = L.ToVector2(2);
        float o = Vector2.Dot(arg0, arg1);
        L.PushNumber(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Angle(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector2 arg0 = L.ToVector2(1);
        Vector2 arg1 = L.ToVector2(2);
        float o = Vector2.Angle(arg0, arg1);
        L.PushNumber(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Distance(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector2 arg0 = L.ToVector2(1);
        Vector2 arg1 = L.ToVector2(2);
        float o = Vector2.Distance(arg0, arg1);
        L.PushNumber(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int ClampMagnitude(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector2 arg0 = L.ToVector2(1);
        var arg1 = (float)L.ChkNumber(2);
        Vector2 o = Vector2.ClampMagnitude(arg0, arg1);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int SqrMagnitude(ILuaState L)
    {
        L.ChkArgsCount(1);
        Vector2 obj = L.ToVector2(1);
        float o = obj.SqrMagnitude();
        L.PushNumber(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Min(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector2 arg0 = L.ToVector2(1);
        Vector2 arg1 = L.ToVector2(2);
        Vector2 o = Vector2.Min(arg0, arg1);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Max(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector2 arg0 = L.ToVector2(1);
        Vector2 arg1 = L.ToVector2(2);
        Vector2 o = Vector2.Max(arg0, arg1);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int SmoothDamp(ILuaState L)
    {
        int count = LuaDLL.lua_gettop(L);        
		if (count == 6) {
            Vector2 arg0 = L.ToVector2(1);
            Vector2 arg1 = L.ToVector2(2);
            Vector2 arg2 = L.ToVector2(3);
            var arg3 = (float)L.ChkNumber(4);
            var arg4 = (float)L.ChkNumber(5);
            var arg5 = (float)L.ChkNumber(6);
            Vector2 o = Vector2.SmoothDamp(arg0, arg1, ref arg2, arg3, arg4, arg5);
            L.PushX(o);
            L.PushX(arg2);
            return 2;
        } else {
            LuaDLL.luaL_error(L, "invalid arguments to method: Vector2.SmoothDamp");
        }

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Add(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector2 arg0 = L.ToVector2(1);
        Vector2 arg1 = L.ToVector2(2);
        Vector2 o = arg0 + arg1;
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Sub(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector2 arg0 = L.ToVector2(1);
        Vector2 arg1 = L.ToVector2(2);
        Vector2 o = arg0 - arg1;
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Neg(ILuaState L)
    {
        L.ChkArgsCount(1);
        Vector2 arg0 = L.ToVector2(1);
        Vector2 o = -arg0;
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Mul(ILuaState L)
    {
        int count = LuaDLL.lua_gettop(L);

        if (count == 2 && L.IsNumber(1) && L.IsVector2(2)) {
            var arg0 = (float)L.ToNumber(1);
            Vector2 arg1 = L.ToVector2(2);
            Vector2 o = arg0 * arg1;
            L.PushX(o);
            return 1;
        } 
        
        if (count == 2 && L.IsVector2(1) && L.IsNumber(2)) {
            Vector2 arg0 = L.ToVector2(1);
            var arg1 = (float)L.ToNumber(2);
            Vector2 o = arg0 * arg1;
            L.PushX(o);
            return 1;
        } 
        
        LuaDLL.luaL_error(L, "invalid arguments to method: Vector2.op_Multiply");

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Div(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector2 arg0 = L.ToVector2(1);
        var arg1 = (float)L.ChkNumber(2);
        Vector2 o = arg0 / arg1;
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Eq(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector2 arg0 = L.ToVector2(1);
        Vector2 arg1 = L.ToVector2(2);
        bool o = arg0 == arg1;
        L.PushBoolean(o);
        return 1;
    }
}
//}

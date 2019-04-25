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
public static class UnityEngine_Vector4
{
    public const string CLASS = "UnityEngine.Vector4";

    public static void PushX(this ILuaState self, Vector4 value)
    {
        self.CreateTable(0, 4);
        self.SetDict("x", value.x);
        self.SetDict("y", value.y);
        self.SetDict("z", value.z);
        self.SetDict("w", value.z);
        self.L_GetMetaTable(CLASS);
        self.SetMetaTable(-2);
    }

    public static Vector4 ToVector4(this ILuaState self, int index)
    {
        var luaT = self.Type(index);
        switch (luaT) {
            case LuaTypes.LUA_TTABLE:
                float x = self.GetNumber(index, "x");
                float y = self.GetNumber(index, "y");
                float z = self.GetNumber(index, "z");
                float w = self.GetNumber(index, "w");
                return new Vector4(x, y, z, w);
            case LuaTypes.LUA_TNIL:
                return Vector4.zero;
            default:
                LogMgr.W("Can't convert a {0} to a UnityEngine.Quaternion. Fallback to Vector4.zero.", luaT);
                goto case LuaTypes.LUA_TNIL;
        }
    }

    public static bool IsVector4(this ILuaState self, int index)
    {
        return self.IsClass(index, CLASS);
    }

    public static void Update(ILuaState L, int index, Vector4 v4)
    {
        L.SetNumber(index, "x", v4.x);
        L.SetNumber(index, "y", v4.y);
        L.SetNumber(index, "z", v4.z);
        L.SetNumber(index, "w", v4.w);
    }

    public static void Wrap(ILuaState L)
    {
        L.WrapRegister(typeof(Vector4), _CreateVector4, new LuaMethod[] {
            new LuaMethod("Set", Set),
            new LuaMethod("Scale", Scale),
            new LuaMethod("Normalize", Normalize),
            new LuaMethod("Magnitude", Magnitude),
            new LuaMethod("SqrMagnitude", SqrMagnitude),
        }, new LuaField[] {
            new LuaField("normalized", get_normalized, null),
            new LuaField("magnitude", get_magnitude, null),
            new LuaField("sqrMagnitude", get_sqrMagnitude, null),
            new LuaField("normalized", get_normalized, null),
            new LuaField("magnitude", get_magnitude, null),
            new LuaField("sqrMagnitude", get_sqrMagnitude, null),
        }, new LuaMethod[] {
            new LuaMethod("Lerp", Lerp),
            new LuaMethod("MoveTowards", MoveTowards),
            new LuaMethod("Dot", Dot),
            new LuaMethod("Project", Project),
            new LuaMethod("Distance", Distance),
            new LuaMethod("Min", Min),
            new LuaMethod("Max", Max),
        }, new LuaField[] {
            new LuaField("kEpsilon", get_kEpsilon, null),
            new LuaField("zero", get_zero, null),
            new LuaField("one", get_one, null),
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
        L.PushString(L.ToVector4(1).ToString());
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int _CreateVector4(ILuaState L)
    {
        int count = L.GetTop() - 1;

        if (count == 2) {
            var arg0 = (float)L.ChkNumber(2);
            var arg1 = (float)L.ChkNumber(3);
            Vector4 obj = new Vector4(arg0, arg1);
            L.PushX(obj);
            return 1;
        } else if (count == 3) {
            var arg0 = (float)L.ChkNumber(2);
            var arg1 = (float)L.ChkNumber(3);
            var arg2 = (float)L.ChkNumber(4);
            Vector4 obj = new Vector4(arg0, arg1, arg2);
            L.PushX(obj);
            return 1;
        } else if (count == 4) {
            var arg0 = (float)L.ChkNumber(2);
            var arg1 = (float)L.ChkNumber(3);
            var arg2 = (float)L.ChkNumber(4);
            var arg3 = (float)L.ChkNumber(5);
            Vector4 obj = new Vector4(arg0, arg1, arg2, arg3);
            L.PushX(obj);
            return 1;
        } else if (count == 0) {
            Vector4 obj = new Vector4();
            L.PushX(obj);
            return 1;
        } else {
            LuaDLL.luaL_error(L, "invalid arguments to method: Vector4.New");
        }

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_kEpsilon(ILuaState L)
    {
        L.PushNumber(Vector4.kEpsilon);
        return 1;
    }
    
    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_normalized(ILuaState L)
    {
        object o = L.ToUserData(1);

        if (o == null) {
            LuaTypes types = L.Type(1);

            if (types == LuaTypes.LUA_TTABLE) {
                LuaDLL.luaL_error(L, "unknown member name normalized");
            } else {
                LuaDLL.luaL_error(L, "attempt to index normalized on a nil value");
            }
        }

        Vector4 obj = (Vector4)o;
        L.PushX(obj.normalized);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_magnitude(ILuaState L)
    {
        object o = L.ToUserData(1);

        if (o == null) {
            LuaTypes types = L.Type(1);

            if (types == LuaTypes.LUA_TTABLE) {
                LuaDLL.luaL_error(L, "unknown member name magnitude");
            } else {
                LuaDLL.luaL_error(L, "attempt to index magnitude on a nil value");
            }
        }

        Vector4 obj = (Vector4)o;
        L.PushNumber(obj.magnitude);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_sqrMagnitude(ILuaState L)
    {
        object o = L.ToUserData(1);

        if (o == null) {
            LuaTypes types = L.Type(1);

            if (types == LuaTypes.LUA_TTABLE) {
                LuaDLL.luaL_error(L, "unknown member name sqrMagnitude");
            } else {
                LuaDLL.luaL_error(L, "attempt to index sqrMagnitude on a nil value");
            }
        }

        Vector4 obj = (Vector4)o;
        L.PushNumber(obj.sqrMagnitude);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_zero(ILuaState L)
    {
        L.PushX(Vector4.zero);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_one(ILuaState L)
    {
        L.PushX(Vector4.one);
        return 1;
    }
    
    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Set(ILuaState L)
    {
        L.ChkArgsCount(5);
        Vector4 obj = L.ToVector4(1);
        var arg0 = (float)L.ChkNumber(2);
        var arg1 = (float)L.ChkNumber(3);
        var arg2 = (float)L.ChkNumber(4);
        var arg3 = (float)L.ChkNumber(5);
        obj.Set(arg0, arg1, arg2, arg3);
        Update(L, 1, obj);
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lerp(ILuaState L)
    {
        L.ChkArgsCount(3);
        var arg0 = L.ToVector4(1);
        var arg1 = L.ToVector4(2);
        var arg2 = (float)L.ChkNumber(3);
        Vector4 o = Vector4.Lerp(arg0, arg1, arg2);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int MoveTowards(ILuaState L)
    {
        L.ChkArgsCount(3);
        var arg0 = L.ToVector4(1);
        var arg1 = L.ToVector4(2);
        var arg2 = (float)L.ChkNumber(3);
        Vector4 o = Vector4.MoveTowards(arg0, arg1, arg2);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Scale(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector4 obj =  L.ToVector4(1);
        var arg0 = L.ToVector4(2);
        obj.Scale(arg0);
        Update(L, 1, obj);
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int GetHashCode(ILuaState L)
    {
        L.ChkArgsCount(1);
        Vector4 obj = L.ToVector4(1);
        int o = obj.GetHashCode();
        L.PushInteger(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Equals(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector4 obj = (Vector4)L.ToAnyObject(1);
        var arg0 = L.ToAnyObject(2);
        bool o = obj.Equals(arg0);
        L.PushBoolean(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Normalize(ILuaState L)
    {
        L.ChkArgsCount(1);
        Vector4 obj = L.ToVector4(1);
        obj.Normalize();
        Update(L, 1, obj);
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Dot(ILuaState L)
    {
        L.ChkArgsCount(2);
        var arg0 = L.ToVector4(1);
        var arg1 = L.ToVector4(2);
        float o = Vector4.Dot(arg0, arg1);
        L.PushNumber(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Project(ILuaState L)
    {
        L.ChkArgsCount(2);
        var arg0 = L.ToVector4(1);
        var arg1 = L.ToVector4(2);
        Vector4 o = Vector4.Project(arg0, arg1);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Distance(ILuaState L)
    {
        L.ChkArgsCount(2);
        var arg0 = L.ToVector4(1);
        var arg1 = L.ToVector4(2);
        float o = Vector4.Distance(arg0, arg1);
        L.PushNumber(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Magnitude(ILuaState L)
    {
        L.ChkArgsCount(1);
        var arg0 = L.ToVector4(1);
        float o = Vector4.Magnitude(arg0);
        L.PushNumber(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int SqrMagnitude(ILuaState L)
    {
        L.ChkArgsCount(1);
        Vector4 obj =  L.ToVector4(1);
        float o = obj.SqrMagnitude();
        L.PushNumber(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Min(ILuaState L)
    {
        L.ChkArgsCount(2);
        var arg0 = L.ToVector4(1);
        var arg1 = L.ToVector4(2);
        Vector4 o = Vector4.Min(arg0, arg1);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Max(ILuaState L)
    {
        L.ChkArgsCount(2);
        var arg0 = L.ToVector4(1);
        var arg1 = L.ToVector4(2);
        Vector4 o = Vector4.Max(arg0, arg1);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Add(ILuaState L)
    {
        L.ChkArgsCount(2);
        var arg0 = L.ToVector4(1);
        var arg1 = L.ToVector4(2);
        Vector4 o = arg0 + arg1;
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Sub(ILuaState L)
    {
        L.ChkArgsCount(2);
        var arg0 = L.ToVector4(1);
        var arg1 = L.ToVector4(2);
        Vector4 o = arg0 - arg1;
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Neg(ILuaState L)
    {
        L.ChkArgsCount(1);
        var arg0 = L.ToVector4(1);
        Vector4 o = -arg0;
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Mul(ILuaState L)
    {
        int count = LuaDLL.lua_gettop(L);

        if (count == 2 && L.IsNumber(1) && L.IsVector4(2)) {
            var arg0 = (float)L.ToNumber(1);
            var arg1 = L.ToVector4(2);
            Vector4 o = arg0 * arg1;
            L.PushX(o);
            return 1;
        }
        
        if (count == 2 && L.IsVector4(1) && L.IsNumber(2)) {
            var arg0 = L.ToVector4(1);
            var arg1 = (float)L.ToNumber(2);
            Vector4 o = arg0 * arg1;
            L.PushX(o);
            return 1;
        }
            
        LuaDLL.luaL_error(L, "invalid arguments to method: Vector4.op_Multiply");

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Div(ILuaState L)
    {
        L.ChkArgsCount(2);
        var arg0 = L.ToVector4(1);
        var arg1 = (float)L.ChkNumber(2);
        Vector4 o = arg0 / arg1;
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Eq(ILuaState L)
    {
        L.ChkArgsCount(2);
        var arg0 = L.ToVector4(1);
        var arg1 = L.ToVector4(2);
        bool o = arg0 == arg1;
        L.PushBoolean(o);
        return 1;
    }
}
//}

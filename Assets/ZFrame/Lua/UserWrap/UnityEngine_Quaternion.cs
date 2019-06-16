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
public static class UnityEngine_Quaternion
{
    public const string CLASS = "UnityEngine.Quaternion";

    public static void PushX(this ILuaState self, Quaternion value)
    {
        self.CreateTable(0, 4);
        self.SetDict("x", value.x);
        self.SetDict("y", value.y);
        self.SetDict("z", value.z);
        self.SetDict("w", value.w);
        self.L_GetMetaTable(CLASS);
        self.SetMetaTable(-2);
    }

    public static Quaternion ToQuaternion(this ILuaState self, int index)
    {
        var luaT = self.Type(index);
        switch (luaT) {
            case LuaTypes.LUA_TTABLE:
                float x = self.GetNumber(index, "x");
                float y = self.GetNumber(index, "y");
                float z = self.GetNumber(index, "z");
                float w = self.GetNumber(index, "w");
                return new Quaternion(x, y, z, w);
            case LuaTypes.LUA_TNIL:
                return Quaternion.identity;
            default:
                LogMgr.W("Can't convert a {0} to a UnityEngine.Quaternion. Fallback to Quaternion.identity.", luaT);
                goto case LuaTypes.LUA_TNIL;
        }
    }

    public static bool IsQuaternion(this ILuaState self, int index)
    {
        return self.IsClass(index, CLASS);
    }

    public static void Update(ILuaState L, int index, Quaternion q)
    {
        L.SetNumber(index, "x", q.x);
        L.SetNumber(index, "y", q.y);
        L.SetNumber(index, "z", q.z);
        L.SetNumber(index, "w", q.w);
    }

    public static void Wrap(ILuaState L)
    {
        L.WrapRegister(typeof(Quaternion), _CreateQuaternion, new LuaMethod[] {
            new LuaMethod("Set", Set),
            new LuaMethod("SetLookRotation", SetLookRotation),
            new LuaMethod("SetFromToRotation", SetFromToRotation),
            new LuaMethod("ToAngleAxis", ToAngleAxis),
        }, new LuaField[] {
            new LuaField("eulerAngles", get_eulerAngles, set_eulerAngles),
        }, new LuaMethod[] {
            new LuaMethod("Lerp", Lerp),
            new LuaMethod("LerpUnclamped", LerpUnclamped),
            new LuaMethod("LookRotation", LookRotation),
            new LuaMethod("FromToRotation", FromToRotation),
            new LuaMethod("AngleAxis", AngleAxis),
            new LuaMethod("Slerp", Slerp),
            new LuaMethod("SlerpUnclamped", SlerpUnclamped),
            new LuaMethod("LerpUnclamped", LerpUnclamped),
            new LuaMethod("RotateTowards", RotateTowards),
            new LuaMethod("Inverse", Inverse),
            new LuaMethod("Angle", Angle),
            new LuaMethod("Euler", Euler),
            new LuaMethod("Dot", Dot),
        }, new LuaField[] {
            new LuaField("kEpsilon", get_kEpsilon, null),
            new LuaField("identity", get_identity, null),
        }, new LuaMethod[] {
            new LuaMethod("__mul", Lua_Mul),
            new LuaMethod("__eq", Lua_Eq),
            new LuaMethod("__tostring", __tostring),
        });
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int __tostring(ILuaState L)
    {
        L.PushString(L.ToQuaternion(1).ToString());
        return 1;
    }


    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int _CreateQuaternion(ILuaState L)
    {
        int count = L.GetTop() - 1;

        if (count == 4) {
            var arg0 = (float)L.ChkNumber(2);
            var arg1 = (float)L.ChkNumber(3);
            var arg2 = (float)L.ChkNumber(4);
            var arg3 = (float)L.ChkNumber(5);
            Quaternion obj = new Quaternion(arg0, arg1, arg2, arg3);
            L.PushX(obj);
            return 1;
        } else if (count == 0) {
            Quaternion obj = new Quaternion();
            L.PushX(obj);
            return 1;
        } else {
            LuaDLL.luaL_error(L, "invalid arguments to method: Quaternion.New");
        }

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_kEpsilon(ILuaState L)
    {
        L.PushNumber(Quaternion.kEpsilon);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_identity(ILuaState L)
    {
        L.PushX(Quaternion.identity);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_eulerAngles(ILuaState L)
    {
        L.PushX(L.ToQuaternion(1).eulerAngles);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int set_eulerAngles(ILuaState L)
    {
        Quaternion obj = L.ToQuaternion(1);
        obj.eulerAngles = L.ToVector3(3);
        Update(L, 1, obj);
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Set(ILuaState L)
    {
        L.ChkArgsCount(5);
        float x = L.ToSingle(2), y = L.ToSingle(3), z = L.ToSingle(4), w = L.ToSingle(5);
        var obj = new Quaternion(x, y, z, w);
        Update(L, 1, obj);
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Dot(ILuaState L)
    {
        L.ChkArgsCount(2);
        Quaternion arg0 = L.ToQuaternion(1);
        Quaternion arg1 = L.ToQuaternion(2);
        float o = Quaternion.Dot(arg0, arg1);
        L.PushNumber(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int AngleAxis(ILuaState L)
    {
        L.ChkArgsCount(2);
        var arg0 = (float)L.ChkNumber(1);
        Vector3 arg1 = L.ToVector3(2);
        Quaternion o = Quaternion.AngleAxis(arg0, arg1);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int ToAngleAxis(ILuaState L)
    {
        L.ChkArgsCount(3);
        Quaternion obj = L.ToQuaternion(1);
        float arg0;
        Vector3 arg1;
        obj.ToAngleAxis(out arg0, out arg1);
        L.PushNumber(arg0);
        L.PushX(arg1);
        return 2;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int FromToRotation(ILuaState L)
    {
        L.ChkArgsCount(2);
        Vector3 arg0 = L.ToVector3(1);
        Vector3 arg1 = L.ToVector3(2);
        Quaternion o = Quaternion.FromToRotation(arg0, arg1);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int SetFromToRotation(ILuaState L)
    {
        L.ChkArgsCount(3);
        Quaternion obj = L.ToQuaternion(1);
        Vector3 arg0 = L.ToVector3(2);
        Vector3 arg1 = L.ToVector3(3);
        obj.SetFromToRotation(arg0, arg1);
        Update(L, 1, obj);
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int LookRotation(ILuaState L)
    {
        int count = LuaDLL.lua_gettop(L);

        if (count == 1) {
            Vector3 arg0 = L.ToVector3(1);
            Quaternion o = Quaternion.LookRotation(arg0);
            L.PushX(o);
            return 1;
        } else if (count == 2) {
            Vector3 arg0 = L.ToVector3(1);
            Vector3 arg1 = L.ToVector3(2);
            Quaternion o = Quaternion.LookRotation(arg0, arg1);
            L.PushX(o);
            return 1;
        } else {
            LuaDLL.luaL_error(L, "invalid arguments to method: Quaternion.LookRotation");
        }

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int SetLookRotation(ILuaState L)
    {
        int count = LuaDLL.lua_gettop(L);

        if (count == 2) {
            Quaternion obj = L.ToQuaternion(1);
            Vector3 arg0 = L.ToVector3(2);
            obj.SetLookRotation(arg0);
            Update(L, 1, obj);
            return 0;
        } else if (count == 3) {
            Quaternion obj = L.ToQuaternion(1);
            Vector3 arg0 = L.ToVector3(2);
            Vector3 arg1 = L.ToVector3(3);
            obj.SetLookRotation(arg0, arg1);
            Update(L, 1, obj);
            return 0;
        } else {
            LuaDLL.luaL_error(L, "invalid arguments to method: Quaternion.SetLookRotation");
        }

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Slerp(ILuaState L)
    {
        L.ChkArgsCount(3);
        Quaternion arg0 = L.ToQuaternion(1);
        Quaternion arg1 = L.ToQuaternion(2);
        var arg2 = (float)L.ChkNumber(3);
        Quaternion o = Quaternion.Slerp(arg0, arg1, arg2);
        L.PushX(o);
        return 1;
    }
    
    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int SlerpUnclamped(ILuaState L)
    {
        L.ChkArgsCount(3);
        Quaternion arg0 = L.ToQuaternion(1);
        Quaternion arg1 = L.ToQuaternion(2);
        var arg2 = (float)L.ChkNumber(3);
        Quaternion o = Quaternion.SlerpUnclamped(arg0, arg1, arg2);
        L.PushX(o);
        return 1;
    }
    
    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int LerpUnclamped(ILuaState L)
    {
        L.ChkArgsCount(3);
        Quaternion arg0 = L.ToQuaternion(1);
        Quaternion arg1 = L.ToQuaternion(2);
        var arg2 = (float)L.ChkNumber(3);
        Quaternion o = Quaternion.LerpUnclamped(arg0, arg1, arg2);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lerp(ILuaState L)
    {
        L.ChkArgsCount(3);
        Quaternion arg0 = L.ToQuaternion(1);
        Quaternion arg1 = L.ToQuaternion(2);
        var arg2 = (float)L.ChkNumber(3);
        Quaternion o = Quaternion.Lerp(arg0, arg1, arg2);
        L.PushX(o);
        return 1;
    }
    
    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int RotateTowards(ILuaState L)
    {
        L.ChkArgsCount(3);
        Quaternion arg0 = L.ToQuaternion(1);
        Quaternion arg1 = L.ToQuaternion(2);
        var arg2 = (float)L.ChkNumber(3);
        Quaternion o = Quaternion.RotateTowards(arg0, arg1, arg2);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Inverse(ILuaState L)
    {
        L.ChkArgsCount(1);
        Quaternion arg0 = L.ToQuaternion(1);
        Quaternion o = Quaternion.Inverse(arg0);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Angle(ILuaState L)
    {
        L.ChkArgsCount(2);
        Quaternion arg0 = L.ToQuaternion(1);
        Quaternion arg1 = L.ToQuaternion(2);
        float o = Quaternion.Angle(arg0, arg1);
        L.PushNumber(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Euler(ILuaState L)
    {
        int count = LuaDLL.lua_gettop(L);

        if (count == 1) {
            Vector3 arg0 = L.ToVector3(1);
            Quaternion o = Quaternion.Euler(arg0);
            L.PushX(o);
            return 1;
        } else if (count == 3) {
            var arg0 = (float)L.ChkNumber(1);
            var arg1 = (float)L.ChkNumber(2);
            var arg2 = (float)L.ChkNumber(3);
            Quaternion o = Quaternion.Euler(arg0, arg1, arg2);
            L.PushX(o);
            return 1;
        } else {
            LuaDLL.luaL_error(L, "invalid arguments to method: Quaternion.Euler");
        }

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Mul(ILuaState L)
    {
        int count = LuaDLL.lua_gettop(L);

        if (count == 2 && L.IsQuaternion(1) && L.IsVector3(2)) {
            Quaternion arg0 = L.ToQuaternion(1);
            Vector3 arg1 = L.ToVector3(2);
            Vector3 o = arg0 * arg1;
            L.PushX(o);
            return 1;
        } 
        
        if (count == 2 && L.IsQuaternion(1) && L.IsQuaternion(2)) {
            Quaternion arg0 = L.ToQuaternion(1);
            Quaternion arg1 = L.ToQuaternion(2);
            Quaternion o = arg0 * arg1;
            L.PushX(o);
            return 1;
        } 
        
        LuaDLL.luaL_error(L, "invalid arguments to method: Quaternion.op_Multiply");

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Eq(ILuaState L)
    {
        L.ChkArgsCount(2);
        Quaternion arg0 = L.ToQuaternion(1);
        Quaternion arg1 = L.ToQuaternion(2);
        bool o = arg0 == arg1;
        L.PushBoolean(o);
        return 1;
    }
}
//}

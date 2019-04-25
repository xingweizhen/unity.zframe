using System;
using LuaInterface;

public class System_Object
{
    public static void Wrap(IntPtr L)
    {
        Register(L);
        L.SetMetaTable(-2);
        L.Pop(1);
    }

    private static System.Type Register(IntPtr L)
    {
        LuaMethod[] regs = new LuaMethod[]
        {
            new LuaMethod("Equals", Equals),
            new LuaMethod("GetHashCode", GetHashCode),
            new LuaMethod("GetType", GetType),
            new LuaMethod("ToString", ToString),
            new LuaMethod("ReferenceEquals", ReferenceEquals),
            new LuaMethod("Destroy", Destroy),
            new LuaMethod("new", _CreateSystem_Object),
            new LuaMethod("GetType", GetClassType),
        };

        LuaField[] fields = new LuaField[]
        {
        };

        var type = typeof(object);
        L.WrapCSharpObject(type, regs, fields);
        return type;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int _CreateSystem_Object(IntPtr L)
    {
        int count = L.GetTop();

        if (count == 0) {
            object obj = new object();
            L.PushAnyObject(obj);
            return 1;
        } else {
            LuaDLL.luaL_error(L, "invalid arguments to method: object.New");
        }

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int GetClassType(IntPtr L)
    {
        int count = L.GetTop();
        if (count == 0) {
            L.PushLightUserData(typeof(object));
            return 1;
        } else {
            return MetaMethods.GetType(L);
        }
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Equals(IntPtr L)
    {
        L.ChkArgsCount(2);
        object obj = L.ToAnyObject(1) as object;
        object arg0 = L.ToAnyObject(2);
        bool o = obj != null ? obj.Equals(arg0) : arg0 == null;
        L.PushBoolean(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int GetHashCode(IntPtr L)
    {
        L.ChkArgsCount(1);
        object obj = (object)L.ChkUserDataSelf(1, "object");
        int o = obj.GetHashCode();
        L.PushInteger(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int GetType(IntPtr L)
    {
        L.ChkArgsCount(1);
        object obj = (object)L.ChkUserDataSelf(1, "object");
        Type o = obj.GetType();
        L.PushLightUserData(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int ToString(IntPtr L)
    {
        L.ChkArgsCount(1);
        object obj = (object)L.ChkUserDataSelf(1, "object");
        string o = obj.ToString();
        L.PushString(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int ReferenceEquals(IntPtr L)
    {
        L.ChkArgsCount(2);
        object arg0 = L.ToAnyObject(1);
        object arg1 = L.ToAnyObject(2);
        bool o = object.ReferenceEquals(arg0, arg1);
        L.PushBoolean(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Destroy(IntPtr L)
    {
        MetaMethods.GC(L);
        return 0;
    }
}


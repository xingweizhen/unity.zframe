using UnityEngine;
using System.Collections;
using LuaInterface;
using ILuaState = System.IntPtr;

public static class System_MonoType
{
    public static void Wrap(ILuaState L)
    {
        // 没有类表，只有元表
        L.L_NewMetaTable(WrapToLua.GetMetaName("System.MonoType"));

        // 继承自System.Object
        L.L_GetMetaTable("System.Object");
        L.SetMetaTable(-2);

        L.SetToLuaIndex();
        L.SetToLuaNewIndex();
        L.SetDict("__gc", MetaMethods.GC);
        L.SetDict("__tostring", MetaMethods.LToString);

        L.RegistMembers(new LuaMethod[] {
            new LuaMethod("IsSubclassOf", IsSubclassOf),
            new LuaMethod("IsAssignableFrom", IsAssignableFrom),
        }, new LuaField[] {
            new LuaField("Name", get_Name, null),
            new LuaField("FullName", get_FullName, null),
        });

        L.Pop(1);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int get_Name(ILuaState L)
    {
        var type = L.ChkTypeObject(1);
        L.PushString(type.Name);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int get_FullName(ILuaState L)
    {
        var type = L.ChkTypeObject(1);
        L.PushString(type.FullName);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsSubclassOf(ILuaState L)
    {
        var type = L.ChkTypeObject(1);
        var baseType = L.ChkTypeObject(2);
        L.PushBoolean(type.IsSubclassOf(baseType));
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsAssignableFrom(ILuaState L)
    {
        var type = L.ChkTypeObject(1);
        var baseType = L.ChkTypeObject(2);
        L.PushBoolean(type.IsAssignableFrom(baseType));
        return 1;
    }
}

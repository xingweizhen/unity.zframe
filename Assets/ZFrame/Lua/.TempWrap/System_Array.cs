using UnityEngine;
using System.Collections;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif
using ILuaState = System.IntPtr;

public static class System_Array
{
    public const string CLASS = "System.Array";

    public static void PushUData(this ILuaState self, System.Array value)
    {
        var ls = LuaEnv.Get(self).ls;
        ls.translator.pushObject(self, value, CLASS);
    }

    public static void Wrap(ILuaState L)
    {
        // 没有类表，只有元表
        L.L_NewMetaTable(WrapToLua.GetMetaName(CLASS));

        // 继承自System.Object
        L.L_GetMetaTable("System.Object");
        L.SetMetaTable(-2);

        L.SetDict("__gc", MetaMethods.GC);
        L.SetDict("__tostring", MetaMethods.LToString);
        L.SetDict("__index", new LuaCSFunction(__index_Array));
        L.SetDict("__newindex", new LuaCSFunction(__newindex_Array));

        L.RegistMembers(null, new LuaField[] {
            new LuaField("Length", get_Length, null),
        });
                
        L.Pop(1);
    }


    /// <summary>
    /// System.Array的__index元方法
    /// </summary>
    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    private static int __index_Array(ILuaState L)
    {
        LuaTypes luaType = L.Type(2);

        if (luaType == LuaTypes.LUA_TNUMBER) {
            System.Array obj = L.ChkUserDataSelf(1, CLASS) as System.Array;

            if (obj == null) {
                L.L_Error("trying to index an invalid Array reference");
                return 0;
            }

            int index = L.ToInteger(2);

            if (index >= obj.Length) {
                L.L_Error(string.Format("array index out of bounds: {0}/{1}", index, obj.Length));
                return 0;
            }

            object val = obj.GetValue(index);

            if (val == null) {
                L.L_Error(string.Format("array index {0} is null", index));
                return 0;
            }

            L.PushAnyObject(val);
            return 1;
        } else if (luaType == LuaTypes.LUA_TSTRING) {
            return MetaMethods.Index(L);
        }

        L.L_Error(string.Format("unknown key for Array, got {0}", luaType));
        return 0;
    }


    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    private static int __newindex_Array(ILuaState L)
    {
        System.Array obj = L.ChkUserDataSelf(1, CLASS) as System.Array;

        if (obj == null) {
            L.L_Error("trying to newindex an invalid Array reference");
            return 0;
        }

        int index = L.ChkInteger(2);
        object val = L.ToAnyObject(3);
        var valType = val.GetType();
        System.Type type = obj.GetType().GetElementType();

        if (!type.IsAssignableFrom(valType)) {
            L.L_Error(string.Format("trying to set object type is not correct: {0} expected, got {1}", type, valType));
            return 0;
        }

        val = System.Convert.ChangeType(val, type);
        obj.SetValue(val, index);

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    private static int get_Length(ILuaState L)
    {
        System.Array obj = L.ChkUserDataSelf(1, CLASS) as System.Array;
        L.PushInteger(obj.Length);
        return 1;
    }


}

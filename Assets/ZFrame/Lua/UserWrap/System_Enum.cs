using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
using LuaDLL = XLua.LuaDLL.Lua;
#endif
using ILuaState = System.IntPtr;

public static class System_Enum
{
    public const string CLASS = "System.Enum";

    private class EnumValue
    {
        public readonly int value;
        public readonly string name;
        public EnumValue(int value, string name)
        {
            this.value = value;
            this.name = name;
        }
    }

    private static Dictionary<System.Type, EnumValue[]> s_EnumMap = new Dictionary<System.Type, EnumValue[]>();

    private static int NameToEnumValue(System.Type enumType, string name, int def)
    {
        EnumValue[] enumValues;
        if (!s_EnumMap.TryGetValue(enumType, out enumValues)) {
            var enumArr = System.Enum.GetValues(enumType);
            
            enumValues = new EnumValue[enumArr.Length];
            for (int i = 0; i < enumArr.Length; i++) {
                var eVal = enumArr.GetValue(i);
                enumValues[i] = new EnumValue((int)eVal, eVal.ToString());
            }
        }
        
        for (int i = 0; i < enumValues.Length; i++) {
            if (enumValues[i].name == name) {
                return enumValues[i].value;
            }
        }
        return def;
    }

    public static bool IsEnumValue(this ILuaState self, int index)
    {
        var type = self.Type(index);
        return type == LuaTypes.LUA_TNUMBER || type == LuaTypes.LUA_TSTRING || type == LuaTypes.LUA_TTABLE;
    }

    public static int ToEnumValue(this ILuaState self, int index, System.Type type, int def = 0)
    {
        var luaT = self.Type(index);
        switch (luaT) {
            case LuaTypes.LUA_TNUMBER:
                return self.ToInteger(index);
            case LuaTypes.LUA_TSTRING:
                return NameToEnumValue(type, self.ToString(index), def);
            case LuaTypes.LUA_TTABLE: {
                    self.PushString("id");
                    self.RawGet(index);
                    int id = self.ToInteger(-1);
                    self.Pop(1);
                    return id;
                }
            default:
                break;
        }

        return def;
    }

    public static int ToEnumValue(this ILuaState self, int index, System.Enum def)
    {
        return self.ToEnumValue(index, def.GetType(), System.Convert.ToInt32(def));
    }

    public static void PushX(this ILuaState self, System.Enum value)
    {
        var type = value.GetType();

        // 自动绑定到Lua
        var typeName = type.FullName;
        self.L_GetMetaTable(typeName);
        if (self.IsNil(-1)) {
            self.Pop(1);
            Wrap(self, type);
        };
        
        var name = System.Enum.GetName(type, value);
        self.GetField(-1, name);
        self.Remove(-2);
    }

    private static void Wrap(ILuaState L, System.Type enumType)
    {
        L.L_NewMetaTable(enumType.FullName);
        //L.L_GetMetaTable("System.Object");
        //L.SetMetaTable(-2);

        LuaDLL.lua_pushlightuserdata(L, LuaDLL.xlua_tag());
        L.PushNumber(1);
        L.RawSet(-3);

        L.SetDict(TableAPI.META_CLASS, CLASS);
        L.SetDict("__tostring", new LuaCSFunction(__tostring));
        L.SetDict("__index", new LuaCSFunction(__index));
        L.SetDict("__newindex", new LuaCSFunction(__newindex));
        
        // 保存所有枚举值
        var values = System.Enum.GetValues(enumType);
        for (int i = 0; i < values.Length; ++i) {
            var value = values.GetValue(i);
            var id = (int)value;
            var name = value.ToString();
            L.CreateTable(0, 2);
            {
                L.SetDict("id", id);
                L.SetDict("name", name);
            }
            L.PushValue(-2);
            L.SetMetaTable(-2);
            L.SetField(-2, name);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int __index(ILuaState L)
    {
        L.L_Error("trying to index an enum value");
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int __newindex(ILuaState L)
    {
        L.L_Error("trying to newindex an enum value");
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int __tostring(ILuaState L)
    {
        L.PushString("name");
        L.GetTable(1);
        L.GetField(1, "name");
        var name = L.ToString(-1);
        L.Pop(1);
        L.GetField(1, "id");
        var id = L.ToInteger(-1);
        L.Pop(1);

        L.PushString(string.Format("{0}({1})", name, id));
        return 1;
    }
}

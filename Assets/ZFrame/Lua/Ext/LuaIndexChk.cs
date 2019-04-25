using UnityEngine;
using System.Collections;
#if ULUA
using LuaInterface;
#else
using XLua;
#endif
using ILuaState = System.IntPtr;

public static class LuaIndexChk 
{
    public static double ChkNumber(this ILuaState self, int index)
    {
        if (self.IsNumber(index)) {
            return self.ToNumber(index);
        }
        self.TypeError(index, "number");
        return default(double);
    }

    public static string ChkString(this ILuaState self, int index)
    {
        if (self.IsString(index)) {
            return self.ToString(index);
        }
        self.TypeError(index, "string");
        return default(string);
    }

    public static bool ChkBoolean(this ILuaState self, int index)
    {
        if (self.IsBoolean(index)) {
            return self.ToBoolean(index);
        } else {
            return !self.IsNoneOrNil(index);
        }
    }

    public static int ChkInteger(this ILuaState self, int index)
    {
        if (self.IsNumber(index)) {
            return self.ToInteger(index);
        }
        self.TypeError(index, "int");
        return default(int);
    }

    public static object ChkEnumValue(this ILuaState self, int index, System.Type type)
    {
        var obj = self.ToEnumValue(index, type);
        if (!System.Enum.IsDefined(type, obj)) {
            self.L_Error(string.Format("{0} expected, got {1}", type, obj));
        }
        return obj;
    }

    public static object ChkUserData(this ILuaState self, int index, System.Type type)
    {
        if (self.IsNil(index)) return null;

        var luaT = self.Type(index);
        if (luaT != LuaTypes.LUA_TUSERDATA && luaT != LuaTypes.LUA_TLIGHTUSERDATA) {
            self.L_ArgError(index, string.Format("{0} expected, got {1}", type.FullName, luaT));
            return null;
        }

        object obj = self.ToUserData(index);

        if (obj == null) {
            self.L_ArgError(index, string.Format("{0} expected, got nil", type.FullName));
            return null;
        }

        System.Type objType = obj.GetType();

        if (type == objType || type.IsAssignableFrom(objType)) {
            return obj;
        }

        self.L_ArgError(index, string.Format("{0} expected, got {1}", type.FullName, objType.Name));
        return null;
    }
}

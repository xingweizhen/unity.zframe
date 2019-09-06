using UnityEngine;
using System.Collections;
#if ULUA
using LuaInterface;
#else
using XLua;
#endif
using ILuaState = System.IntPtr;

public static class LuaIndexOpt
{
    public static T Opt<T>(this ILuaState self, I2V.Index2Value<T> indexTo, int index, T def)
    {
        if (self.IsNoneOrNil(index)) return def;
        
        T value;
        indexTo(self, index, out value);
        return value;
    }

    public static double OptNumber(this ILuaState self, int index, double def)
    {
        return self.IsNoneOrNil(index) ? def : self.ToNumber(index);
    }

    public static float OptSingle(this ILuaState self, int index, float def)
    {
        return self.IsNoneOrNil(index) ? def : self.ToSingle(index);
    }

    public static string OptString(this ILuaState self, int index, string def)
    {
        return self.IsNoneOrNil(index) ? def : self.ToString(index);
    }

    public static bool OptBoolean(this ILuaState self, int index, bool def)
    {
        return self.IsNoneOrNil(index) ? def : self.ToBoolean(index);
    }

    public static int OptInteger(this ILuaState self, int index, int def)
    {
        return self.IsNoneOrNil(index) ? def : self.ToInteger(index);
    }

    public static long OptLong(this ILuaState self, int index, long def)
    {
        return self.IsNoneOrNil(index) ? def : self.ToLong(index);
    }

    public static int OptEnumValue(this ILuaState self, int index, System.Type type, System.Enum def)
    {
        return self.IsNoneOrNil(index) ? System.Convert.ToInt32(def) : self.ToEnumValue(index, type);
    }

    public static object OptUserData(this ILuaState self, int index, System.Type type, object def)
    {
        return self.IsNoneOrNil(index) ? def : self.ChkUserData(index, type);
    }
}

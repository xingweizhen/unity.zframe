using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILuaState = System.IntPtr;
using TinyJSON;

public static class I2V
{
    public static void AbsIndex(this ILuaState self, ref int index)
    {
        if (index < 0) index = self.GetTop() + 1 + index;
    }

    public delegate void Index2Value<T>(ILuaState lua, int index, out T value);

    public static readonly Index2Value<double> ToNumber = (ILuaState lua, int index, out double value) => value = lua.ToNumber(index);
    public static readonly Index2Value<float> ToSingle = (ILuaState lua, int index, out float value) => value = lua.ToSingle(index);
    public static readonly Index2Value<string> L_ToString = (ILuaState lua, int index, out string value) => value = lua.ToString(index);
    public static readonly Index2Value<int> ToInteger = (ILuaState lua, int index, out int value) => value = lua.ToInteger(index);
    public static readonly Index2Value<long> ToLong = (ILuaState lua, int index, out long value) => value = lua.ToLong(index);
    public static readonly Index2Value<bool> ToBoolean = (ILuaState lua, int index, out bool value) => value = lua.ToBoolean(index);
    public static readonly Index2Value<Color> ToColor = (ILuaState lua, int index, out Color value) => value = lua.ToColor(index);
    public static readonly Index2Value<Vector2> ToVector2 = (ILuaState lua, int index, out Vector2 value) => value = lua.ToVector2(index);
    public static readonly Index2Value<Vector3> ToVector3 = (ILuaState lua, int index, out Vector3 value) => value = lua.ToVector3(index);
    public static readonly Index2Value<Vector4> ToVector4 = (ILuaState lua, int index, out Vector4 value) => value = lua.ToVector4(index);
    public static readonly Index2Value<Quaternion> ToQuaternion = (ILuaState lua, int index, out Quaternion value) => value = lua.ToQuaternion(index);
    public static readonly Index2Value<Bounds> ToBounds = (ILuaState lua, int index, out Bounds value) => value = lua.ToBounds(index);
    public static readonly Index2Value<Variant> ToJsonObj = (ILuaState lua, int index, out Variant value) => value = lua.ToJsonObj(index);
    public static readonly Index2Value<GameObject> ToGameObject = (ILuaState lua, int index, out GameObject value) => value = lua.ToGameObject(index);

    public delegate void Value2Top<in T>(ILuaState lua, T value);
    public static readonly Value2Top<Vector2> PushVector2 = (lua, value) => lua.PushX(value);
    public static readonly Value2Top<Vector3> PushVector3 = (lua, value) => lua.PushX(value);
    public static readonly Value2Top<Variant> PushJson = (lua, value) => lua.PushX(value);


}

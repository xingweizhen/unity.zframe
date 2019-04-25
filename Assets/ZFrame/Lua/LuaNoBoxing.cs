using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrame;
using ILuaState = System.IntPtr;

//public abstract class Data2Lua<T, V> : NoBoxingValue<T, V>, IDataToLua where T : Data2Lua<T, V>, new()
//{   
//    public abstract void Push(ILuaState lua);
//}

public class BoolToLua : NoBoxingBool, IDataToLua
{   
    public void Push(ILuaState lua)
    {
        lua.PushBoolean(m_Value);
    }
}

public class IntToLua : NoBoxingInt, IDataToLua //Data2Lua<IntToLua, int>
{   
    public void Push(ILuaState lua)
    {
        lua.PushInteger(m_Value);
    }
}

public class FloatToLua : NoBoxingFloat, IDataToLua //Data2Lua<FloatToLua, float>
{
    public void Push(ILuaState lua)
    {
        lua.PushNumber(m_Value);
    }
}

public class Vector2ToLua : NoBoxingVector2, IDataToLua //Data2Lua<Vector2ToLua, Vector2>
{
    public void Push(ILuaState lua)
    {
        lua.PushX(m_Value);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrame;
using ILuaState = System.IntPtr;

public class BoolToLua : NoBoxingValue<bool>, IDataToLua
{   
    public void Push(ILuaState lua)
    {
        lua.PushBoolean(m_Value);
    }
}

public class IntToLua : NoBoxingValue<int>, IDataToLua
{   
    public void Push(ILuaState lua)
    {
        lua.PushInteger(m_Value);
    }
}

public class FloatToLua : NoBoxingValue<float>, IDataToLua
{
    public void Push(ILuaState lua)
    {
        lua.PushNumber(m_Value);
    }
}

public class Vector2ToLua : NoBoxingValue<Vector2>, IDataToLua
{
    public void Push(ILuaState lua)
    {
        lua.PushX(m_Value);
    }
}

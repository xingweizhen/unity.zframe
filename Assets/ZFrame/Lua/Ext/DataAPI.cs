using System.Collections;
using System.Collections.Generic;
#if ULUA
using LuaInterface;
#else
using XLua;
#endif
using ILuaState = System.IntPtr;

public interface IDataFromLua
{
    void InitFromLua(ILuaState lua, int index);
}

public static class DataAPI
{
    public static void ToData<T>(this ILuaState self, int index, out T Data) where T : IDataFromLua, new()
    {
        Data = new T();
        if (self.IsTable(index)) {
            Data.InitFromLua(self, index);
        }
    }

    public static void GetDataValue(this ILuaState self, int index, string key, IDataFromLua Data)
    {
        self.GetField(index, key);
        Data.InitFromLua(self, -1);
        self.Pop(1);
    }

    public static T GetDataValue<T>(this ILuaState self, int index, string key) where T : IDataFromLua, new()
    {
        var Data = new T();
        self.GetField(index, key);
        Data.InitFromLua(self, -1);
        self.Pop(1);

        return Data;
    }
}

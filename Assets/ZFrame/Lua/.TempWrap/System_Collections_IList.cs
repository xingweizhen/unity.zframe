using System.Collections;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif
using ILuaState = System.IntPtr;

public static class System_Collections_IList
{
    private const string META_TABLE = "System.Collections.IList";

    public static void PushUData(this ILuaState self, IList value)
    {
        var ls = LuaEnv.Get(self).ls;
        ls.translator.pushObject(self, value, META_TABLE);
    }

    public static void Wrap(ILuaState L)
    {
        L.L_NewMetaTable(META_TABLE);

        L.L_GetMetaTable("System.Object");
        L.SetMetaTable(-2);

        L.SetDict("__gc", MetaMethods.GC);
        L.SetDict("__tostring", MetaMethods.LToString);
        L.SetDict("__index", new LuaCSFunction(__index_IList));
        L.SetDict("__newindex", new LuaCSFunction(__newindex_IList));

        L.RegistMembers(new LuaMethod[]
            {
                new LuaMethod("Add", Add),
                new LuaMethod("Clear", Clear),
                new LuaMethod("Contains", Contains),
                new LuaMethod("IndexOf", IndexOf),
                new LuaMethod("Insert", Insert),
                new LuaMethod("Remove", Remove),
                new LuaMethod("RemoveAt", RemoveAt),

                new LuaMethod("AddRange", AddRange),
            },
            new LuaField[]
            {
                new LuaField("Count", get_Count, null),
                new LuaField("IsFixedSize", get_IsFixedSize, null),
                new LuaField("IsReadOnly", get_IsReadOnly, null),                
            }
        );

        L.Pop(1);
    }

    private static object GetElement(object obj)
    {
        if (obj is LuaFunction) {
            var func = obj as LuaFunction;
            obj = new System.Action(() => {
                int top = func.BeginPCall();
                func.PCall(top, 0);
                func.EndPCall(top);
            });
        }
        return obj;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    private static int __index_IList(ILuaState L)
    {
        LuaTypes luaType = L.Type(2);

        if (luaType == LuaTypes.LUA_TNUMBER) {
            IList obj = L.ChkUserDataSelf(1, META_TABLE) as IList;

            if (obj == null) {
                L.L_Error("trying to index an invalid IList reference");
                return 0;
            }

            int index = L.ToInteger(2);

            if (index >= obj.Count) {
                L.L_Error(string.Format("index out of bounds: {0}/{1}", index, obj.Count));
                return 0;
            }

            object val = obj[index];

            if (val == null) {
                L.L_Error(string.Format("index {0} is null", index));
                return 0;
            }

            L.PushAnyObject(val);
            return 1;
        } else if (luaType == LuaTypes.LUA_TSTRING) {
            return MetaMethods.Index(L);
        }

        L.L_Error(string.Format("unknown key for IList, got {0}", luaType));
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    private static int __newindex_IList(ILuaState L)
    {
        IList obj = L.ChkUserDataSelf(1, META_TABLE) as IList;

        if (obj == null) {
            L.L_Error("trying to newindex an invalid IList reference");
            return 0;
        }

        int index = L.ChkInteger(2);
        object val = L.ToAnyObject(3);

        if (index >= obj.Count) {
            L.L_Error(string.Format("index out of bounds: {0}/{1}", index, obj.Count));
            return 0;
        }

        obj[index] = val;

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_Count(ILuaState L)
    {
        object o = L.ToUserData(1);
        IList obj = (IList)o;

        if (obj == null) {
            LuaTypes types = L.Type(1);

            if (types == LuaTypes.LUA_TTABLE) {
                LuaDLL.luaL_error(L, "unknown member name IsFixedSize");
            } else {
                LuaDLL.luaL_error(L, "attempt to index IsFixedSize on a nil value");
            }
        }

        L.PushInteger(obj.Count);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_IsFixedSize(ILuaState L)
    {
        object o = L.ToUserData(1);
        IList obj = (IList)o;

        if (obj == null) {
            LuaTypes types = L.Type(1);

            if (types == LuaTypes.LUA_TTABLE) {
                LuaDLL.luaL_error(L, "unknown member name IsFixedSize");
            } else {
                LuaDLL.luaL_error(L, "attempt to index IsFixedSize on a nil value");
            }
        }

        L.PushBoolean(obj.IsFixedSize);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_IsReadOnly(ILuaState L)
    {
        object o = L.ToUserData(1);
        IList obj = (IList)o;

        if (obj == null) {
            LuaTypes types = L.Type(1);

            if (types == LuaTypes.LUA_TTABLE) {
                LuaDLL.luaL_error(L, "unknown member name IsReadOnly");
            } else {
                LuaDLL.luaL_error(L, "attempt to index IsReadOnly on a nil value");
            }
        }

        L.PushBoolean(obj.IsReadOnly);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Add(ILuaState L)
    {
        L.ChkArgsCount(2);
        IList obj = (IList)L.ChkUserDataSelf(1, "IList");
        object arg0 = L.ToAnyObject(2);
        int o = obj.Add(GetElement(arg0));
        L.PushInteger(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Clear(ILuaState L)
    {
        L.ChkArgsCount(1);
        IList obj = (IList)L.ChkUserDataSelf(1, "IList");
        obj.Clear();
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Contains(ILuaState L)
    {
        L.ChkArgsCount(2);
        IList obj = (IList)L.ChkUserDataSelf(1, "IList");
        object arg0 = L.ToAnyObject(2);
        bool o = obj.Contains(arg0);
        L.PushBoolean(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int IndexOf(ILuaState L)
    {
        L.ChkArgsCount(2);
        IList obj = (IList)L.ChkUserDataSelf(1, "IList");
        object arg0 = L.ToAnyObject(2);
        int o = obj.IndexOf(arg0);
        L.PushInteger(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Insert(ILuaState L)
    {
        L.ChkArgsCount(3);
        IList obj = (IList)L.ChkUserDataSelf(1, "IList");
        var arg0 = (int)L.ChkNumber(2);
        object arg1 = L.ToAnyObject(3);
        obj.Insert(arg0, GetElement(arg1));
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Remove(ILuaState L)
    {
        L.ChkArgsCount(2);
        IList obj = (IList)L.ChkUserDataSelf(1, "IList");
        object arg0 = L.ToAnyObject(2);
        obj.Remove(arg0);
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int RemoveAt(ILuaState L)
    {
        L.ChkArgsCount(2);
        IList obj = (IList)L.ChkUserDataSelf(1, "IList");
        var arg0 = (int)L.ChkNumber(2);
        obj.RemoveAt(arg0);
        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int AddRange(ILuaState L)
    {
        L.ChkArgsCount(2);
        IList obj = (IList)L.ChkUserDataSelf(1, "IList");
        var arg0 = (IEnumerable)L.ChkUserData(2, typeof(IEnumerable));
        var itor = arg0.GetEnumerator();
        while (itor.MoveNext()) obj.Add(itor.Current);
        return 0;
    }
}


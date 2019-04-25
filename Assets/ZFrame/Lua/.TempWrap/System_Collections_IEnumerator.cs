using System.Collections;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif
using ILuaState = System.IntPtr;

public static class System_Collections_IEnumerator
{
    private static string s_meta;

    public static void PushUData(this ILuaState self, IEnumerator value)
    {
        var ls = LuaEnv.Get(self).ls;
        ls.translator.pushObject(self, value, s_meta);
    }

    public static void Wrap(ILuaState L)
    {
        var type = Register(L);

        L.L_GetMetaTable("System.Object");
        L.SetMetaTable(-2);

        s_meta = WrapToLua.GetMetaName(type.FullName);
        L.SetMetaTable(-2);
        L.Pop(1);
    }

    private static System.Type Register(ILuaState L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("MoveNext", MoveNext),
			new LuaMethod("Reset", Reset),
			new LuaMethod("new", _CreateIEnumerator),
			new LuaMethod("GetType", GetClassType),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("Current", get_Current, null),
		};

		var type = typeof(IEnumerator);
		L.WrapCSharpObject(type, regs, fields);
		return type;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateIEnumerator(ILuaState L)
	{
		LuaDLL.luaL_error(L, "IEnumerator class does not have a constructor function");
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetClassType(ILuaState L)
	{
		int count = L.GetTop();
		if (count == 0) {
			L.PushLightUserData(typeof(IEnumerator));
			return 1;
		} else {
			return MetaMethods.GetType(L);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_Current(ILuaState L)
	{
		object o = L.ToUserData(1);
		IEnumerator obj = (IEnumerator)o;

		if (obj == null)
		{
			LuaTypes types = L.Type(1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name Current");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index Current on a nil value");
			}
		}

		L.PushAnyObject(obj.Current);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int MoveNext(ILuaState L)
	{
		L.ChkArgsCount(1);
		IEnumerator obj = (IEnumerator)L.ChkUserDataSelf(1, "IEnumerator");
		bool o = obj.MoveNext();
		L.PushBoolean(o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Reset(ILuaState L)
	{
		L.ChkArgsCount(1);
		IEnumerator obj = (IEnumerator)L.ChkUserDataSelf(1, "IEnumerator");
		obj.Reset();
		return 0;
	}
}


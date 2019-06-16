using System;
using UnityEngine;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
using LuaDLL = XLua.LuaDLL.Lua;
#endif
using ILuaState = System.IntPtr;

public static class UnityEngine_Bounds
{
	public const string CLASS = "UnityEngine.Bounds";

    public static void PushX(this ILuaState self, Bounds value)
    {
        self.CreateTable(0, 2);
        self.PushString("center");
        self.PushX(value.center);
        self.RawSet(-3);

        self.PushString("size");
        self.PushX(value.size);
        self.RawSet(-3);

        self.L_GetMetaTable(CLASS);
        self.SetMetaTable(-2);
    }

    public static Bounds ToBounds(this ILuaState self, int index)
    {
        var luaT = self.Type(index);
        switch (luaT) {
            case LuaTypes.LUA_TTABLE:
                self.GetField(index, "center");
                var center = self.ToVector3(index + 1);
                self.Pop(1);

                self.GetField(index, "size");
                var size = self.ToVector3(index + 1);
                self.Pop(1);

                return new Bounds(center, size);
            case LuaTypes.LUA_TNIL:
                return default(Bounds);
            default:
                LogMgr.W("{0} Can't convert a {1} to a UnityEngine.Bounds. Fallback to default(Bounds).",
                    self.DebugCurrentLine(2), luaT);
                goto case LuaTypes.LUA_TNIL;
        }
    }

    public static void Update(ILuaState self, int index, Bounds b)
    {
        self.GetField(index, "center");
        UnityEngine_Vector3.Update(self, -1, b.center);
        self.Pop(1);

        self.GetField(index, "size");
        UnityEngine_Vector3.Update(self, -1, b.size);
        self.Pop(1);
    }

    public static bool IsBounds(this ILuaState self, int index)
    {
        return self.IsClass(index, CLASS);
    }

    public static void Wrap(ILuaState L)
    {
        L.WrapRegister(typeof(Bounds), _CreateBounds, new LuaMethod[] {
            new LuaMethod("Contains", Contains),
            new LuaMethod("SqrDistance", SqrDistance),
            new LuaMethod("IntersectRay", IntersectRay),
            new LuaMethod("ClosestPoint", ClosestPoint),
            new LuaMethod("SetMinMax", SetMinMax),
            new LuaMethod("Encapsulate", Encapsulate),
            new LuaMethod("Expand", Expand),
            new LuaMethod("Intersects", Intersects),
        }, new LuaField[] {
           new LuaField("center", get_center, set_center),
            new LuaField("size", get_size, set_size),
            new LuaField("extents", get_extents, set_extents),
            new LuaField("min", get_min, set_min),
            new LuaField("max", get_max, set_max),
        }, new LuaMethod[] {
          
        }, new LuaField[] {
           
        }, new LuaMethod[] {
            new LuaMethod("__eq", Lua_Eq),
            new LuaMethod("__tostring", __tostring),
        });
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int __tostring(ILuaState L)
    {
        L.PushString(L.ToBounds(1).ToString());
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateBounds(IntPtr L)
	{
		int count = L.GetTop() - 1;

		if (count == 2)
		{
			var arg0 = L.ToVector3(2);
			var arg1 = L.ToVector3(3);
			Bounds obj = new Bounds(arg0,arg1);
            L.PushX(obj);
			return 1;
		}
		else if (count == 0)
		{
			Bounds obj = new Bounds();
            L.PushX(obj);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: Bounds.New");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_center(IntPtr L)
	{
		var obj = L.ToBounds(1);
		L.PushX(obj.center);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_size(IntPtr L)
	{
        var obj = L.ToBounds(1);
        L.PushX(obj.size);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_extents(IntPtr L)
	{
        var obj = L.ToBounds(1);
        L.PushX(obj.extents);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_min(IntPtr L)
	{
        var obj = L.ToBounds(1);
        L.PushX(obj.min);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_max(IntPtr L)
	{
        var obj = L.ToBounds(1);
        L.PushX(obj.max);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_center(IntPtr L)
	{
        var obj = L.ToBounds(1);
        obj.center = L.ToVector3(3);
        Update(L, 1, obj);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_size(IntPtr L)
	{
        var obj = L.ToBounds(1);
        obj.size = L.ToVector3(3);
        Update(L, 1, obj);
        return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_extents(IntPtr L)
	{
        var obj = L.ToBounds(1);
        obj.extents = L.ToVector3(3);
        Update(L, 1, obj);
        return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_min(IntPtr L)
	{
        var obj = L.ToBounds(1);
        obj.min = L.ToVector3(3);
        Update(L, 1, obj);
        return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_max(IntPtr L)
	{
        var obj = L.ToBounds(1);
        obj.max = L.ToVector3(3);
        Update(L, 1, obj);
        return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Contains(IntPtr L)
	{
		L.ChkArgsCount(2);
        var obj = L.ToBounds(1);
        var arg0 = L.ToVector3(2);
		bool o = obj.Contains(arg0);
		L.PushBoolean(o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SqrDistance(IntPtr L)
	{
		L.ChkArgsCount(2);
        var obj = L.ToBounds(1);
        var arg0 = L.ToVector3(2);
		float o = obj.SqrDistance(arg0);
		L.PushNumber(o);
		return 1;
	}

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int IntersectRay(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);

        if (count == 2) {
            var obj = L.ToBounds(1);
            Ray arg0; L.ToTranslator().Get(L, 2, out arg0);
            bool o = obj.IntersectRay(arg0);
            L.PushBoolean(o);
            return 1;
        } else if (count == 3) {
            var obj = L.ToBounds(1);
            Ray arg0; L.ToTranslator().Get(L, 2, out arg0);
            float arg1;
            bool o = obj.IntersectRay(arg0, out arg1);
            L.PushBoolean(o);
            L.PushNumber(arg1);
            return 2;
        } else {
            LuaDLL.luaL_error(L, "invalid arguments to method: Bounds.IntersectRay");
        }

        return 0;
    }

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ClosestPoint(IntPtr L)
	{
		L.ChkArgsCount(2);
        var obj = L.ToBounds(1);
        var arg0 = L.ToVector3(2);
		Vector3 o = obj.ClosestPoint(arg0);
		L.PushX(o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetHashCode(IntPtr L)
	{
		L.ChkArgsCount(1);
        var obj = L.ToBounds(1);
        int o = obj.GetHashCode();
		L.PushInteger(o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Equals(IntPtr L)
	{
		L.ChkArgsCount(2);
        var obj = L.ToBounds(1);
        var arg0 = L.ToAnyObject(2);
		bool o = obj.Equals(arg0);
		L.PushBoolean(o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Lua_Eq(IntPtr L)
	{
		L.ChkArgsCount(2);
		var arg0 = L.ToBounds(1);
		var arg1 = L.ToBounds(2);
		bool o = arg0 == arg1;
		L.PushBoolean(o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetMinMax(IntPtr L)
	{
		L.ChkArgsCount(3);
		Bounds obj = L.ToBounds(1);
		var arg0 = L.ToVector3(2);
		var arg1 = L.ToVector3(3);
		obj.SetMinMax(arg0,arg1);
        Update(L, 1, obj);
        return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Encapsulate(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);
		if (count != 2 || !L.IsBounds(1)) goto INVAILD_ARGUMENTS;

		if (L.IsBounds(1)) {
			if (L.IsBounds(2)) {
				var obj = L.ToBounds(1);
				var arg0 = L.ToBounds(2);
				obj.Encapsulate(arg0);
				Update(L, 1, obj);
				return 0;
			}

			if (L.IsVector3(2)) {
				var obj = L.ToBounds(1);
				var arg0 = L.ToVector3(2);
				obj.Encapsulate(arg0);
				Update(L, 1, obj);
				return 0;
			}
		}

		INVAILD_ARGUMENTS:
		LuaDLL.luaL_error(L, "invalid arguments to method: Bounds.Encapsulate");

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Expand(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);
		if (count != 2 || !L.IsBounds(1)) goto INVAILD_ARGUMENTS;
		
		if (L.IsBounds(1)) {
			if (L.IsVector3(2)) {
				var obj = L.ToBounds(1);
				var arg0 = L.ToVector3(2);
				obj.Expand(arg0);
				Update(L, 1, obj);
				return 0;
			}

			if (L.IsNumber(2)) {
				var obj = L.ToBounds(1);
				var arg0 = (float)L.ToNumber(2);
				obj.Expand(arg0);
				Update(L, 1, obj);
				return 0;
			}
		}
		
		INVAILD_ARGUMENTS:
		LuaDLL.luaL_error(L, "invalid arguments to method: Bounds.Expand");

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Intersects(IntPtr L)
	{
		L.ChkArgsCount(2);
        var obj = L.ToBounds(1);
        var arg0 = L.ToBounds(2);
		bool o = obj.Intersects(arg0);
		L.PushBoolean(o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ToString(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 1)
		{
            var obj = L.ToBounds(1);
            string o = obj.ToString();
			L.PushString(o);
			return 1;
		}
		else if (count == 2)
		{
            var obj = L.ToBounds(1);
            var arg0 = L.ToLuaString(2);
			string o = obj.ToString(arg0);
			L.PushString(o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: Bounds.ToString");
		}

		return 0;
	}
}


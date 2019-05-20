using UnityEngine;
using System.Collections;
using TinyJSON;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif
using ILuaState = System.IntPtr;

namespace ZFrame.Lua
{
	public static class LibTool
	{

		public const string LIB_NAME = "libtool.cs";

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int OpenLib(ILuaState lua)
		{
			lua.NewTable();

			lua.SetDict("JSONToTable", JSONToTable);
			lua.SetDict("TableToJSON", TableToJSON);
			return 1;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		private static int JSONToTable(ILuaState lua)
		{
			string jsonStr = lua.ChkString(1);
			if (string.IsNullOrEmpty(jsonStr)) {
				lua.PushNil();
			} else {
				lua.PushX(JSON.Load(jsonStr));
			}

			return 1;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		private static int TableToJSON(ILuaState lua)
		{
			Variant jsonObj = lua.ToJsonObj(1);
			if (jsonObj != null) {
				bool prettyPrinted = lua.OptBoolean(2, false);
				Variant.s_GlobalIndent = 0;
				lua.PushString(jsonObj.ToJSONString(prettyPrinted));
			} else lua.PushString("");

			return 1;
		}
	}
}

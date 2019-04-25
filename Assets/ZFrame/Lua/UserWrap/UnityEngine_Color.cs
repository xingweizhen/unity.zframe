using UnityEngine;
using System.Collections;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
using LuaDLL = XLua.LuaDLL.Lua;
#endif
using ILuaState = System.IntPtr;

//namespace ToLua.Wrap
//{
public static class UnityEngine_Color
{
    public const string CLASS = "UnityEngine.Color";
    
    public static void PushX(this ILuaState self, Color value)
    {
        self.CreateTable(0, 4);
        self.SetDict("r", value.r);
        self.SetDict("g", value.g);
        self.SetDict("b", value.b);
        self.SetDict("a", value.a);
        self.L_GetMetaTable(CLASS);
        self.SetMetaTable(-2);
    }

    public static Color ToColor(this ILuaState self, int index)
    {
        var luaT = self.Type(index);
        switch (luaT) {
            case LuaTypes.LUA_TTABLE:
                float r = self.GetNumber(index, "r");
                float g = self.GetNumber(index, "g");
                float b = self.GetNumber(index, "b");
                float a = self.GetNumber(index, "a");
                return new Color(r, g, b, a);
            case LuaTypes.LUA_TNUMBER:
                return ((uint)self.ToInteger(index)).ToColor();
            case LuaTypes.LUA_TSTRING:
                var str = self.ToString(index);
                Color color;
                if (ColorUtility.TryParseHtmlString(str, out color)) {
                    return color;
                }
                LogMgr.W("Can't parse \"{0}\" to a UnityEngine.Color. Fallback to Color.clear.", str);
                return Color.clear;
            case LuaTypes.LUA_TNIL:
                return Color.clear;
            default:
                LogMgr.W("Can't convert a {0} to a UnityEngine.Color. Fallback to Color.clear.", luaT);
                return Color.clear;
        }
    }

    public static bool IsColor(this ILuaState self, int index)
    {
        return self.IsClass(index, CLASS);
    }

    public static void Update(ILuaState L, int index, Color c)
    {
        L.SetNumber(index, "r", c.r);
        L.SetNumber(index, "g", c.g);
        L.SetNumber(index, "b", c.b);
        L.SetNumber(index, "a", c.a);
    }

    public static void Wrap(ILuaState L)
    {

        L.WrapRegister(typeof(Color), _CreateColor, new LuaMethod[] {
            
        }, new LuaField[] {
            new LuaField("grayscale", get_grayscale, null),
            new LuaField("linear", get_linear, null),
            new LuaField("gamma", get_gamma, null),
        }, new LuaMethod[] {
            new LuaMethod("Lerp", Lerp),
            new LuaMethod("LerpUnclamped", LerpUnclamped),
            new LuaMethod("RGBToHSV", RGBToHSV),
            new LuaMethod("HSVToRGB", HSVToRGB),
            new LuaMethod("TryParseHtmlString", TryParseHtmlString),
        }, new LuaField[] {
            new LuaField("red", get_red, null),
            new LuaField("green", get_green, null),
            new LuaField("blue", get_blue, null),
            new LuaField("white", get_white, null),
            new LuaField("black", get_black, null),
            new LuaField("yellow", get_yellow, null),
            new LuaField("cyan", get_cyan, null),
            new LuaField("magenta", get_magenta, null),
            new LuaField("gray", get_gray, null),
            new LuaField("grey", get_grey, null),
            new LuaField("clear", get_clear, null),
        }, new LuaMethod[] {
            new LuaMethod("__add", Lua_Add),
            new LuaMethod("__sub", Lua_Sub),
            new LuaMethod("__mul", Lua_Mul),
            new LuaMethod("__div", Lua_Div),
            new LuaMethod("__eq", Lua_Eq),
            new LuaMethod("__tostring", __tostring),
        });        
    }
    
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int __tostring(ILuaState L)
    {
        L.PushString(L.ToColor(1).ToString());
        return 1;
    }


    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int _CreateColor(ILuaState L)
    {
        int count = L.GetTop() - 1;

        if (count == 3) {
            var arg0 = (float)L.ChkNumber(2);
            var arg1 = (float)L.ChkNumber(3);
            var arg2 = (float)L.ChkNumber(4);
            Color obj = new Color(arg0, arg1, arg2);
            L.PushX(obj);
            return 1;
        } else if (count == 4) {
            var arg0 = (float)L.ChkNumber(2);
            var arg1 = (float)L.ChkNumber(3);
            var arg2 = (float)L.ChkNumber(4);
            var arg3 = (float)L.ChkNumber(5);
            Color obj = new Color(arg0, arg1, arg2, arg3);
            L.PushX(obj);
            return 1;
        } else if (count == 0) {
            Color obj = new Color();
            L.PushX(obj);
            return 1;
        } else {
            LuaDLL.luaL_error(L, "invalid arguments to method: Color.New");
        }
        return 0;
    }
    
    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_red(ILuaState L)
    {
        L.PushX(Color.red);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_green(ILuaState L)
    {
        L.PushX(Color.green);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_blue(ILuaState L)
    {
        L.PushX(Color.blue);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_white(ILuaState L)
    {
        L.PushX(Color.white);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_black(ILuaState L)
    {
        L.PushX(Color.black);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_yellow(ILuaState L)
    {
        L.PushX(Color.yellow);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_cyan(ILuaState L)
    {
        L.PushX(Color.cyan);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_magenta(ILuaState L)
    {
        L.PushX(Color.magenta);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_gray(ILuaState L)
    {
        L.PushX(Color.gray);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_grey(ILuaState L)
    {
        L.PushX(Color.grey);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_clear(ILuaState L)
    {
        L.PushX(Color.clear);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_grayscale(ILuaState L)
    {
        Color obj = L.ToColor(1);
        L.PushNumber(obj.grayscale);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_linear(ILuaState L)
    {
        Color obj = L.ToColor(1);
        L.PushX(obj.linear);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_gamma(ILuaState L)
    {
        Color obj = L.ToColor(1);
        L.PushX(obj.gamma);
        return 1;
    }
    
    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int get_maxColorComponent(ILuaState L)
    {
        Color obj = L.ToColor(1);
        L.PushNumber(obj.maxColorComponent);
        return 1;
    }
    
    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int LerpUnclamped(ILuaState L)
    {
        L.ChkArgsCount(3);
        Color arg0 = L.ToColor(1);
        Color arg1 = L.ToColor(2);
        var arg2 = (float)L.ChkNumber(3);
        Color o = Color.LerpUnclamped(arg0, arg1, arg2);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int RGBToHSV(ILuaState L)
    {
        L.ChkArgsCount(4);
        Color arg0 = L.ToColor(1);
        float arg1;
        float arg2;
        float arg3;
        Color.RGBToHSV(arg0, out arg1, out arg2, out arg3);
        L.PushNumber(arg1);
        L.PushNumber(arg2);
        L.PushNumber(arg3);
        return 3;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int HSVToRGB(ILuaState L)
    {
        int count = LuaDLL.lua_gettop(L);

        if (count == 3) {
            var arg0 = (float)L.ChkNumber(1);
            var arg1 = (float)L.ChkNumber(2);
            var arg2 = (float)L.ChkNumber(3);
            Color o = Color.HSVToRGB(arg0, arg1, arg2);
            L.PushX(o);
            return 1;
        } else if (count == 4) {
            var arg0 = (float)L.ChkNumber(1);
            var arg1 = (float)L.ChkNumber(2);
            var arg2 = (float)L.ChkNumber(3);
            var arg3 = L.ChkBoolean(4);
            Color o = Color.HSVToRGB(arg0, arg1, arg2, arg3);
            L.PushX(o);
            return 1;
        } else {
            LuaDLL.luaL_error(L, "invalid arguments to method: Color.HSVToRGB");
        }

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int TryParseHtmlString(ILuaState L)
    {
        var htmlString = L.ToString(1);
        Color color;
        ColorUtility.TryParseHtmlString(htmlString, out color);
        L.PushX(color);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lerp(ILuaState L)
    {
        L.ChkArgsCount(3);
        Color arg0 = L.ToColor(1);
        Color arg1 = L.ToColor(2);
        var arg2 = (float)L.ChkNumber(3);
        Color o = Color.Lerp(arg0, arg1, arg2);
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Add(ILuaState L)
    {
        L.ChkArgsCount(2);
        Color arg0 = L.ToColor(1);
        Color arg1 = L.ToColor(2);
        Color o = arg0 + arg1;
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Sub(ILuaState L)
    {
        L.ChkArgsCount(2);
        Color arg0 = L.ToColor(1);
        Color arg1 = L.ToColor(2);
        Color o = arg0 - arg1;
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Mul(ILuaState L)
    {
        int count = LuaDLL.lua_gettop(L);

        if (count == 2 && L.IsNumber(1) && L.IsColor(2)) {
            var arg0 = (float)L.ToNumber(1);
            Color arg1 = L.ToColor(2);
            Color o = arg0 * arg1;
            L.PushX(o);
            return 1;
        }

        if (count == 2 && L.IsColor(1) && L.IsNumber(2)) {
            Color arg0 = L.ToColor(1);
            var arg1 = (float)L.ToNumber(2);
            Color o = arg0 * arg1;
            L.PushX(o);
            return 1;
        }

        if (count == 2 && L.IsColor(1) && L.IsColor(2)) {
            Color arg0 = L.ToColor(1);
            Color arg1 = L.ToColor(2);
            Color o = arg0 * arg1;
            L.PushX(o);
            return 1;
        }

        LuaDLL.luaL_error(L, "invalid arguments to method: Color.op_Multiply");

        return 0;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Div(ILuaState L)
    {
        L.ChkArgsCount(2);
        Color arg0 = L.ToColor(1);
        var arg1 = (float)L.ChkNumber(2);
        Color o = arg0 / arg1;
        L.PushX(o);
        return 1;
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int Lua_Eq(ILuaState L)
    {
        L.ChkArgsCount(2);
        Color arg0 = L.ToColor(1);
        Color arg1 = L.ToColor(2);
        bool o = arg0 == arg1;
        L.PushBoolean(o);
        return 1;
    }
}
//}

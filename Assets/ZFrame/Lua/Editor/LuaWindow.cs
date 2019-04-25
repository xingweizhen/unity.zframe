using UnityEngine;
using UnityEditor;
using System.Collections;
#if ULUA
using LuaInterface;
#else
using XLua;
using XLua.LuaDLL;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif
using ILuaState = System.IntPtr;

public class LuaWindow : EditorWindow
{
    [MenuItem("XLua/编辑器...")]
    public static void ShowWindow()
    {
        EditorWindow edtWnd = GetWindow(typeof(LuaWindow));
        edtWnd.minSize = new Vector2(720, 480);
        edtWnd.maxSize = edtWnd.minSize;
    }

    private string m_LuaCodes = string.Empty;
    private static string s_Output = string.Empty;
    private LuaEnv m_Lua;

    private static int L_Print(ILuaState lua)
    {
        int n = lua.GetTop();
        string s = string.Empty;

        lua.GetGlobal("tostring");

        for (int i = 1; i <= n; i++) {
            lua.PushValue(-1);
            lua.PushValue(i);
            lua.PCall(1, 1, 0);

            if (i > 1) {
                s += "\t";
            }
            s += lua.ToString(-1);
            lua.Pop(1);
        }

        s_Output = s;

        return 0;
    }

    private void Awake()
    {
        m_Lua = new LuaEnv();
        var L = m_Lua.L;

        L.PushCSharpFunction(new LuaCSFunction(L_Print));
        L.SetGlobal("print");
    }

    public void OnGUI()
    {
        m_LuaCodes = GUILayout.TextArea(m_LuaCodes, GUILayout.ExpandHeight(true));
        GUILayout.Label(s_Output);
        if (GUILayout.Button("执行")) {
            m_Lua.L.L_DoString(m_LuaCodes + "\n");
            m_Lua.L.SetTop(0);
        }

    }
}

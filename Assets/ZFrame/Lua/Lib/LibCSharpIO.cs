using UnityEngine;
using System.Collections;
using System.IO;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif
using ILuaState = System.IntPtr;

public static class LibCSharpIO
{

    public const string LIB_NAME = "libcsharpio.cs";

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OpenLib(ILuaState lua)
    {
        lua.NewTable();

        lua.SetDict("ReadAllText", ReadAllText);
        lua.SetDict("WriteAllText", WriteAllText);
        lua.SetDict("DeleteFile", DeleteFile);
        lua.SetDict("DeleteDir", DeleteDir);
        lua.SetDict("MoveFile", MoveFile);
        lua.SetDict("CreateDir", CreateDir);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]

    static int ReadAllText(ILuaState lua)
    {
        string path = lua.ChkString(1);
        if (File.Exists(path)) {
            try {
                string text = File.ReadAllText(path);
                lua.PushString(text);
                return 1;
            } catch (System.Exception e) {
                LogMgr.E(e.Message + ": " + path);
            }
        }

        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int WriteAllText(ILuaState lua)
    {
        string path = lua.ChkString(1);
        string text = lua.ChkString(2);
        try {
            File.WriteAllText(path, text);
            lua.PushBoolean(true);
            return 1;
        } catch (System.Exception e) {
            LogMgr.E(e.Message + ": " + path);
            lua.PushBoolean(false);
            return 1;
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int DeleteFile(ILuaState lua)
    {
        string path = lua.ChkString(1);
        if (!File.Exists(path)) {
            lua.PushBoolean(false);
            return 1;
        }

        try {
            File.Delete(path);
            lua.PushBoolean(true);
            return 1;
        } catch (System.Exception e) {
            LogMgr.E(e.Message + ": " + path);
            lua.PushBoolean(false);
            return 1;
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DeleteDir(ILuaState lua)
    {
        string path = lua.ChkString(1);
        try {
            Directory.Delete(path, true);
            lua.PushBoolean(true);
            return 1;
        } catch (System.Exception e) {
            LogMgr.E("DeleteDir {0}:{1}", path, e.Message);
            lua.PushBoolean(false);
            return 1;
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int MoveFile(ILuaState lua)
    {
        string src = lua.ChkString(1);
        string dst = lua.ChkString(2);
        bool overWrite = lua.OptBoolean(3, false);
        if (File.Exists(src)) {
            try {
                if (File.Exists(dst)) {
                    if (overWrite) {
                        File.Delete(dst);
                    } else {
                        return 0;
                    }
                } else {
                    SystemTools.NeedDirectory(Path.GetDirectoryName(dst));
                }
                File.Move(src, dst);
                return 1;
            } catch (System.Exception e) {
                LogMgr.E("MoveFile {0} -> {1}:{2}", src, dst, e.Message);
                lua.PushBoolean(false);
                return 1;
            }
        }

        lua.PushBoolean(true);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int CreateDir(ILuaState lua)
    {
        string path = lua.ChkString(1);
        SystemTools.NeedDirectory(path);
        return 0;
    }
}

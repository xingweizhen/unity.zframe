using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyLuaScriptsMgr : ZFrame.Lua.LuaScriptMgr
{
    protected override void InitLuaEnv()
    {
        // 在这里进行一些自定义的Lua环境初始化
        base.InitLuaEnv();
        // m_Env.AddBuildin();
    }
}

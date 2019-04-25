using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using ZFrame;
#if ULUA
using LuaInterface;
#else
using XLua;
#endif

public class LuaClock : MonoBehaviour
{
    [SerializeField]
    private string m_LuaScript;
    [SerializeField]
    private string m_Method;
    public float interval = 1;
    public bool ignoreTimeScale = true;

    private float m_Time;    
    private int m_FuncRef;
    private LuaFunction m_Func;

    private void Start()
    {
        if (!string.IsNullOrEmpty(m_LuaScript)) {
            var lua = LuaScriptMgr.Instance.L;
            var pkgName = LuaComponent.GetPackName(m_LuaScript);
            lua.GetGlobal("PKG", pkgName, m_Method);
            m_Func = lua.ToLuaFunction(-1);
            lua.Pop(1);
        }
        m_Time = 0;
    }

    private void Update()
    {
        m_Time += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        var pass = Mathf.FloorToInt(m_Time / interval);        
        if (pass > 0) {
            m_Time -= pass * interval;

            // call without gc
            var lua = m_Func.GetState();
            var b = m_Func.BeginPCall();
            lua.PushInteger(pass);
            lua.ExecPCall(1, 0, b);
        }
    }

}

using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class LogMgr : MonoSingleton<LogMgr>
{
    public const string DEBUG = "XWZ_DEBUG";
    public const string UNITY_EDITOR = "UNITY_EDITOR";
    public const string UNITY_STANDALONE = "UNITY_STANDALONE";
    public const string DEVELOPMENT_BUILD = "DEVELOPMENT_BUILD";

    const string PROMPT = "{0} {1}";
    public enum LogLevel
    {
        I,
        D,
        W,
        E,
    }

    public LogLevel setLevel = LogLevel.D;
#if UNITY_EDITOR || UNITY_STANDALONE || DEVELOPMENT_BUILD
    private static LogLevel m_Level = LogLevel.D;
#else
    private static LogLevel m_Level = LogLevel.E;
#endif

    public static LogLevel logLevel { get { return m_Level; } private set { m_Level = value; } }

    protected override void Awaking()
    {
#if UNITY_EDITOR
        logLevel = setLevel;
        string DEBUG_FILE = null;
#elif UNITY_STANDALONE
        string DEBUG_FILE = "debug.cfg";
#else
        string DEBUG_FILE = Application.persistentDataPath + "/debug.cfg";
#endif
        if (System.IO.File.Exists(DEBUG_FILE)) {
            string cfg = System.IO.File.ReadAllText(DEBUG_FILE);
            var js = TinyJSON.JSON.Load(cfg);
            logLevel = (LogLevel)(int)js["logLevel"];
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        logLevel = setLevel;
    }
#endif

    public void SetLevel(LogLevel level)
    {
        m_Level = level;
        setLevel = level;        
    }

    public static void Log(LogLevel l, string format, params object[] Args)
    {
        if (logLevel <= l) {
            switch (l) {
                case LogLevel.E:
                    UnityEngine.Debug.LogErrorFormat(format, Args);
                    break;
                case LogLevel.W:
                    UnityEngine.Debug.LogWarningFormat(format, Args);
                    break;
                case LogLevel.D:
                    UnityEngine.Debug.LogFormat(format, Args);
                    break;
                default:
                    UnityEngine.Debug.LogFormat(PROMPT, l, string.Format(format, Args));
                    break;
            }
        }
    }

    public static void Log(LogLevel l, Object context, string format, params object[] Args)
    {
        if (logLevel <= l) {
            switch (l) {
                case LogLevel.E:
                    UnityEngine.Debug.LogErrorFormat(context, format, Args);
                    break;
                case LogLevel.W:
                    UnityEngine.Debug.LogWarningFormat(context, format, Args);
                    break;
                case LogLevel.D:
                    UnityEngine.Debug.LogFormat(context, format, Args);
                    break;
                default:
                    UnityEngine.Debug.LogFormat(context, PROMPT, l, string.Format(format, Args));
                    break;
            }
        }
    }

    public static void I(string format, params object[] Args)
    {
        Log(LogLevel.I, format, Args);
    }

    public static void D(string format, params object[] Args)
    {
        Log(LogLevel.D, format, Args);
    }

    public static void W(string format, params object[] Args)
    {
        Log(LogLevel.W, format, Args);
    }

    public static void E(string format, params object[] Args)
    {
        Log(LogLevel.E, format, Args);
    }
    
    public static void I(Object context, string format, params object[] Args)
    {
        Log(LogLevel.I, context, format, Args);
    }

    public static void D(Object context, string format, params object[] Args)
    {
        Log(LogLevel.D, context, format, Args);
    }

    public static void W(Object context, string format, params object[] Args)
    {
        Log(LogLevel.W, context, format, Args);
    }

    public static void E(Object context, string format, params object[] Args)
    {
        Log(LogLevel.E, context, format, Args);
    }

    public static void Info(object message)
    {
        if (logLevel <= LogLevel.I) {
            UnityEngine.Debug.Log(message);
        }
    }

    public static void Log(object message)
    {
        if (logLevel <= LogLevel.D) {
            UnityEngine.Debug.Log(message);
        }
    }

    public static void LogWarning(object message)
    {
        if (logLevel <= LogLevel.W) {
            UnityEngine.Debug.LogWarning(message);
        }
    }

    public static void LogError(object message)
    {
        if (logLevel <= LogLevel.E) {
            UnityEngine.Debug.LogError(message);
        }
    }
}

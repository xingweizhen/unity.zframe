using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame
{
    public enum LogLevel
    {
        E = LogType.Error,
        W = LogType.Warning,
        D = LogType.Log,
        I,
    }

    public static class Log
    {
        public const string UNITY_EDITOR = "UNITY_EDITOR";
        public const string UNITY_STANDALONE = "UNITY_STANDALONE";
        public const string DEVELOPMENT_BUILD = "DEVELOPMENT_BUILD";

        private static LogLevel s_Level = LogLevel.D;
        public static LogLevel level {
            get { return s_Level; }
            set {
                s_Level = value;
                if (value == LogLevel.I) value = LogLevel.D;
                if (Debug.unityLogger != null)
                    Debug.unityLogger.filterLogType = (LogType)value;
            }
        }

        public static void LogFormat(this Object context, LogLevel l, string format, params object[] args)
        {
            if (Debug.unityLogger == null) return;

            if (l < LogLevel.I) {
                Debug.unityLogger.LogFormat((LogType)l, context, format, args);
            } else if (s_Level == LogLevel.I) {
                Debug.unityLogger.LogFormat(LogType.Log, context, "I: " + format, args);
            }
        }

        public static void Format(LogLevel l, string format, params object[] args)
        {
            if (Debug.unityLogger == null) return;

            if (l < LogLevel.I) {
                Debug.unityLogger.LogFormat((LogType)l, format, args);
            } else if (s_Level == LogLevel.I) {
                Debug.unityLogger.LogFormat(LogType.Log, "I: " + format, args);
            }
        }

        public static void Exception(System.Exception exception, Object context)
        {
            if (Debug.unityLogger != null)
                Debug.unityLogger.LogException(exception, context);
        }
    }
}

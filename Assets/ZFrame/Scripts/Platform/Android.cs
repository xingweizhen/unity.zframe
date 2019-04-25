using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR || UNITY_ANDROID
namespace ZFrame.Platform
{
    public class Android : IPlatform
    {
        public void OnAppLaunch()
        {
#if !DEVELOPMENT_BUILD
            Application.logMessageReceived += Application_logMessageReceived;
#endif
        }

        public void CancelAllNotification()
        {
            //Call(UTIL_PKG_NAME + ".XNotification", "CancelAllNotifications");
        }

        public void ScheduleNotification(Notice notice)
        {
            //if (notice.daily) {
            //    Call(UTIL_PKG_NAME + ".XNotification", "RegDailyNotification", 
            //        notice.id, notice.title, notice.icon, notice.message, notice.remainingHour);
            //} else {
            //    Call(UTIL_PKG_NAME + ".XNotification", "RegOnceNotification",
            //        notice.id, notice.title, notice.icon, notice.message, (int)(notice.remainingHour * 3600));
            //}
        }

        public void MessageBox(string json)
        {
            //Call(UTIL_PKG_NAME + ".XAlertDialog", "Alert", json);
        }

        public string ProcessingData(string json)
        {
            return string.Empty;
            //return CallR<string>(SDK_PKG_NAME + ".SDKApi", "OnGameMessageReturn", json);
        }

        public bool CheckPermission(string permission)
        {
            if (GetAPILevel() >= 23) {
                permission = "android.permission." + permission;

                var ret = Call<int>(null, "checkSelfPermission", ToJavaString(permission));
                return ret == 0;
            }
            return true;
        }

        public bool RequestPermission(string permission)
        {
            if (CheckPermission(permission)) return true;

            // 申请权限
            permission = "android.permission." + permission;
            Call(null, "requestPermissions", new[] { ToJavaString(permission) }, 1);

            return false;
        }

        public void Call(string className, string method, params object[] args)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity")) {
                    if (string.IsNullOrEmpty(className)) {
                        jo.Call(method, args);
                    } else {
                        using (AndroidJavaClass jc_info = new AndroidJavaClass(className)) {
                            List<object> li = new List<object>();
                            li.Add(jo);
                            li.AddRange(args);
                            jc_info.CallStatic(method, li.ToArray());
                        }
                    }
                }
            }
        }

        public T Call<T>(string className, string method, params object[] args)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity")) {
                    if (string.IsNullOrEmpty(className)) {
                        return jo.Call<T>(method, args);
                    }

                    using (AndroidJavaClass jc_info = new AndroidJavaClass(className)) {
                        List<object> li = new List<object>();
                        li.Add(jo);
                        li.AddRange(args);
                        return jc_info.CallStatic<T>(method, li.ToArray());
                    }
                }
            }
        }

        public static AndroidJavaObject ToJavaString(string csharpString)
        {
            return new AndroidJavaObject("java.lang.String", csharpString);
        }

        public static string ToCSharpString(AndroidJavaObject javaString)
        {
            byte[] resultByte = javaString.Call<byte[]>("getBytes");
            return System.Text.Encoding.Default.GetString(resultByte);
        }

        public static int GetAPILevel()
        {
            return new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
        }

        private static void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Assert) {
                var trace = new System.Diagnostics.StackTrace();
                Debug.Log(trace.ToString());
            }
        }
    }
}
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//#if ULUA
//using LuaInterface;
//#else
//using XLua;
//#endif

namespace ZFrame.Platform
{
    public class Standalone : IPlatform
    {
        private static string m_LogPath;

        public void CancelAllNotification()
        {
            
        }

        public void MessageBox(string json)
        {
//            var lua = LuaScriptMgr.Instance.L;
//            var jo = TinyJSON.JSON.Load(json);
//            var chunk = string.Format(
//            @"local MB = _G.UI.MBox
//              local Params = {2}title='{0}',content='{1}'{3}
//              if MB.is_active(Params) then MB.close(); return end
//              if MB.is_queued(Params) then return end
//              MB.make()
//                :set_param('title', '{0}')
//                :set_param('content', '{1}')
//                :set_param('block', true)
//                :set_event(_G.DY_DATA.AlertCBF['1'])
//                :show()", jo["title"], jo["message"], '{', '}');
//            var ret = lua.L_DoString(chunk);
//
//            if (ret != LuaThreadStatus.LUA_OK) {
//                LogMgr.E("{0}\n{1}", chunk, lua.ToString(-1));
//            }
        }

        public virtual void OnAppLaunch()
        {
            
        }

        public virtual void SetAppTitle(string title)
        {
            LogMgr.D("设置应用标题: {0}", title);
        }

        public string ProcessingData(string json)
        {
            return string.Empty;
        }

        public void ScheduleNotification(Notice notice)
        {
            
        }

        public bool RequestPermission(string permission)
        {
            return true;
        }

        public void Call(string className, string method, params object[] args)
        {

        }

        public T Call<T>(string className, string method, params object[] args)
        {
            return default(T);
        }


        protected static void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (m_LogPath == null) {
                m_LogPath = string.Format("log_{0}.txt", System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            }
            switch (type) {
                case LogType.Log:
                case LogType.Warning:
                    System.IO.File.AppendAllText(m_LogPath, string.Format("{0}: {1}\r\n",
                        Mathf.Round(Time.realtimeSinceStartup * 1000), condition));
                    break;
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    System.IO.File.AppendAllText(m_LogPath, string.Format("{0}: {1}\r\n{2}\r\n",
                        Mathf.Round(Time.realtimeSinceStartup * 1000), condition, stackTrace));
                    break;
            }

        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TinyJSON;

namespace ZFrame.Platform
{

    // warning CS0168: 声明了变量，但从未使用
    // warning CS0219: 给变量赋值，但从未使用
#pragma warning disable 0168, 0219, 0414
    public class NotifyMgr : MonoSingleton<NotifyMgr>
    {
        public static event System.Action<bool> onAppPause;
        public static event System.Action<bool> onAppFocus;
                
        private void Start()
        {
            if (SDKManager.Instance) {
                SDKManager.Instance.plat.CancelAllNotification();
            }
        }
        
        /// <summary>
        /// 强制暂停时，先 OnApplicationPause，后 OnApplicationFocus；
        /// 重新“启动”手机时，先OnApplicationFocus，后 OnApplicationPause；
        /// </summary>
        private void OnApplicationPause(bool paused)
        {
            if (!paused && SDKManager.Instance) {
                SDKManager.Instance.plat.CancelAllNotification();
            }
            if (onAppPause != null) onAppPause.Invoke(paused);
        }

        private void OnApplicationFocus(bool focused)
        {
            if (onAppFocus != null) onAppFocus.Invoke(focused);
        }

        private void AlertMessage(string json)
        {
            if (SDKManager.Instance) {
                SDKManager.Instance.plat.MessageBox(json);
            }
        }
        public void RegDailyNotification(string json)
        {
            if (!SDKManager.Instance) return;

            var js = JSON.Load(json);
            float hour = js["hour"];
            if (hour >= 0 && hour < 24) {
                string message = js["message"];
                string title = js["title"];
                string icon = js["icon"];
                int id = js["id"];
                var notice = new Notice(id, icon, title, message, hour, true);
                SDKManager.Instance.plat.ScheduleNotification(notice);
            } else {
                LogMgr.W("[LocalNotification] Wrong hour = " + hour);
            }
        }

        public void RegOnceNotification(string json)
        {
            if (!SDKManager.Instance) return;

            var js = JSON.Load(json);
            int id = js["id"];
            string icon = js["icon"];
            string title = js["title"];
            string message = js["message"];
            float hour = js["hour"];
            var notice = new Notice(id, icon, title, message, hour, false);
            SDKManager.Instance.plat.ScheduleNotification(notice);
        }
    }
}

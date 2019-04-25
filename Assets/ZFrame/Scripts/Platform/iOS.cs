using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR || UNITY_IOS
namespace ZFrame.Platform
{
    using UnityEngine.iOS;
    using System.Runtime.InteropServices;

    public class iOS : IPlatform
    {
        
        [DllImport("__Internal")]
        public static extern string ProcessingData(string param);
        [DllImport("__Internal")]
        public static extern void OnAppLaunch();
        [DllImport("__Internal")]
        private static extern void setApplicationIconBadgeNumber(int n);
        [DllImport("__Internal")]
        private static extern void Alert(string json);

        void IPlatform.OnAppLaunch()
        {
            OnAppLaunch();
        }

        public void ScheduleNotification(Notice notice)
        {
            System.DateTime nowDate = System.DateTime.Now;
            var hour = notice.remainingHour;
            if (notice.daily) {
                int h = (int)hour;
                hour = hour - h;
                int year = nowDate.Year;
                int month = nowDate.Month;
                int day = nowDate.Day;
                nowDate = new System.DateTime(year, month, day, h, 0, 0).AddHours(hour);
                if (nowDate <= System.DateTime.Now) {
                    nowDate = nowDate.AddDays(1);
                }
            } else {
                nowDate = nowDate.AddHours(hour);
            }

            if (nowDate > System.DateTime.Now) {
                LocalNotification localNotification = new LocalNotification();
                localNotification.fireDate = nowDate;
                localNotification.alertBody = notice.message;
                localNotification.applicationIconBadgeNumber = 1;
                localNotification.hasAction = true;
                localNotification.soundName = LocalNotification.defaultSoundName;
                if (notice.daily) {
                    localNotification.repeatCalendar = CalendarIdentifier.ChineseCalendar;
                    localNotification.repeatInterval = CalendarUnit.Day;
                }
                NotificationServices.ScheduleLocalNotification(localNotification);

                LogMgr.D("Local Ntf:@{0}, = {1}", nowDate.ToLongTimeString(), notice);
            }
        }
        
        public void CancelAllNotification()
        {
            NotificationServices.CancelAllLocalNotifications();
            NotificationServices.ClearLocalNotifications();
            setApplicationIconBadgeNumber(0);
        }

        public void MessageBox(string json)
        {
            Alert(json);
        }

        public bool RequestPermission(string permission)
        {
            return true;
        }

        string IPlatform.ProcessingData(string json)
        {
            return ProcessingData(json);
        }

        public void Call(string className, string method, params object[] args)
        {

        }

        public T Call<T>(string className, string method, params object[] args)
        {
            return default(T);
        }
    }
}
#endif

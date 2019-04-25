using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Platform
{
    public class Notice
    {
        public int id;
        public string icon;
        public string title;
        public string message;
        public float remainingHour;
        public bool daily;
        public Notice(int id, string icon, string title, string message, float remainingHour, bool daily)
        {
            this.id = id;
            this.icon = icon;
            this.title = title;
            this.message = message;
            this.remainingHour = remainingHour;
            this.daily = daily;
        }
    }

    public interface IPlatform
    {
        void OnAppLaunch();
        
        void ScheduleNotification(Notice notice);
        void CancelAllNotification();
        void MessageBox(string json);
        string ProcessingData(string json);
        bool RequestPermission(string permission);

        void Call(string className, string method, params object[] args);
        T Call<T>(string className, string method, params object[] args);
    }
}

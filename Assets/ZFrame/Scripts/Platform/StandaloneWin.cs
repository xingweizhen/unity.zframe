using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
namespace ZFrame.Platform
{
    using System;
    using System.Runtime.InteropServices;

    public class StandaloneWin : Standalone
    {
#region WIN32API
        delegate bool EnumWindowsCallBack(IntPtr hwnd, IntPtr lParam);
        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern bool SetWindowTextW(IntPtr hwnd, string title);
        [DllImport("user32")]
        private static extern int EnumWindows(EnumWindowsCallBack lpEnumFunc, IntPtr lParam);
        [DllImport("user32")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref IntPtr lpdwProcessId);
#endregion

        private IntPtr myWindowHandle;

        private bool EnumWindCallback(IntPtr hwnd, IntPtr lParam)
        {
            IntPtr pid = IntPtr.Zero;
            GetWindowThreadProcessId(hwnd, ref pid);
            // 判断当前窗口是否属于本进程
            if (pid == lParam) {
                myWindowHandle = hwnd;
                return false;
            }
            return true;
        }

        public override void OnAppLaunch()
        {
            // 获取进程ID
            IntPtr handle = (IntPtr)System.Diagnostics.Process.GetCurrentProcess().Id;
            // 枚举查找本窗口
            EnumWindows(EnumWindCallback, handle);

            Application.logMessageReceived += Application_logMessageReceived;
        }
        
        public override void SetAppTitle(string title)
        {
            // 设置窗口标题
            SetWindowTextW(myWindowHandle, title);
        }
    }
}
#endif

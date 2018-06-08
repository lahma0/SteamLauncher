using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SteamLauncher.Tools
{
    public class ActiveWindow
    {
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
            IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc,
            uint idProcess, uint idThread, uint dwFlags);

        public delegate void ActiveWindowChangedHandler(object sender, String windowHeader, IntPtr hwnd);

        public event ActiveWindowChangedHandler ActiveWindowChanged;

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread,
            uint dwmsEventTime);

        private const uint WINEVENT_OUTOFCONTEXT = 0;

        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        private IntPtr _hook;

        private readonly WinEventDelegate _winEventProc;

        public ActiveWindow()
        {
            _winEventProc = new WinEventDelegate(WinEventProc);
        }

        ~ActiveWindow()
        {
            DisableHook();
        }

        public void EnableHook()
        {
            _hook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND,
                EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventProc,
                0, 0, WINEVENT_OUTOFCONTEXT);
        }

        public void DisableHook()
        {
            try { UnhookWinEvent(_hook); } catch { }
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
            int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType != EVENT_SYSTEM_FOREGROUND)
                return;

            ActiveWindowChanged?.Invoke(this, GetActiveWindowTitle(hwnd), hwnd);
        }

        private string GetActiveWindowTitle(IntPtr hwnd)
        {
            var buff = new StringBuilder(500);
            GetWindowText(hwnd, buff, buff.Capacity);
            return buff.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using SteamLauncher.Logging;

namespace SteamLauncher.Tools
{
    public sealed class ActiveWindow
    {
        private bool IsHookEnabled { get; set; }

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
            IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc,
            uint idProcess, uint idThread, uint dwFlags);

        public delegate void ActiveWindowChangedHandler(object sender, string windowHeader, IntPtr hwnd);

        // ReSharper disable once InconsistentNaming
        private event ActiveWindowChangedHandler _activeWindowChanged;

        public event ActiveWindowChangedHandler ActiveWindowChanged
        {
            add
            {
                Logger.Info("Adding new subscriber to ActiveWindowChanged.");

                // Start listening for Window Foreground Events
                if (!IsHookEnabled)
                    EnableHook();

                _activeWindowChanged += value;
            }
            remove
            {
                // If already null, there are no subscribers, so attempting to remove an event subscription is invalid
                if (_activeWindowChanged == null)
                    return;

                // Remove event subscriber
                Logger.Info("Removing subscriber from ActiveWindowChanged.");
                _activeWindowChanged -= value;

                // When there are no existing event subscribers, _activeWindowChanged == null.
                if (_activeWindowChanged == null)
                {
                    // Disable WinEventHook and stop listening for window foreground events.
                    Logger.Info("Last subscriber unsubscribed from ActiveWindowChanged.");
                    DisableHook();
                }
            }
        }

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread,
            uint dwmsEventTime);

        private IntPtr _hook;

        private readonly WinEventDelegate _winEventProc;

        #region Singleton Constructor/Destructor

        private static readonly Lazy<ActiveWindow> Lazy = new Lazy<ActiveWindow>(() => new ActiveWindow());

        public static ActiveWindow Instance => Lazy.Value;

        private ActiveWindow()
        {
            //Logger.Info($"Instantiating {nameof(ActiveWindow)} singleton...");

            // _winEventProc = new WinEventDelegate(WinEventProc);
            _winEventProc = WinEventProc;
        }

        ~ActiveWindow()
        {
            //Logger.Info($"Executing {nameof(ActiveWindow)} destructor...");
            DisableHook();
        }

        #endregion

        private void EnableHook()
        {
            if (IsHookEnabled)
                return;

            Logger.Info("Window event hook enabled.");
            _hook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND,
                EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventProc,
                0, 0, WINEVENT_OUTOFCONTEXT);

            IsHookEnabled = true;
        }

        private void DisableHook()
        {
            if (!IsHookEnabled)
                return;

            if (_hook == IntPtr.Zero)
            {
                IsHookEnabled = false;
                return;
            }

            try
            {
                Logger.Info("Window event hook disabled.");
                IsHookEnabled = !UnhookWinEvent(_hook);
            } catch { }
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
            int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType != EVENT_SYSTEM_FOREGROUND)
                return;

            var activeWindowTitle = WindowMgmt.GetWindowTitle(hwnd);
            Logger.Info($"WinEventProc - Window Title: '{activeWindowTitle}' - Handle: '{hwnd}'.");

            _activeWindowChanged?.Invoke(this, activeWindowTitle, hwnd);
        }

        #region EnumWindows

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary> 
        /// Find all windows that match the given filter.
        /// </summary>
        /// <param name="filter"> A delegate that returns true for windows that should be returned and false for 
        /// windows that should not be returned </param>
        /// <returns>A list of window handles matching the described parameters.</returns>
        public static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            // var found = IntPtr.Zero;
            var windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr window, IntPtr param)
            {
                if (filter(window, param))
                {
                    // only add the windows that pass the filter
                    windows.Add(window);
                }

                // but return true here so that we iterate all windows
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary> 
        /// Find all windows that contain the given title text.
        /// </summary>
        /// <param name="titleText"> The text that the window title must contain. </param>
        /// <returns>A list of window handles matching the described parameters.</returns>
        public static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
        {
            return FindWindows((window, param) => WindowMgmt.GetWindowTitle(window).Contains(titleText));
        }

        /// <summary> 
        /// Find all windows whose title starts with the given text.
        /// </summary>
        /// <param name="titleText"> The text that the window title must start with.</param>
        /// <returns>A list of window handles matching the described parameters.</returns>
        public static IEnumerable<IntPtr> FindWindowsStartsWithText(string titleText)
        {
            return FindWindows((window, param) => WindowMgmt.GetWindowTitle(window).StartsWith(titleText));
        }

        /// <summary>
        /// Finds all processes whose main window caption contains the provided text.
        /// </summary>
        /// <param name="titleText">The text to look for in the window's caption.</param>
        /// <param name="sleepMs">How long to sleep thread prior to enumerating windows.</param>
        /// <returns>A list of processes matching the described parameters.</returns>
        public static IEnumerable<Process> FindProcsWithMainWindowCaption(string titleText, int sleepMs = 0)
        {
            // I cannot figure out for the life of me why I thought it necessary to add this.. really strange
            if (sleepMs > 0)
                Thread.Sleep(sleepMs);

            var windows = FindWindowsWithText(titleText);
            var procs = new List<Process>();
            foreach (var handle in windows)
            {
                try
                {
                    WindowMgmt.GetWindowThreadProcessId(handle, out uint procId);
                    if (procId != 0)
                        procs.Add(Process.GetProcessById(Convert.ToInt32(procId)));
                }
                catch { }
            }

            return procs;
        }

        #endregion
    }
}

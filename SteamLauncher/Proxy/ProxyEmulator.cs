using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SteamLauncher.Logging;
using SteamLauncher.ProcWatch;
using SteamLauncher.DataStore;
using SteamLauncher.Shortcuts;
using SteamLauncher.Tools;
using Unbroken.LaunchBox.Plugins.Data;

namespace SteamLauncher.Proxy
{
    public class ProxyEmulator
    {
        public bool IsDisposed { get; private set; }

        private IntPtr GameWindowHandle { get; set; }

        private IntPtr _proxyWindowHandle;
        private IntPtr ProxyWindowHandle
        {
            get
            {
                if (_proxyWindowHandle == default)
                    _proxyWindowHandle = WindowMgmt.FindWindow(null, "SteamLauncherProxy");

                return _proxyWindowHandle;
            }
        }

        private IntPtr _lbGameStartupWindowHandle;
        private IntPtr LbGameStartupWindowHandle
        {
            get
            {
                if (_lbGameStartupWindowHandle == default)
                    _lbGameStartupWindowHandle = WindowMgmt.FindWindow(null, "LaunchBox Game Startup");

                return _lbGameStartupWindowHandle;
            }
        }

        private IntPtr LbWindowHandle { get; }

        /// <summary>
        /// Marks the exact time that the game is started.
        /// </summary>
        private DateTime GameStartedTime { get; set; }

        /// <summary>
        /// As long as this value is true, OnActiveWindowChanged will try to prevent Steam from gaining focus each time
        /// it is run. This property will be changed to false once 'TotalSecondsToPreventSteamFocus' is expired which
        /// is why this property is used instead of only reading the config's value. Since this class is disposed and
        /// recreated on every game launch, there is no need to manage or reset this value between game launches.
        /// </summary>
        private bool IsPreventSteamFocusStealingEnabled { get; set; } = Settings.Config.PreventSteamFocusStealing;

        private ProcessWatcher _procWatcher;

        private ProcessWatcher ProcWatcher
        {
            get => _procWatcher;
            set
            {
                if (_procWatcher != null)
                {
                    Logger.Info("Disposing previous ProcessWatcher instance.");
                    _procWatcher.StatusUpdatedEventHandler -= ProcessWatcherOnStatusUpdated;
                    _procWatcher.Stop();
                }

                _procWatcher = value;
            }
        }

        private bool ActiveWindowHookEnabled { get; set; }

        public IGame Game { get; }

        public IEmulator Emulator { get; }

        private dynamic Launcher
        {
            get
            {
                if (Emulator != null)
                    return Emulator;

                return Game;
            }
        }

        public ProxyEmulator(IGame game, IEmulator emulator)
        {
            Game = game;
            Emulator = emulator;
            LbWindowHandle = Info.LaunchBoxProcess.MainWindowHandle;
        }

        public void CreateLaunchSteamShortcut()
        {
            EnableActiveWindowHook();
            //FixLbGameStartupWindowZOrder();

            // Create game/emulator shortcut
            var gameShortcut = Emulator == null
                ? GameShortcut.CreateGameShortcut(Game)
                : new EmulatorShortcut(Game, Emulator);

            // Override shortcut launch args with the running proxy's args (these are going to be 100% accurate in 
            // most cases bc they are the actual args LB would use to launch the game/emulator normally)
            gameShortcut.OverrideLaunchArguments = GetProxyArgString() ??
                                                   throw new Exception(
                                                       "Failed to get argument string from SteamLauncherProxy.");

            // Create Steam shortcut
            var steamShortcut = gameShortcut.GenerateSteamShortcut();

            // var actualGameExe = Game.GetAllCustomFields().SingleOrDefault(x => x.Name.EqualsIgnoreCase("ActualGameExe"));

            // Check config for a matching launcher-to-actual-exe relationship; if found, watch that proc instead
            var watchProcFileInfo = LauncherToExe.ResolveRelationship(new FileInfo(gameShortcut.ResolvedLaunchExePath));

            // Start ProcessWatcher
            StartProcessWatcher(watchProcFileInfo,
                                Settings.Config.ProcessStartTimeoutSec,
                                Settings.Config.ProcessWatcherPollingIntervalSec);

            //ChangeProxyWindowZOrder();

            // Launch Steam shortcut
            if (!steamShortcut.LaunchShortcut())
            {
                throw new Exception("Failed to launch Steam shortcut.");
            }

            // Record the time the game was started
            GameStartedTime = DateTime.Now;

            //SendSteamWindowToBackground();
            //_ = FixWindowOrder();
            //FixLbGameStartupWindowZOrder();

            //TestManageWindows();
            //EnableActiveWindowHook();
        }

        private void ChangeProxyWindowZOrder()
        {
            var hwnd = ProxyWindowHandle;
            if (hwnd == default)
            {
                Logger.Info("Couldn't find SteamLauncherProxy window");
                return;
            }
            var result = WindowMgmt.SetWindowTopMost(hwnd, true);
            Logger.Info($"Result of 'SetWindowTopMost' on SteamLauncherProxy: {result}");
            WindowMgmt.SetForegroundWindow(hwnd);
        }

        private void TestManageWindows()
        {
            var hwnd = WindowMgmt.FindWindow(null, "LaunchBox Game Startup");
            if (hwnd == default)
            {
                Logger.Info("Couldn't find LaunchBox Startup window");
                return;
            }

            var result = WindowMgmt.SetWindowTopMost(hwnd, true);
            Logger.Info($"Result of 'SetWindowTopMost': {result}");
            Task.Run(async () =>
            {
                await Task.Delay(12000);
                var result2 = WindowMgmt.SetWindowTopMost(hwnd, false);
                Logger.Info($"Result of 'SetWindowTopMost' (not topmost): {result2}");
            });
        }

        public void DisposeProxyEmulator()
        {
            if (IsDisposed)
                return;

            Logger.Info("Disposing ProxyEmulator.");
            try
            {
                StopProcessWatcher();
                DisableActiveWindowHook();
                KillProxyProcesses();
                RestoreAppPaths();
            }
            catch (Exception ex)
            {
                Logger.Warning($"An exception occurred while attempting to dispose of ProxyEmu: {ex.Message}");
            }

            IsDisposed = true;
        }

        private void StartProcessWatcher(FileInfo procFileInfo,
                                         int waitForStartTimeoutSec = 10,
                                         int pollingIntervalSec = 2)
        {
            Logger.Info($"Starting ProcessWatcher on '{procFileInfo.Name}'.");
            ProcWatcher = new ProcessWatcher(procFileInfo, waitForStartTimeoutSec, pollingIntervalSec);
            ProcWatcher.StatusUpdatedEventHandler += ProcessWatcherOnStatusUpdated;
            ProcWatcher.Start();
        }

        private void StopProcessWatcher()
        {
            ProcWatcher = null;
        }

        private void ProcessWatcherOnStatusUpdated(object sender, ProcessWatcherEventArgs e)
        {
            switch (e.Status)
            {
                case ProcessWatcherEventArgs.ProcessStatus.Started:
                    Logger.Info($"Process '{e.ProcessFileInfo.Name}' started.");
                    //EnableActiveWindowHook();

                    Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        WindowMgmt.ShowWindow(LbGameStartupWindowHandle, ShowWindowCmd.SW_HIDE);
                    });

                    break;
                case ProcessWatcherEventArgs.ProcessStatus.Stopped:
                    Logger.Info($"Process '{e.ProcessFileInfo.Name}' stopped.");

                    // IT IS WORKING - DO NOT MESS WITH IT - Ensures LB shutdown screen won't be accidentally dismissed
                    // by switching windows while game/emu is still running.. also ensures LB window becomes the
                    // foreground/active window after the LB Game Startup/Shutdown screen closes (even if you switched
                    // windows while the game/emu was still running)
                    WindowMgmt.EnableWindow(LbGameStartupWindowHandle, true);
                    WindowMgmt.SetWindowTopMost(LbGameStartupWindowHandle, true);
                    WindowMgmt.SetWindowTopMost(LbWindowHandle, false);
                    WindowMgmt.ShowWindow(LbGameStartupWindowHandle, ShowWindowCmd.SW_SHOW);


                    //ChangeWindowZOrder(LbWindowHandle, LbGameStartupWindowHandle);

                    //// Wait for the LB Game Startup window to close and then ensure LB window is in foreground
                    //Task.Run(async () =>
                    //{
                    //    await Task.Delay(2000);

                    //    var pollingMs = 150;
                    //    var timeoutMs = 5000;
                    //    var waitedMs = 0;
                    //    var foregroundWindow = WindowMgmt.GetForegroundWindow();

                    //    while (foregroundWindow == LbGameStartupWindowHandle && 
                    //           foregroundWindow != LbWindowHandle && 
                    //           waitedMs < timeoutMs)
                    //    {
                    //        await Task.Delay(pollingMs);
                    //        waitedMs += pollingMs;
                    //        foregroundWindow = WindowMgmt.GetForegroundWindow();
                    //    } 

                    //    if (foregroundWindow != LbWindowHandle)
                    //        WindowMgmt.SetForegroundWindow(LbWindowHandle);
                    //});

                    //Task.Run(async () =>
                    //{
                    //    await Task.Delay(2000);
                    //    if (WindowMgmt.GetForegroundWindow() != LbWindowHandle)
                    //    {
                    //        Logger.Info("Forcing LaunchBox window into the foreground because Steam window stole focus.");
                    //        WindowMgmt.SetForegroundWindow(LbWindowHandle);
                    //    }
                    //});

                    break;
                case ProcessWatcherEventArgs.ProcessStatus.WaitForStartTimeout:
                    Logger.Info($"Timed out waiting for '{e.ProcessFileInfo.Name}'.");
                    break;
            }

            if (e.Status == ProcessWatcherEventArgs.ProcessStatus.Stopped ||
                e.Status == ProcessWatcherEventArgs.ProcessStatus.WaitForStartTimeout)
            {
                DisposeProxyEmulator();
            }
        }

        private void EnableActiveWindowHook()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var activeWindow = ActiveWindow.Instance;
                if (activeWindow == null)
                    return;

                if (ActiveWindowHookEnabled)
                    return;

                Logger.Info($"{nameof(ProxyOnActiveWindowChanged)} is subscribing to {nameof(ActiveWindow.ActiveWindowChanged)}.");
                try
                {
                    activeWindow.ActiveWindowChanged += ProxyOnActiveWindowChanged;
                    ActiveWindowHookEnabled = true;
                }
                catch { }
            });
        }

        private void DisableActiveWindowHook()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var activeWindow = ActiveWindow.Instance;
                if (activeWindow == null || !ActiveWindowHookEnabled)
                    return;

                Logger.Info($"{nameof(ProxyOnActiveWindowChanged)} is unsubscribing from {nameof(ActiveWindow.ActiveWindowChanged)}.");
                try
                {
                    activeWindow.ActiveWindowChanged -= ProxyOnActiveWindowChanged;
                    ActiveWindowHookEnabled = false;
                }
                catch { }
            });
        }

        private static async Task DelayUnhideWindowTask(IntPtr hwnd, int delayMs = 10000)
        {
            Logger.Info("Scheduling 'Unhide Window'");

            await Task.Delay(delayMs);
            var result = WindowMgmt.ShowWindow(hwnd, ShowWindowCmd.SW_SHOWNOACTIVATE);
            Logger.Info($"Result of un-hiding window: {result}");
        }

        private void ProxyOnActiveWindowChanged(object sender, string windowHeader, IntPtr hwnd)
        {
            Logger.Info($"'{nameof(ProxyOnActiveWindowChanged)}' Event - Window Title: '{windowHeader}' - Handle: '{hwnd}'");

            if (windowHeader.EqualsIgnoreCase("launchbox game startup"))
            {
                //var result = WindowMgmt.SetWindowTopMost(hwnd, true);
                //Logger.Info($"Result of 'SetWindowTopMost': {result}");
                //Task.Run(async () =>
                //{
                //    await Task.Delay(12000);
                //    var result2 = WindowMgmt.SetWindowTopMost(hwnd, false);
                //    Logger.Info($"Result of 'SetWindowTopMost' (not topmost): {result2}");
                //});

                //WindowMgmt.LockForegroundWindow(true);
                //Task.Run(async () =>
                //{
                //    await Task.Delay(2000);
                //    ActiveWindow.LockForegroundWindow(false);
                //});
                return;
            }

            //if (windowHeader.ContainsIgnoreCase("steam") && WindowMgmt.GetClassName(hwnd) == "vguiPopupWindow")
            if (windowHeader == "Steam" || windowHeader.EndsWith("- Steam"))
            {
                //Logger.Info("Steam window detected on top");
                //var result = ActiveWindow.ShowWindow(LbGameStartupWindowHandle, ActiveWindow.ShowWindowType.Hide);
                //Logger.Info($"Result of hiding Lb Game Startup window: {result}");
                //_ = DelayUnhideWindowTask(LbGameStartupWindowHandle, 15000);
                //ActiveWindow.SetForegroundWindow(ProxyWindowHandle);
                //SendSteamWindowToBackground(hwnd);
                return;
            }



            if (windowHeader.EqualsIgnoreCase(Info.ProxyEmulatorTitle))
            {
                _proxyWindowHandle = hwnd;
                //ActiveWindow.ShowWindow(LbGameStartupWindowHandle, ActiveWindow.ShowWindowType.Hide);
                //PrecedeInZOrder(LbGameStartupWindowHandle, LbWindowHandle);
                //WindowMgmt.SetWindowPos(LbWindowHandle,
                //                        LbGameStartupWindowHandle,
                //                        0,
                //                        0,
                //                        0,
                //                        0,
                //                        WindowMgmt.SetWindowPosFlags.SWP_NOMOVE |
                //                        WindowMgmt.SetWindowPosFlags.SWP_NOSIZE);

                WindowMgmt.EnableWindow(LbGameStartupWindowHandle, false);
                WindowMgmt.SetWindowPos(LbGameStartupWindowHandle,
                                        WindowMgmt.HWND_NOTOPMOST,
                                        0,
                                        0,
                                        0,
                                        0,
                                        WindowMgmt.SetWindowPosFlags.SWP_NOMOVE |
                                        WindowMgmt.SetWindowPosFlags.SWP_NOSIZE);

                //WindowMgmt.ShowWindow(_proxyWindowHandle, ShowWindowCmd.SW_FORCEMINIMIZE);
                if (GameWindowHandle != default)
                {
                    //WindowMgmt.SetForegroundWindow(GameWindowHandle);
                    //WindowMgmt.EnableWindow(_proxyWindowHandle, false);
                    WindowMgmt.ShowWindow(_proxyWindowHandle, ShowWindowCmd.SW_FORCEMINIMIZE);
                }

                return;
            }

            if (GameWindowHandle == default)
            {
                GameWindowHandle = hwnd;
                return;
            }

            if (ProcWatcher?.ProcessFileInfo == null)
                return;

            //DisableActiveWindowHook();

            //// Ensures the LaunchBox game startup/shutdown window is at the top of the z order
            //FixLbGameStartupWindowZOrder();

            //ChangeLbStartupWindowVisibility(true);

            //// Remove SteamLauncherProxy.exe's icon from the taskbar
            //Logger.Info("Removing Proxy window from taskbar.");
            //RemoveWindowFromTaskbar(hwnd);

            //// Ensures the game window is shown and that it is the foreground window
            //TryShowWindow(ProcWatcher.ProcessFileInfo);

            //_ = DelayUnhideLbStartupWindowTask();
        }

        /// <summary>
        /// Forces Steam window to the background (bottom of Z-order).
        /// </summary>
        private static void SendSteamWindowToBackground(IntPtr steamWindowHandle = default)
        {
            Logger.Info($"Forcing Steam window into the background because config item '{nameof(Settings.Config.PreventSteamFocusStealing)}' is enabled.");

            if (steamWindowHandle == default)
            {
                //steamWindowHandle = SteamProcessInfo.SteamProcess.MainWindowHandle;
                steamWindowHandle = WindowMgmt.FindWindow("vguiPopupWindow", "Steam");
                if (steamWindowHandle == IntPtr.Zero)
                    return;
            }


            Logger.Info($"Calling '{nameof(WindowMgmt.SetWindowPos)}' on window handle '{steamWindowHandle.ToInt64():X}'.");

            WindowMgmt.SetWindowPos(
                steamWindowHandle,
                WindowMgmt.HWND_BOTTOM,
                0, 0, 0, 0,
                WindowMgmt.SetWindowPosFlags.SWP_NOMOVE |
                WindowMgmt.SetWindowPosFlags.SWP_NOSIZE |
                WindowMgmt.SetWindowPosFlags.SWP_NOACTIVATE |
                WindowMgmt.SetWindowPosFlags.SWP_ASYNCWINDOWPOS);
        }

        /// <summary>
        /// Changes the z order of the LaunchBox startup/shutdown window. If <paramref name="precedeWindowHandle"/> 
        /// value is supplied, it is placed directly before it. If it is not supplied, it is placed at the very top.
        /// </summary>
        /// <param name="precedeWindowHandle">
        /// The window handle that the LB startup/shutdown screen should precede in the z order. 
        /// </param>
        private static void FixLbGameStartupWindowZOrder(IntPtr precedeWindowHandle = default) // default = HWND_TOP
        {
            Logger.Info("Fixing LaunchBox game startup/shutdown window z order.");

            var lbGameOverWindowHandle = WindowMgmt.FindWindow(null, "LaunchBox Game Startup");
            if (lbGameOverWindowHandle == IntPtr.Zero)
            {
                Logger.Error("Could not fix the LaunchBox game startup/shutdown window z order because " +
                             "no valid window handle could be found.");
                return;
            }

            Logger.Info($"Calling SetWindowPos on window handle '{lbGameOverWindowHandle.ToInt64():X}'.");

            var result = WindowMgmt.SetWindowPos(lbGameOverWindowHandle,
                                                 precedeWindowHandle,
                                                 0,
                                                 0,
                                                 0,
                                                 0,
                                                 WindowMgmt.SetWindowPosFlags.SWP_NOMOVE |
                                                 WindowMgmt.SetWindowPosFlags.SWP_NOSIZE |
                                                 WindowMgmt.SetWindowPosFlags.SWP_NOACTIVATE |
                                                 WindowMgmt.SetWindowPosFlags.SWP_NOOWNERZORDER);

            //Logger.Info($"Return value of {nameof(FixLbGameStartupWindowZOrder)}'s call to {nameof(WindowMgmt.SetWindowPos)} (true=successful, false=unsuccessful): {result}");
        }

        /// <summary>
        /// Hides the window associated with the provided handle which also removes its icon from the taskbar.
        /// </summary>
        /// <param name="windowHandle">The handle of the window to hide and remove from the taskbar.</param>
        private static void RemoveWindowFromTaskbar(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero)
            {
                Logger.Error("Could not hide the window and remove it from the taskbar because the provided " +
                             "window handle is invalid.");
                return;
            }

            WindowMgmt.ShowWindow(windowHandle, ShowWindowCmd.SW_HIDE);
        }

        private static void ChangeWindowZOrder(IntPtr topWindowHandle, IntPtr bottomWindowHandle)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var result = WindowMgmt.SetWindowPos(topWindowHandle,
                                                     bottomWindowHandle,
                                                     0, 0, 0, 0,
                                                     WindowMgmt.SetWindowPosFlags.SWP_NOMOVE |
                                                     WindowMgmt.SetWindowPosFlags.SWP_NOSIZE |
                                                     WindowMgmt.SetWindowPosFlags.SWP_NOACTIVATE |
                                                     WindowMgmt.SetWindowPosFlags.SWP_NOOWNERZORDER);

                Logger.Info($"Result of '{nameof(ChangeWindowZOrder)}': {result}");
            });
        }

        private static async Task FixWindowOrder()
        {
            Logger.Info($"Begin");
            await Task.Delay(new TimeSpan(0, 0, 2));
            Logger.Info("After delay");
            var lbGameStartupWindowHandle = WindowMgmt.FindWindow(null, "LaunchBox Game Startup");
            var proxyWindowHandle = WindowMgmt.FindWindow(null, Info.ProxyEmulatorTitle.ToLower());
            var lbMainWindowHandle = Info.LaunchBoxProcess.MainWindowHandle;
            var lbWindowHandle = WindowMgmt.FindWindow(null, "LaunchBox Big Box");
            if (proxyWindowHandle != default && lbGameStartupWindowHandle != default)
            {
                ChangeWindowZOrder(lbGameStartupWindowHandle, proxyWindowHandle);
            }

            //if (lbWindowHandle != default && lbGameStartupWindowHandle != default)
            //{
            //    var result = PrecedeInZOrder(lbMainWindowHandle, lbGameStartupWindowHandle);
            //    Logger.Info($"Result of 'PrecedeInZOrder': {result}");
            //}
            Logger.Info($"End");
        }

        private static async Task DelayUnhideLbStartupWindowTask()
        {
            Logger.Info("Scheduling 'Show LB startup window'");

            await Task.Delay(new TimeSpan(0, 0, 2));
            ChangeLbStartupWindowVisibility();
        }

        private static void ChangeLbStartupWindowVisibility(bool hide = false)
        {
            var lbGameOverWindowHandle = WindowMgmt.FindWindow(null, "LaunchBox Game Startup");
            if (lbGameOverWindowHandle == IntPtr.Zero)
            {
                Logger.Error("Could not change the visibility of the LaunchBox game startup/shutdown window. " +
                             "No valid window handle could be found.");
                return;
            }

            Logger.Info($"Calling ShowWindow on window handle '{lbGameOverWindowHandle.ToInt64():X}'.");

            WindowMgmt.ShowWindow(lbGameOverWindowHandle,
                                    hide ? ShowWindowCmd.SW_HIDE
                                        : ShowWindowCmd.SW_SHOWNOACTIVATE);
        }

        /// <summary>
        /// Used to "Show" or "Foreground" the main window of the first process found matching the file info provided.
        /// </summary>
        /// <param name="findProcInfo">Defines the executable to look for in the list of running processes.</param>
        private static IntPtr TryShowWindow(FileInfo findProcInfo)
        {
            try
            {
                var proc = findProcInfo.GetActiveProcesses().First();
                var gameWindowHandle = proc.MainWindowHandle;

                Logger.Info($"Main game window handle found for '{findProcInfo.Name}' (PID: {proc.Id}).");

                // Ensures the game window is shown and that it is the foreground window
                TryShowWindow(gameWindowHandle);

                return gameWindowHandle;
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case Win32Exception _:
                        Logger.Error($"Failed to retrieve the game/emulator process to show the main game " +
                                     $"window: {ex.Message}");
                        break;
                    case ArgumentNullException _:
                    case InvalidOperationException _:
                        Logger.Error($"Failed to show the main game window due to an invalid or missing " +
                                     $"window handle: {ex.Message}");
                        break;
                    default:
                        Logger.Error($"Failed to show the main game window: {ex.Message}");
                        break;
                }
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Used to "Show" or "Foreground" the window associated with the provided handle.
        /// </summary>
        /// <param name="windowHandle">The handle of the window to show.</param>
        private static bool TryShowWindow(IntPtr windowHandle)
        {
            var result = false;
            try
            {
                if (windowHandle == IntPtr.Zero)
                    throw new ArgumentNullException(nameof(windowHandle));

                Logger.Info($"Showing the window with handle '{windowHandle.ToInt64():X}'.");

                //ActiveWindow.SetActiveWindow(windowHandle);

                //Thread.Sleep(500);

                if (WindowMgmt.GetForegroundWindow() != windowHandle)
                {
                    Logger.Warning("Not in foreground - Running SetForegroundWindow");
                    // Except for special cases, this will bring the majority of windows into the foreground
                    result = WindowMgmt.SetForegroundWindow(windowHandle);
                }


                if (WindowMgmt.GetForegroundWindow() != windowHandle)
                {
                    Logger.Warning("Not in foreground - Running ShowWindow");
                    // Some borderless/fullscreen windows minimize themselves when focus is lost and 
                    // 'SetForegroundWindow' will not show the window so we also use this in case.
                    result = WindowMgmt.ShowWindow(windowHandle, ShowWindowCmd.SW_SHOWMAXIMIZED);
                }

                //ActiveWindow.SetActiveWindow(windowHandle);
                //ActiveWindow.ShowWindow(windowHandle, ActiveWindow.ShowWindowType.ShowMaximized);

                //Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case ArgumentNullException _:
                    case InvalidOperationException _:
                        Logger.Error($"Failed to show the main game window due to an invalid or missing " +
                                     $"window handle: {ex.Message}");
                        break;
                    default:
                        Logger.Error($"Failed to show the main game window: {ex.Message}");
                        break;
                }
            }

            return result;
        }

        public static List<Process> GetRunningProxyProcesses()
        {
            var proxyProcs = new List<Process>();
            proxyProcs.AddRange(Process.GetProcessesByName(Info.ProxyEmulatorTitle));

            return proxyProcs;
        }

        /// <summary>
        /// Kills all processes whose main window caption is "SteamLauncherProxy", regardless of whether the window is
        /// hidden or if the proxy exe file has been renamed (such as when the proxy impersonates DOSBox/ScummVM).
        /// </summary>
        public static void KillProxyProcesses()
        {
            var procs = ActiveWindow.FindProcsWithMainWindowCaption(Info.ProxyEmulatorTitle, 500)
                .Where(proc => !proc.HasExited);
            foreach (var proc in procs)
            {
                proc.Kill();
            }
        }

        private static string GetProxyArgString()
        {
            //var proxyProc = Process.GetProcessesByName(ProxyEmulatorTitle)?[0];

            var proxyProc = ActiveWindow.FindProcsWithMainWindowCaption(Info.ProxyEmulatorTitle, 500)
                .FirstOrDefault(proc => !proc.HasExited);
            if (proxyProc == null)
                throw new Exception("Failed to get proxy arguments because no proxy process was found.");

            ProcessCommandLine.Retrieve(proxyProc, out var cmdLineArgs);
            //var argList = ProcessCommandLine.CommandLineToArgs(cmdLineArgs);
            var argStr = ProcessCommandLine.CommandLineToArgsString(cmdLineArgs);

            return argStr;

            //var cmdLine = proxyProc.GetCommandLineOrDefault();
            //if (string.IsNullOrEmpty(cmdLine))
            //    throw new Exception("Failed to get proxy arguments string from SteamLauncherProxy.");

            //return Utilities.GetArgStringFromCmdLine(cmdLine);
        }

        /// <summary>
        /// Restores the original application paths to any game/emulators whose paths were previously 
        /// modified to point to the proxy executable.
        /// </summary>
        public static void RestoreAppPaths()
        {
            if (Settings.Repairs.DosBoxExeNeedsRepair)
            {
                try
                {
                    RestoreOriginalDosBoxExe();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to restore original DOSBox exe: {ex.Message}");
                }
            }

            if (Settings.Repairs.ScummVmExeNeedsRepair)
            {
                try
                {
                    RestoreOriginalScummVmExe();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to restore original ScummVM exe: {ex.Message}");
                }
            }

            if (Settings.Repairs.RepairPaths == null || Settings.Repairs.RepairPaths?.Count <= 0)
                return;

            Logger.Info("Repairing application paths...");

            // Used to keep track of which items should be removed from 'RepairPaths' in the state data file
            var pathsToRemove = new List<RepairPath>();

            foreach (var repairPath in Settings.Repairs.RepairPaths)
            {
                var emuOrGame = repairPath.GetEmuOrGameObj();
                if (emuOrGame == null)
                {
                    // No emulator or game exists with that ID so queue it for removal from the state data file
                    pathsToRemove.Add(repairPath);
                    continue;
                }

                var repairFilename = Path.GetFileName(repairPath.Path);

                // Ensure that the proxy exe or the steam exe do not get restored as the original app path
                // (as these should never be swapped with the proxy path in the 1st place)
                if (repairFilename == null || repairFilename.ToLower().Contains(Info.ProxyEmulatorTitle.ToLower()) ||
                    repairFilename.EqualsIgnoreCase("steam.exe"))
                {
                    // It is an invalid repair path so queue it for removal from the state data file
                    pathsToRemove.Add(repairPath);
                    continue;
                }

                var pathBeforeRepair = emuOrGame.ApplicationPath;

                // Restore the path to its correct value
                emuOrGame.ApplicationPath = repairPath.Path;

                // Verify that the path was actually changed
                if (emuOrGame.ApplicationPath != repairPath.Path)
                {
                    Logger.Error($"Failed to repair path! Title: '{emuOrGame.Title}' - Path to be restored: " +
                                 $"'{repairPath.Path}' - Actual path: '{emuOrGame.ApplicationPath}'");
                    continue;
                }

                Logger.Info($"Successfully repaired path! Title: '{emuOrGame.Title}' - Path before repair: " +
                            $"'{pathBeforeRepair}' - Path after repair: '{emuOrGame.ApplicationPath}'");

                // Queue for removal from state data file since we verified the path was actually repaired
                pathsToRemove.Add(repairPath);
            }

            // Remove any paths that were queued for removal from the state data file
            pathsToRemove.ForEach(x => Settings.Repairs.RepairPaths.Remove(x));

            // Save the state data file immediately so these changes are not lost if a crash occurs
            Settings.Repairs.Save();
        }

        /// <summary>
        /// Modifies the LB game/emulator application path to point to the proxy executable, but it
        /// first backs up the original path to the config file so it can be restored later on.
        /// Note: In most cases, the application path will only point to the proxy for a matter of 
        /// seconds (the period between 'OnBeforeGameLaunching' and 'OnAfterGameLaunched').
        /// </summary>
        public void SetLauncherExeToProxy()
        {
            if (Launcher == null)
                throw new Exception("The Launcher variable is null.");

            // Check if game is using DOSBox
            if (Launcher is IGame && Launcher.UseDosBox)
            {
                Logger.Info($"The game '{Launcher.Title}' uses DOSBox.");

                // Check if DOSBox exe still needs to be repaired due to a crash; if so, restore
                if (Settings.Repairs.DosBoxExeNeedsRepair)
                {
                    RestoreAppPaths();

                    // Ensure it was actually restored
                    if (Settings.Repairs.DosBoxExeNeedsRepair)
                        throw new Exception("Attempts to restore the original DOSBox exe failed.");
                }

                try
                {
                    ReplaceDosBoxExeWithProxy();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to replace DOSBox exe with proxy: {ex.Message}");
                }

                return;
            }

            // Check if game is using ScummVM
            if (Launcher is IGame && Launcher.UseScummVm)
            {
                Logger.Info($"The game '{Launcher.Title}' uses ScummVM.");

                // Check if ScummVM exe still needs to be repaired due to a crash; if so, restore
                if (Settings.Repairs.ScummVmExeNeedsRepair)
                {
                    RestoreAppPaths();

                    // Ensure it was actually restored
                    if (Settings.Repairs.ScummVmExeNeedsRepair)
                        throw new Exception("Attempts to restore the original ScummVM exe failed.");
                }

                try
                {
                    ReplaceScummVmExeWithProxy();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to replace ScummVM exe with proxy: {ex.Message}");
                }

                return;
            }

            if (String.IsNullOrEmpty(Launcher.ApplicationPath))
                throw new Exception("The Launcher application path is null or empty.");

            // Do NOT change to 'var' bc it will be a dynamic type & custom string extension methods won't work
            string launcherFilename = Path.GetFileName(Launcher.ApplicationPath);

            if (launcherFilename == null)
                throw new Exception("The Launcher application path does not point to a valid filename.");

            if (launcherFilename.EqualsIgnoreCase("steam.exe"))
                throw new Exception("The Launcher application path points to steam.exe.");

            if (Launcher.ApplicationPath.ToLower().StartsWith("steam:"))
                throw new Exception("The Launcher application path points to a Steam URL.");

            // Check if app path already points to proxy exe due to a crash; if so, run restore
            if (launcherFilename.ToLower().Contains(Info.ProxyEmulatorTitle.ToLower()))
            {
                RestoreAppPaths();
                launcherFilename = Path.GetFileName(Launcher.ApplicationPath);

                // Check app path again after restore to be sure it was correctly restored
                if (launcherFilename == null || launcherFilename.ToLower().Contains(Info.ProxyEmulatorTitle.ToLower()))
                    throw new Exception("The Launcher application path points to the proxy exe and " +
                                        "could not be restored to its original value.");
            }

            var repairPath = new RepairPath()
            {
                Id = Launcher.Id,
                Path = Launcher.ApplicationPath,
                IdType = Launcher is IEmulator ? IdType.Emulator : IdType.Game,
            };

            Logger.Info(
                $"Adding a RepairPath to the state file - Id: {repairPath.Id} - " + $"Path: '{repairPath.Path}'");
            Settings.Repairs.RepairPaths.Add(repairPath);
            Settings.Repairs.Save();

            Launcher.ApplicationPath = Info.SteamLauncherProxyExeRelativePath;
            Logger.Info($"Set '{Launcher.Title}' application path to the proxy executable.");
        }

        private static void ReplaceDosBoxExeWithProxy()
        {
            Logger.Info("Replacing DOSBox exe with the proxy.");

            // Make permanent backup of original DOSBox exe in case it needs to be manually restored at some point
            Logger.Info($"Attempting to create a permanent backup of the DOSBox exe at " +
                        $"'{Info.DosBoxExePermanentBackupPath}'.");

            if (Utilities.CreateFileBackup(Info.DosBoxExePath, Info.DosBoxExePermanentBackupPath))
                Logger.Info(
                    $"Permanent backup of DosBox EXE created successfully at '{Info.DosBoxExePermanentBackupPath}'.");
            else
                Logger.Info("Permanent backup of DOSBox exe already exists.");

            // Temporarily backup/rename the original DOSBox exe
            Logger.Info($"Temporarily backing up the original DOSBox exe by renaming " +
                        $"'{Info.DosBoxExePath}' to '{Info.DosBoxExeBackupPath}'.");
            File.Move(Info.DosBoxExePath, Info.DosBoxExeBackupPath);

            // Copy proxy to DOSBox dir and rename to DOSBox.exe
            Logger.Info($"Copying proxy exe to pose as DOSBox exe by copying '{Info.SteamLauncherProxyExePath}' to " +
                        $"'{Info.DosBoxExePath}'.");
            File.Copy(Info.SteamLauncherProxyExePath, Info.DosBoxExePath);

            // Set the repair flag in the state data file
            Logger.Info("Setting the DOSBox repair flag in the state data file.");
            Settings.Repairs.DosBoxExeNeedsRepair = true;
            Settings.Repairs.Save();

            Logger.Info("Successfully replaced the DOSBox exe with the proxy.");
        }

        private static void RestoreOriginalDosBoxExe()
        {
            Logger.Info("Restoring DOSBox exe with its backup.");

            // Sleep briefly so plugin does not "get ahead" of the file system
            Thread.Sleep(Settings.Config.DosBoxScummVmProxySleepMs);

            // Delete the proxy exe previously queued for deletion
            if (File.Exists(Info.DosBoxExeQueuedToDeletePath))
            {
                Logger.Info($"Deleting the proxy exe (posing as DOSBox) that was previously queued for " +
                            $"deletion: '{Info.DosBoxExeQueuedToDeletePath}'");
                File.Delete(Info.DosBoxExeQueuedToDeletePath);
            }

            // Queue for deletion the proxy exe posing as DOSBox
            if (File.Exists(Info.DosBoxExePath))
            {
                Logger.Info($"Queuing for deletion the proxy exe posing as DOSBox by renaming " +
                            $"'{Info.DosBoxExePath}' to '{Info.DosBoxExeQueuedToDeletePath}'.");
                File.Move(Info.DosBoxExePath, Info.DosBoxExeQueuedToDeletePath);
            }

            // Restore the temporary backup by renaming it back to its original name
            Logger.Info($"Restoring the DOSBox temporary backup by renaming '{Info.DosBoxExeBackupPath}' " +
                        $"to '{Info.DosBoxExePath}'.");
            File.Move(Info.DosBoxExeBackupPath, Info.DosBoxExePath);

            // Reset the repair flag in the state data file
            Logger.Info("Resetting the DOSBox repair flag in the state data file.");
            Settings.Repairs.DosBoxExeNeedsRepair = false;
            Settings.Repairs.Save();

            Logger.Info("Successfully restored the original DOSBox exe.");
        }

        private static void ReplaceScummVmExeWithProxy()
        {
            Logger.Info("Replacing ScummVM exe with the proxy.");

            // Make permanent backup of original ScummVM exe in case it needs to be manually restored at some point
            if (!File.Exists(Info.ScummVmExePermanentBackupPath))
            {
                Logger.Info($"Creating a permanent backup of the ScummVM exe at " +
                            $"'{Info.ScummVmExePermanentBackupPath}'.");
                File.Copy(Info.ScummVmExePath, Info.ScummVmExePermanentBackupPath);
            }

            // Temporarily backup/rename the original ScummVM exe
            Logger.Info($"Temporarily backing up the original ScummVM exe by renaming " +
                        $"'{Info.ScummVmExePath}' to '{Info.ScummVmExeBackupPath}'.");
            File.Move(Info.ScummVmExePath, Info.ScummVmExeBackupPath);

            // Copy proxy to ScummVM dir and rename to scummvm.exe
            Logger.Info($"Copying proxy exe to pose as ScummVM exe by copying '{Info.SteamLauncherProxyExePath}' to " +
                        $"'{Info.ScummVmExePath}'.");
            File.Copy(Info.SteamLauncherProxyExePath, Info.ScummVmExePath);

            // Set the repair flag in the state data file
            Logger.Info("Setting the ScummVM repair flag in the state data file.");
            Settings.Repairs.ScummVmExeNeedsRepair = true;
            Settings.Repairs.Save();

            Logger.Info("Successfully replaced the ScummVM exe with the proxy.");
        }

        private static void RestoreOriginalScummVmExe()
        {
            Logger.Info("Restoring ScummVM exe with its backup.");

            // Sleep briefly so plugin does not "get ahead" of the file system
            Thread.Sleep(Settings.Config.DosBoxScummVmProxySleepMs);

            // Delete the proxy exe previously queued for deletion
            if (File.Exists(Info.ScummVmExeQueuedToDeletePath))
            {
                Logger.Info($"Deleting the proxy exe (posing as ScummVM) that was previously queued for " +
                            $"deletion: '{Info.ScummVmExeQueuedToDeletePath}'");
                File.Delete(Info.ScummVmExeQueuedToDeletePath);
            }

            // Queue for deletion the proxy exe posing as ScummVM
            if (File.Exists(Info.ScummVmExePath))
            {
                Logger.Info($"Queuing for deletion the proxy exe posing as ScummVM by renaming " +
                            $"'{Info.ScummVmExePath}' to '{Info.ScummVmExeQueuedToDeletePath}'.");
                File.Move(Info.ScummVmExePath, Info.ScummVmExeQueuedToDeletePath);
            }

            // Restore the temporary backup by renaming it back to its original name
            Logger.Info($"Restoring the ScummVM temporary backup by renaming '{Info.ScummVmExeBackupPath}' " +
                        $"to '{Info.ScummVmExePath}'.");
            File.Move(Info.ScummVmExeBackupPath, Info.ScummVmExePath);

            // Reset the repair flag in the state data file
            Logger.Info("Resetting the ScummVM repair flag in the config file.");
            Settings.Repairs.ScummVmExeNeedsRepair = false;
            Settings.Repairs.Save();

            Logger.Info("Successfully restored the original ScummVM exe.");
        }

        //private static string ExtractRomPathFromProxyCmdLine()
        //{
        //    var proxyProc = Utilities.GetSteamLauncherProxyProcesses()?[0];
        //    var cmdLine = proxyProc.GetCommandLineOrDefault();
        //    if (cmdLine == null)
        //        return null;

        //    var args = Utilities.CommandLineToArgs(cmdLine);
        //    foreach (var arg in args)
        //    {
        //        if (!arg.ToLower().Contains(@"7-zip"))
        //            continue;

        //        var dirName = Path.GetDirectoryName(arg.Trim('"'));
        //        if (string.IsNullOrEmpty(dirName))
        //            continue;

        //        dirName = dirName.ToLower();
        //        if (dirName.EndsWith(@"7-zip\temp") || dirName.EndsWith(@"7-zip/temp"))
        //            return arg.Trim('"');
        //    }

        //    return null;
        //}
    }
}
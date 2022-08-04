using System;
using System.Collections.Generic;
using Microsoft.Win32;
using SteamLauncher.SteamClient.Native;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SteamLauncher.Logging;
using SteamLauncher.Tools;

namespace SteamLauncher.SteamClient
{
    public static class SteamProcessInfo
    {
        /// <summary>
        /// The path where Steam is currently installed; null if Steam is not installed.
        /// </summary>
        public static string SteamInstallPath { get; } = GetSteamInstallPath();

        /// <summary>
        /// The path to the Steam Client DLL (steamclient.dll or steamclient64.dll); null if Steam is not installed.
        /// </summary>
        public static string SteamClientDllPath { get; } = GetSteamClientDllPath();

        /// <summary>
        /// The path to the Steam executable; null if Steam is not installed.
        /// </summary>
        public static string SteamExePath { get; } = GetSteamExePath();

        /// <summary>
        /// Defines whether Steam is currently installed.
        /// </summary>
        public static bool IsSteamInstalled { get; } = GetIsSteamInstalled();

        /// <summary>
        /// Checks if the beta version of the Steam client is installed.
        /// </summary>
        public static bool IsRunningBetaClient { get; } = GetIsRunningBetaClient();

        /// <summary>
        /// Gets the ID of the currently active Steam user.
        /// </summary>
        public static int ActiveUserId => GetActiveUserId();

        /// <summary>
        /// Gets the shortcuts.vdf path for the currently active Steam user.
        /// </summary>
        public static string ShortcutsVdfPath => GetShortcutsVdfPath();

        /// <summary>
        /// Checks if the Steam process is running with elevated permissions.
        /// </summary>
        public static bool IsElevated => SteamProcessStatus == SteamStatus.RunningElevated;

        /// <summary>
        /// Provides both a label and color indicating the current status of the Steam process.
        /// </summary>
        public static SteamStatus SteamProcessStatus { get; private set; } = SteamStatus.Unknown;


        public static bool IsRunning => SteamProcessStatus == SteamStatus.Running || 
                                             SteamProcessStatus == SteamStatus.RunningElevated;

        /// <summary>
        /// Gets the Steam process ID from the registry; returns 0 if Steam isn't running or isn't installed.
        /// </summary>
        /// <returns>Steam's process ID if it is running; otherwise, 0.</returns>
        public static int GetSteamPid()
        {
            if (!IsSteamInstalled)
                return 0;

            const string pidRegKey = @"HKEY_CURRENT_USER\Software\Valve\Steam\ActiveProcess";
            const string pidRegName = "pid";

            Logger.Info($"Attempting to retrieve Steam PID from registry value named '{pidRegName}' inside the key '{pidRegKey}'.");

            if (!(Registry.GetValue(pidRegKey, pidRegName, null) is int pid))
            {
                Logger.Warning($"The Steam PID could not be retrieved because the registry value '{pidRegName}' or registry key '{pidRegKey}' was invalid or did not exist.");
                return 0;
            }

            Logger.Info($"The Steam PID value retrieved from the registry is '{pid}'.");
            return pid;
        }

        private static Process _steamProcess = null;

        //public static Process SteamProcess
        //{
        //    get
        //    {
        //        UpdateSteamStatus();
        //        return _steamProcess;
        //    }
        //}

        ///// <summary>
        ///// Gets a running instance of the Steam process if available, otherwise it automatically starts it.
        ///// </summary>
        //public static Process SteamProcess
        //{                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
        //    get
        //    {
        //        // If Steam is not installed, return null
        //        if (!IsSteamInstalled)
        //        {
        //            Logger.Error("Steam installation not found!");
        //            return null;
        //        }

        //        // If we already have a valid SteamProcess cached and it is still running, use it
        //        if (_steamProcess != null && _steamProcess.Responding)
        //            return _steamProcess;

        //        // Get Steam process id from registry
        //        var pid = GetSteamPid();

        //        // If Steam PID is greater than 0, it's either running, or exited abnormally
        //        if (pid > 0)
        //        {
        //            // Check if a process with that PID is actually running
        //            var p = Process.GetProcesses().SingleOrDefault(x => x.Id == pid);

        //            // If running & process is responding, cache and return it
        //            // NOTE: Use p.Responding instead of !p.HasExited bc if Steam is elevated & LB isn't, an exception is thrown
        //            if (p != null && p.ProcessName.ToLower().Contains("steam") && p.Responding)
        //            {
        //                Logger.Info("Steam is already running.");
        //                _steamProcess = p;
        //                return _steamProcess;
        //            }
        //        }

        //        Logger.Info("Steam is not running. Attempting to launch a new Steam process.");

        //        // Steam is not running, so we start the Steam process and wait for it to load
        //        _steamProcess = Process.Start(SteamExePath);
        //        if (_steamProcess == null)
        //        {
        //            Logger.Error("An unexpected error occurred while attempting to start the Steam process.");
        //            return null;
        //        }

        //        // This can prob be removed as the Thread.Sleep+WaitOnSteamUpdate seem to add enough waiting for even extreme circumstances
        //        // NOTE: Check if Steam is elevated, bc if it is & LB isn't, an exception is thrown on calling WaitForInputIdle
        //        if (!UacHelper.IsProcessElevated(_steamProcess))
        //            _steamProcess.WaitForInputIdle(10000); 

        //        // Allow time for the Steam update dialog to appear before calling 'WaitOnSteamUpdate'
        //        System.Threading.Thread.Sleep(2000);

        //        if (_steamProcess.Responding)
        //            Logger.Info($"Successfully started a new Steam process.");
                
        //        // Checks if more wait time is needed due to Steam updating
        //        WaitOnSteamUpdate(_steamProcess);

        //        System.Threading.Thread.Sleep(3000);
        //        return _steamProcess;
        //    }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
        //}

        /// <summary>
        /// Updates <see cref="SteamProcessStatus"/> and modifies the <see cref="SteamProcess"/> property if it is no
        /// longer valid.
        /// </summary>
        public static void UpdateSteamStatus()
        {
            try
            {
                // If Steam is not installed, it cannot be running
                if (!IsSteamInstalled)
                {
                    Logger.Error("Steam installation not found!");
                    SteamProcessStatus = SteamStatus.Uninstalled;
                    _steamProcess = null;
                    return;
                }

                // Check if we already have a valid SteamProcess cached and if it is still running
                if (_steamProcess != null && _steamProcess.IsActive() && _steamProcess.Responding)
                {
                    SteamProcessStatus = SteamStatus.Running;
                    if (_steamProcess.IsElevated())
                        SteamProcessStatus = SteamStatus.RunningElevated;

                    return;
                }

                // Get Steam process id from registry
                var pid = GetSteamPid();

                // If Steam PID is greater than 0, it's either running, or exited abnormally
                if (pid > 0)
                {
                    // Check if a process with that PID is actually running
                    var p = Process.GetProcesses().SingleOrDefault(x => x.Id == pid);

                    // If running & process is responding, cache and return it
                    // NOTE: Use p.Responding instead of !p.HasExited bc if Steam is elevated & LB isn't, an exception is thrown
                    if (p != null && 
                        p.ProcessName.ToLower().Equals("steam") && 
                        p.IsActive() && 
                        p.Responding)
                    {
                        _steamProcess = p;
                        SteamProcessStatus = SteamStatus.Running;
                        if (_steamProcess.IsElevated())
                            SteamProcessStatus = SteamStatus.RunningElevated;

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An unknown error occurred while updating the Steam process status: {ex}");
                SteamProcessStatus = SteamStatus.Unknown;
                return;
            }

            SteamProcessStatus = SteamStatus.Stopped;
            _steamProcess = null;
        }

        /// <summary>
        /// Gets the Steam Process object synchronously. Warning: This function is blocking and should only be used
        /// when it is not possible to use the async version. Do not use on threads that update the UI.
        /// </summary>
        /// <returns>A Process object representing the running Steam process.</returns>
        public static Process GetSteamProcess()
        {
            return GetSteamProcessAsync().GetAwaiter().GetResult();
        }

        public static async Task<Process> GetSteamProcessAsync()
        {
            await StartSteamAsync();
            return _steamProcess;
        }

        /// <summary>
        /// Asynchronously starts the Steam process if it is not already running.
        /// </summary>
        /// <returns></returns>
        public static async Task StartSteamAsync()
        {
            UpdateSteamStatus();
            if (IsRunning || !IsSteamInstalled)
                return;

            Logger.Info("Steam is not running. Attempting to launch a new Steam process.");
            SteamProcessStatus = SteamStatus.Starting;

            // Steam is not running, so we start the Steam process and wait for it to load
            _steamProcess = Process.Start(SteamExePath);
            if (_steamProcess == null)
            {
                Logger.Error("An unexpected error occurred while attempting to start the Steam process.");
                SteamProcessStatus = SteamStatus.Stopped;
                throw new InvalidOperationException("Could not start Steam process");
            }

            // This can prob be removed as the Thread.Sleep+WaitOnSteamUpdate seem to add enough waiting for even extreme circumstances
            // NOTE: Check if Steam is elevated, bc if it is & LB isn't, an exception is thrown on calling WaitForInputIdle
            if (!_steamProcess.IsElevated())
                _steamProcess.WaitForInputIdle(10000);

            // Allow time for the Steam update dialog to appear before calling 'WaitOnSteamUpdate'
            await Task.Delay(2000);

            if (_steamProcess.IsActive() && _steamProcess.Responding)
            {
                Logger.Info($"Successfully started a new Steam process.");
                SteamProcessStatus = SteamStatus.Running;
                if (_steamProcess.IsElevated())
                    SteamProcessStatus = SteamStatus.RunningElevated;
            }

            // Checks if more wait time is needed due to Steam updating
            var updated = await WaitOnSteamUpdateAsync(_steamProcess);

            if (updated)
                await Task.Delay(3000);
        }

        /// <summary>
        /// To be run immediately after starting a new Steam process to wait until complete if a Steam update is underway.
        /// </summary>
        /// <param name="steamProcess">The new Steam process that was just started.</param>
        private static async Task<bool> WaitOnSteamUpdateAsync(Process steamProcess)
        {
            Logger.Info("Checking if the Steam client is in the process of updating...");
            await Task.Delay(2000);

            try
            {
                var mainWindowHandle = steamProcess.MainWindowHandle;
                if (mainWindowHandle == IntPtr.Zero)
                {
                    UpdateSteamStatus();
                    return false;
                }

                var sb = new StringBuilder(25);
                var loopCounter = 0;

                var hStatic = WindowMgmt.FindWindowEx(mainWindowHandle, IntPtr.Zero, "Static", null);

                while (hStatic != IntPtr.Zero)
                {
                    SysNative.SendMessage(hStatic, SysNative.WM_GETTEXT, sb.Capacity, sb);
                    // ReSharper disable once StringLiteralTypo
                    if (sb.Length == 0 || !sb.ToString().Contains("updat"))
                        break;

                    await Task.Delay(1000);
                    if (loopCounter == 0)
                    {
                        Logger.Info("A Steam client update is in progress; waiting for completion.");
                        SteamProcessStatus = SteamStatus.Updating;
                    }

                    loopCounter++;
                    sb.Clear();
                }

                if (loopCounter > 0)
                {
                    Logger.Info("The Steam client update is complete; continuing program execution.");
                    SteamProcessStatus = SteamStatus.Running;
                    if (_steamProcess.IsElevated())
                        SteamProcessStatus = SteamStatus.RunningElevated;

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"The following exception occurred while attempting to detect if a Steam client update was in progress: {ex.Message}");
                UpdateSteamStatus();
            }

            return false;
        }

        /// <summary>
        /// Retrieves the Steam installation path from the registry or returns null if not found.
        /// </summary>
        /// <returns>A string containing the Steam installation path; if not found, returns null.</returns>
        private static string GetSteamInstallPath()
        {
            string installPath;
            if (SysNative.Is64Bit())
                installPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\WOW6432Node\Valve\Steam", "InstallPath", null) as string;
            else
                installPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Valve\Steam", "InstallPath", null) as string;

            if (string.IsNullOrEmpty(installPath))
                installPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;

            if (string.IsNullOrEmpty(installPath))
            {
                const string errorMsg = "No valid Steam installation path could be found.";
                Logger.Error(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            installPath = installPath.Replace("/", @"\");
            Logger.Info($"Steam installation path: '{installPath}'");

            return installPath;
        }

        /// <summary>
        /// Gets the path of the Steam Client DLL.
        /// </summary>
        /// <returns>A string containing the Steam Client DLL path; if not found, returns null.</returns>
        private static string GetSteamClientDllPath()
        {
            if (string.IsNullOrWhiteSpace(SteamInstallPath))
                return null;

            var dllPath = Path.Combine(SteamInstallPath, SysNative.Is64Bit() ? "steamclient64.dll" : "steamclient.dll");
            Logger.Info($"Steam Client DLL path: '{dllPath}'");

            return dllPath;
        }

        /// <summary>
        /// Gets the path of the Steam executable.
        /// </summary>
        /// <returns>A string containing the Steam executable path; if not found, returns null.</returns>
        private static string GetSteamExePath()
        {
            if (string.IsNullOrWhiteSpace(SteamInstallPath))
                return null;

            var exePath = Path.Combine(SteamInstallPath, "Steam.exe");
            Logger.Info($"Steam EXE path: '{exePath}'");

            return exePath;
        }

        /// <summary>
        /// Verifies that Steam is installed.
        /// </summary>
        /// <returns>true if a valid Steam installation is found; otherwise, false;</returns>
        private static bool GetIsSteamInstalled()
        {
            if (string.IsNullOrWhiteSpace(SteamClientDllPath) || string.IsNullOrWhiteSpace(SteamExePath))
                return false;

            return File.Exists(SteamClientDllPath) && File.Exists(SteamExePath);
        }

        /// <summary>
        /// Checks if the beta version of the Steam client is installed.
        /// </summary>
        /// <returns>true if the beta version if installed; otherwise, false</returns>
        private static bool GetIsRunningBetaClient()
        {
            var regPath = @"HKEY_LOCAL_MACHINE\Software\WOW6432Node\Valve\Steam";
            if (!SysNative.Is64Bit())
                regPath = @"HKEY_LOCAL_MACHINE\Software\Valve\Steam";
            
            var betaName = (Registry.GetValue(regPath, "BetaName", null) as string)?.ToLower();
            if (betaName == null)
            {
                Logger.Warning($"Could not determine if user is running beta version of Steam (assuming 'No').");
                return false;
            }

            if (betaName == "")
            {
                Logger.Info($"User is running the NON-beta Steam client.");
                return false;
            }

            if (betaName == "publicbeta")
            {
                Logger.Info($"User is running the BETA Steam client.");
                return true;
            }

            Logger.Info($"User is running an unrecognized BETA Steam client: '{betaName}'.");
            return true;
        }

        /// <summary>
        /// Gets the ID of the currently active Steam user.
        /// </summary>
        /// <returns>The Steam Active UserId as an int value.</returns>
        private static int GetActiveUserId()
        {
            if (!(Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam\ActiveProcess", "ActiveUser", null) is int userId))
                throw new Exception("No Active UserId found in the registry.");

            return userId;
        }

        /// <summary>
        /// Gets the shortcuts.vdf path for the currently active Steam user.
        /// </summary>
        /// <returns>A string containing the path to the shortcuts file.</returns>
        private static string GetShortcutsVdfPath()
        {
            return Path.Combine(SteamInstallPath, "userdata", ActiveUserId.ToString(), "config", "shortcuts.vdf");
        }
    }

    public class SteamStatus
    {
        public static SteamStatus Unknown { get; } = new SteamStatus(nameof(Unknown), Brushes.Orange);

        public static SteamStatus Running { get; } = new SteamStatus(nameof(Running), Brushes.LimeGreen);

        public static SteamStatus RunningElevated { get; } = new SteamStatus("Running (Elevated)", Brushes.Lime);

        public static SteamStatus Stopped { get; } = new SteamStatus(nameof(Stopped), Brushes.Red);

        public static SteamStatus Starting { get; } = new SteamStatus(nameof(Starting), Brushes.Yellow);

        public static SteamStatus Updating { get; } = new SteamStatus(nameof(Updating), Brushes.Blue);

        public static SteamStatus Uninstalled { get; } = new SteamStatus("Not Installed", Brushes.OrangeRed);

        public string Status { get; }

        public SolidColorBrush Color { get; }

        private SteamStatus(string status, SolidColorBrush color)
        {
            Color = color;
            Status = status;
        }

        public static IEnumerable<SteamStatus> List { get; } = new[]
            {Unknown, Running, RunningElevated, Stopped, Starting, Updating, Uninstalled};

        public static SteamStatus FromStatus(string statusString)
        {
            return List.Single(s => string.Equals(s.Status, statusString, StringComparison.OrdinalIgnoreCase));
        }

        public static SteamStatus FromColor(SolidColorBrush color)
        {
            return List.Single(c => c.Color == color);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Status, Color);
        }

        public override bool Equals(object obj)
        {
            return obj is SteamStatus status && Equals(status);
        }

        public bool Equals(SteamStatus steamStatus)
        {
            if (steamStatus is null)
                return false;

            if (ReferenceEquals(this, steamStatus))
                return true;

            return string.Equals(steamStatus.Status, Status) && steamStatus.Color.Color.Equals(Color.Color);
        }

        public static bool operator ==(SteamStatus s1, SteamStatus s2)
        {
            if (s1 is null)
                return s2 is null;

            return s1.Equals(s2);
        }

        public static bool operator !=(SteamStatus s1, SteamStatus s2)
        {
            return !(s1 == s2);
        }
    }
}

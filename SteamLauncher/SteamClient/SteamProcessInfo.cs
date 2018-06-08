using System;
using Microsoft.Win32;
using SteamLauncher.SteamClient.Native;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        /// Gets the Steam process ID from the registry; returns 0 if Steam isn't running or isn't installed.
        /// </summary>
        /// <returns>Steam's process ID if it is running; otherwise, 0.</returns>
        public static int GetSteamPid()
        {
            if (!IsSteamInstalled)
                return 0;

            string pidRegKey;
            string pidRegName;

            if (SysNative.Is64Bit())
            {
                pidRegKey = @"HKEY_CURRENT_USER\Software\Valve\Steam\ActiveProcess";
                pidRegName = "pid";
            }
            else
            {
                pidRegKey = @"HKEY_LOCAL_MACHINE\Software\Valve\Steam";
                pidRegName = "SteamPID";
            }

            Logger.Info($"Attempting to retrieve Steam PID from registry value named '{pidRegName}' inside the key '{pidRegKey}'.");
            var pid = (int)Registry.GetValue(pidRegKey, pidRegName, -1);
            if (pid == -1)
            {
                Logger.Warning($"The Steam PID could not be retrieved because the registry value '{pidRegName}' or registry key '{pidRegKey}' was invalid or did not exist.");
                return 0;
            }

            Logger.Info($"The Steam PID value retrieved from the registry is '{pid}'.");
            return pid;
        }

        private static Process _steamProcess = null;

        /// <summary>
        /// Gets a running instance of the Steam process if available, otherwise it automatically starts it.
        /// </summary>
        public static Process SteamProcess
        {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
            get
            {
                // If Steam is not installed, return null
                if (!IsSteamInstalled)
                {
                    Logger.Error("Steam installation not found!");
                    return null;
                }

                // If we already have a valid SteamProcess cached and it is still running, use it
                if (_steamProcess != null && _steamProcess.Responding)
                    return _steamProcess;

                // Get Steam process id from registry
                int pid = GetSteamPid();

                // If Steam PID is greater than 0, it's either running, or exited abnormally
                if (pid > 0)
                {
                    // Check if a process with that PID is actually running
                    var p = Process.GetProcesses().SingleOrDefault(x => x.Id == pid);

                    // If running & process is responding, cache and return it
                    // NOTE: Use p.Responding instead of !p.HasExited bc if Steam is elevated & LB isn't, an exception is thrown
                    if (p != null && p.Responding)
                    {
                        Logger.Info("Steam is already running.");
                        _steamProcess = p;
                        return _steamProcess;
                    }
                }

                Logger.Info("Steam is not running. Attempting to launch a new Steam process.");

                // Steam is not running, so we start the Steam process and wait for it to load
                _steamProcess = Process.Start(SteamExePath);
                if (_steamProcess == null)
                {
                    Logger.Error("An unexpected error occurred while attempting to start the Steam process.");
                    return null;
                }

                // This can prob be removed as the Thread.Sleep+WaitOnSteamUpdate seem to add enough waiting for even extreme circumstances
                // NOTE: Check if Steam is elevated, bc if it is & LB isn't, an exception is thrown on calling WaitForInputIdle
                if (!UacHelper.IsProcessElevated(_steamProcess))
                    _steamProcess.WaitForInputIdle(10000); 

                // Allow time for the Steam update dialog to appear before calling 'WaitOnSteamUpdate'
                System.Threading.Thread.Sleep(2000);

                if (_steamProcess.Responding)
                    Logger.Info($"Successfully started a new Steam process.");
                
                // Checks if more wait time is needed due to Steam updating
                WaitOnSteamUpdate(_steamProcess);

                System.Threading.Thread.Sleep(3000);
                return _steamProcess;
            }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
        }

        /// <summary>
        /// To be run immediately after starting a new Steam process to wait until complete if a Steam update is underway.
        /// </summary>
        /// <param name="steamProcess">The new Steam process that was just started.</param>
        private static void WaitOnSteamUpdate(Process steamProcess)
        {
            Logger.Info("Checking if the Steam client is in the process of updating...");
            System.Threading.Thread.Sleep(2000);

            try
            {
                var mainWindowHandle = steamProcess.MainWindowHandle;
                if (mainWindowHandle == IntPtr.Zero)
                    return;

                StringBuilder sb = null;
                var loopCounter = 0;

                var hStatic = SysNative.FindWindowEx(mainWindowHandle, IntPtr.Zero, "Static", null);

                while (hStatic != IntPtr.Zero)
                {
                    if (sb == null)
                        sb = new StringBuilder(25);

                    SysNative.SendMessage(hStatic, SysNative.WM_GETTEXT, sb.Capacity, sb);
                    if (sb.Length == 0 || !sb.ToString().Contains("updat"))
                        break;

                    System.Threading.Thread.Sleep(1000);
                    if (loopCounter == 0)
                        Logger.Info("A Steam client update is in progress; waiting for completion.", 1);

                    loopCounter++;
                }

                if (loopCounter > 0)
                    Logger.Info("The Steam client update is complete; continuing program execution.", 1);
            }
            catch (Exception ex)
            {
                Logger.Warning($"The following exception occurred while attempting to detect if a Steam client update was in progress: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the Steam installation path from the registry or returns null if not found.
        /// </summary>
        /// <returns>A string containing the Steam installation path; if not found, returns null.</returns>
        private static string GetSteamInstallPath()
        {
            string installPath;
            if (SysNative.Is64Bit())
                installPath = (string) Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null);
            else
                installPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Valve\Steam", "InstallPath", null);

            if (installPath == null)
            {
                const string errorMsg = "No valid Steam installation path could be found.";
                Logger.Error(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            installPath = installPath.Replace("/", @"\");
            Logger.Info($"Setting Steam installation path to: '{installPath}'");

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

            Logger.Info($"Setting Steam Client DLL path to: '{dllPath}'");

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
            Logger.Info($"Setting Steam EXE path to: '{exePath}'");

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
    }
}

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using SteamLauncher.Logging;
using SteamLauncher.SteamClient.Interfaces;
using SteamLauncher.Settings;
using SteamLauncher.Tools;

namespace SteamLauncher.SteamClient
{
    /// <summary>
    /// To be used in the creation or reference of a Steam client shortcut.
    /// Note: This class is messy and definitely needs a rework... prob needs to be split into multiple classes.
    /// </summary>
    public class SteamShortcutManager
    {
        public struct DefaultShortcut
        {
            public const string NAME = "SteamLauncher";
            public const string EXE = @"%WINDIR%\system32\cmd.exe";
            public const string TAG = "SteamLauncherPlugin";
            public const string START_DIR = @"%WINDIR%\system32";
        }

        private static IClientShortcuts ClientShortcuts => SteamContext.Instance.ClientShortcuts;

        public UInt32 AppId { get; private set; }

        public string AppName { get; private set; }

        public string ExePath { get; private set; }

        public string ExePathInDoubleQuotes => ExePath.InDblQuotes();

        public string StartDir { get; private set; }

        public string StartDirInDoubleQuotes => StartDir.InDblQuotes();

        public string CommandLine { get; private set; }

        public SteamShortcutManager()
        {

        }

        public SteamShortcutManager(UInt32 appId)
        {
            AppId = appId;
        }

        /// <summary>
        /// Resets the shortcut's properties to the SteamShortcutManager default settings.
        /// </summary>
        /// <returns>True if successful; otherwise, false.</returns>
        public bool ResetProxyShortcut()
        {
            if (AppId == 0)
                return false;

            Logger.Info($"Resetting shortcut properties [{AppId}].");
            ClientShortcuts.SetShortcutAppName(AppId, DefaultShortcut.NAME);
            ClientShortcuts.SetShortcutExe(AppId, DefaultShortcut.EXE);
            ClientShortcuts.SetShortcutStartDir(AppId, DefaultShortcut.START_DIR);
            ClientShortcuts.ClearShortcutUserTags(AppId);
            ClientShortcuts.AddShortcutUserTag(AppId, DefaultShortcut.TAG);
            ClientShortcuts.SetShortcutHidden(AppId, true);
            return true;
        }

        /// <summary>
        /// Generates an instance of SteamShortcutManager using the data provided to it (presumably by the LaunchBox plugin API inside 'LaunchViaSteamMenuItem').
        /// </summary>
        /// <param name="gameName">The title of the game/rom (ex: 'Super Mario Bros.').</param>
        /// <param name="platformName">The platform that the game/rom runs under (ex: 'Nintendo 64', 'Windows', etc).</param>
        /// <param name="exePath">The absolute or relative (to LaunchBox dir) path to the exe (emulator, game exe, dosbox exe, etc) meant to run the game/rom.</param>
        /// <param name="startDir">The absolute or relative (to LaunchBox dir) path to be the working directory of the emulator.</param>
        /// <param name="args">Any additional arguments to be provided to the exe.</param>
        /// <returns>An instance of SteamShortcutManager which can be used to launch the game/rom.</returns>
        public static SteamShortcutManager GenerateShortcut(string gameName, string platformName, string exePath, string startDir, string args)
        {
            var launchBoxPath = Utilities.GetLaunchBoxPath();

            Logger.Info("Generating Steam shortcut...");
            Logger.Info($"LaunchBox directory located at: '{launchBoxPath}'", 1);

            if (!Utilities.IsFullPath(exePath))
                exePath = Path.GetFullPath(Path.Combine(launchBoxPath, exePath));

            Logger.Info($"Exe Path: '{exePath}'", 1);
            Logger.Info($"Arguments: '{args}'", 1);
            if (string.IsNullOrWhiteSpace(startDir))
                startDir = Path.GetDirectoryName(exePath);

            Logger.Info($"Start Directory: '{startDir}'", 1);
            var customPlatformName = Config.Instance.CustomPlatformNames.FirstOrDefault(x =>
                x.Name.Equals(platformName, StringComparison.OrdinalIgnoreCase))?.Custom ?? platformName;

            Logger.Info($"Original Platform Name: {platformName}; Custom Platform Name: {customPlatformName}; " +
                        $"(Changed: {platformName != customPlatformName})", 1);
            var displayName = $"{gameName} ({customPlatformName})".Trim(' ');

            // Create new SteamShortcutManager instance (which can be used to create/launch a Steam shortcut)
            var shortcut = new SteamShortcutManager
            {
                AppName = displayName,
                ExePath = exePath,
                StartDir = startDir,
                CommandLine = args
            };

            return shortcut;
        }

        /// <summary>
        /// Gets/creates Steam shortcut, assigns this instance's properties to the Steam shortcut, and then launches the shortcut in Steam.
        /// </summary>
        /// <returns>True if successful; otherwise, false.</returns>
        public bool LaunchShortcut()
        {
            AppId = GetNewOrExistingShortcut(out var isShortcutNew);
            if (AppId == 0)
                return false;

            Logger.Info($"Assigning properties to {(isShortcutNew ? "new" : "existing")} Steam shortcut: " +
                        $"[{AppId}] '{AppName}' | {ExePathInDoubleQuotes} | {StartDirInDoubleQuotes} | {CommandLine}");

            ClientShortcuts.SetShortcutAppName(AppId, AppName);
            ClientShortcuts.SetShortcutExe(AppId, ExePathInDoubleQuotes);
            ClientShortcuts.SetShortcutStartDir(AppId, StartDirInDoubleQuotes);
            ClientShortcuts.SetShortcutCommandLine(AppId, CommandLine);

            // Wait a moment to ensure Steam is done altering the shortcut
            System.Threading.Thread.Sleep(500);

            // Get the ShortcutID for this shortcut so we can construct a Steam shortcut URL
            var crc = ResolveShortcutId(ExePathInDoubleQuotes, AppName);
            Logger.Info($"'{AppName}' ShortcutID resolved to: {crc}");
            var steamUrl = $"steam://rungameid/{crc}";

            Logger.Info($"Starting Steam shortcut: {steamUrl}");

            // Start the shortcut
            Process.Start(steamUrl);

            return true;
        }

        /// <summary>
        /// An overload of <see cref="GetNewOrExistingShortcut(out bool)"/> to be used if the caller doesn't care if the returned shortcut is new or not.
        /// </summary>
        /// <returns>The AppID of the existing or newly created shortcut.</returns>
        private static UInt32 GetNewOrExistingShortcut()
        {
            bool isNew;
            return GetNewOrExistingShortcut(out isNew);
        }

        /// <summary>
        /// Tries to get an existing SteamLauncher shortcut; if no shortcut exists, it creates a new one.
        /// </summary>
        /// <param name="isNew">Defines if a new shortcut was created (as opposed to returning an existing shortcut).</param>
        /// <returns>The AppID of the existing or newly created shortcut.</returns>
        private static UInt32 GetNewOrExistingShortcut(out bool isNew)
        {
            UInt32 appId = RecycleShortcut();
            if (appId != 0)
            {
                // An existing shortcut was found, so we use it instead of creating a new one
                isNew = false;
                return appId;
            }

            // No existing shortcut exists, so we need to create one
            appId = CreateDefaultShortcut();

            // Indicate this is a newly created shortcut
            isNew = true;
            return appId;
        }

        /// <summary>
        /// Creates a hidden Steam shortcut with the name '<see cref="DefaultShortcut.NAME"/>' and an exe path of '<see cref="DefaultShortcut.EXE"/>'.
        /// </summary>
        /// <returns>The AppID of the newly created shortcut.</returns>
        private static UInt32 CreateDefaultShortcut()
        {
            UInt32 appId = ClientShortcuts.AddShortcut(DefaultShortcut.NAME, DefaultShortcut.EXE, "", "", "");
            if (appId == 0)
            {
                const string errorMsg = "An unexpected problem occurred while trying to create a default SteamLauncher shortcut.";
                Logger.Error(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            ClientShortcuts.AddShortcutUserTag(appId, DefaultShortcut.TAG);
            ClientShortcuts.SetShortcutHidden(appId, true);
            ClientShortcuts.SetAllowOverlay(appId, true);
            ClientShortcuts.SetAllowDesktopConfig(appId, true);

            return appId;
        }

        /// <summary>
        /// Cleans up excess SteamLauncher shortcuts if more than one is found and returns AppID of remaining single instance.
        /// </summary>
        /// <returns>AppID of single remaining instance of SteamLauncher shortcut.</returns>
        private static UInt32 RecycleShortcut()
        {
            List<UInt32> steamLauncherShortcuts = GetSteamLauncherShortcuts().ToList();
            if (steamLauncherShortcuts.Count == 0)
            {
                Logger.Info("No existing SteamLauncher shortcuts found.");
                return 0;
            }

            UInt32 keepShortcut = steamLauncherShortcuts[0];
            Logger.Info($"Recycling shortcut with AppID: {keepShortcut}");
            steamLauncherShortcuts.RemoveAt(0);
            if (steamLauncherShortcuts.Count > 0)
            {
                Logger.Info($"Removing {steamLauncherShortcuts.Count} residual shortcuts.");
                steamLauncherShortcuts.ForEach(shortcut => ClientShortcuts.RemoveShortcut(shortcut));
            }
            return keepShortcut;
        }

        /// <summary>
        /// Iterates over all existing Steam shortcuts and returns all SteamLauncher shortcuts (shortcuts with tag <see cref="DefaultShortcut.TAG"/>).
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<UInt32> GetSteamLauncherShortcuts()
        {
            UInt32 shortcutCount = ClientShortcuts.GetShortcutCount();
            Logger.Info($"Searching {shortcutCount} shortcuts to find all SteamLauncher shortcuts.");

            for (UInt32 sc = 0; sc < shortcutCount; sc++)
            {
                UInt32 appId = ClientShortcuts.GetShortcutAppIdByIndex(sc);
                if (appId == 0)
                    continue;

                // TODO: The Steam update on 2018-10-13 broke SteamLauncher 0.9.0.4. The call to GetShortcutUserTagCountByAppId is returning a very large invalid number (in most cases it should be 0). I suspect something has changed in the vftable offsets again.

                // Check if the shortcut has the unique SteamLauncher user category tag
                for (UInt32 tc = 0; tc < ClientShortcuts.GetShortcutUserTagCountByAppId(appId); tc++)
                {
                    if (!string.Equals(ClientShortcuts.GetShortcutUserTagByAppId(appId, tc), DefaultShortcut.TAG,
                        StringComparison.OrdinalIgnoreCase))
                        continue;

                    Logger.Info($"SteamLauncher shortcut found: [{appId}] {ClientShortcuts.GetShortcutAppNameByAppId(appId)}");
                    yield return appId;
                    break;
                }
            }
        }

        /// <summary>
        /// ShortcutID is the number steam shortcuts -> steam://rungameid/XXXXXXXXXXXXXXXXXXX (NOT the same as an AppID)
        /// </summary>
        /// <param name="exe">The complete path (including surrounding quotes) of an EXE referenced by a shortcut.</param>
        /// <param name="appName">The complete name of a Steam shortcut.</param>
        /// <returns>A UInt64 'ShortcutID' value.</returns>
        public static UInt64 ResolveShortcutId(string exe, string appName)
        {
            Logger.Info($"Attempting to resolve ShortcutID for {appName}...");
            UInt64 high32 = Crc32.Compute(Encoding.UTF8.GetBytes(exe + appName)) | 0x80000000;
            return (high32 << 32) | 0x02000000;
        }
    }
}

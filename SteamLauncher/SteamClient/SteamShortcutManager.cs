﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using SteamLauncher.Logging;
using SteamLauncher.SteamClient.Interfaces;
using SteamLauncher.DataStore;
using SteamLauncher.DataStore.VTablesStore;
using SteamLauncher.Tools;

namespace SteamLauncher.SteamClient
{
    /// <summary>
    /// Used for creating/managing a Steam client shortcut.
    /// </summary>
    public class SteamShortcutManager
    {
        public struct DefaultShortcut
        {
            public const string TAG = "SteamLauncherPlugin";
        }

        private static VTable ClientShortcutsVTable => SteamContext.Instance.ClientShortcutsVTable;
        //private static IClientShortcuts ClientShortcuts => SteamContext.Instance.ClientShortcuts;

        public UInt32 AppId { get; private set; }

        public string AppName { get; private set; }

        public string ExePath { get; private set; }

        public string ExePathInDoubleQuotes => ExePath.InDblQuotes();

        public string IconPath { get; private set; }

        public string IconPathInDoubleQuotes => IconPath.InDblQuotes();

        public string StartDir { get; private set; }

        public string StartDirInDoubleQuotes => StartDir.InDblQuotes();

        public string CommandLine { get; private set; }

        public UInt64 ShortcutId { get; private set; }

        public string ShortcutUrl => ShortcutId <= 0 ? null : $"steam://rungameid/{ShortcutId}";

        public SteamShortcutManager()
        {

        }

        public SteamShortcutManager(UInt32 appId)
        {
            AppId = appId;
        }

        /// <summary>
        /// Generates an instance of SteamShortcutManager using the data provided to it (presumably by the LaunchBox plugin API inside 'LaunchViaSteamMenuItem').
        /// </summary>
        /// <param name="gameName">The title of the game/rom (ex: 'Super Mario Bros.').</param>
        /// <param name="platformName">The platform that the game/rom runs under (ex: 'Nintendo 64', 'Windows', etc).</param>
        /// <param name="exePath">The absolute or relative (to LaunchBox dir) path to the exe (emulator, game exe, dosbox exe, etc) meant to run the game/rom.</param>
        /// <param name="startDir">The absolute or relative (to LaunchBox dir) path to be the working directory of the emulator.</param>
        /// <param name="args">Any additional arguments to be provided to the exe.</param>
        /// <param name="iconPath">The path of a PNG to be used as the shortcut's icon (for no icon, use empty string).</param>
        /// <returns>An instance of SteamShortcutManager which can be used to launch the game/rom.</returns>
        public static SteamShortcutManager GenerateShortcut(string gameName, string platformName, string exePath, string startDir, string args, string iconPath="")
        {
            // Cleanup any existing SteamLauncher shortcuts
            //RemoveAllSteamLauncherShortcuts();

            // Alternative to above (to fix issue noted in 'RemoveAllSteamLauncherShortcuts()')
            //ClientShortcuts.RemoveAllTemporaryShortcuts();
            ClientShortcutsVTable.GetVtEntry("RemoveAllTemporaryShortcuts").Invoke();

            // Create new SteamShortcutManager instance (which can be used to create/launch a Steam shortcut)
            var shortcut = new SteamShortcutManager();

            var launchBoxPath = Info.LaunchBoxDir;

            Logger.Info("Generating Steam shortcut...");
            //`Logger.Info($"LaunchBox directory located at: '{launchBoxPath}'");

            var customPlatformName =
                Settings.Config.CustomPlatformNames
                    .FirstOrDefault(x => x.Name.Equals(platformName, StringComparison.OrdinalIgnoreCase))?.Custom ??
                platformName;

            Logger.Info($"Original Platform Name: {platformName}; Custom Platform Name: " +
                        $"{(string.IsNullOrWhiteSpace(customPlatformName) ? "[EMPTY]" : customPlatformName)}; " +
                        $"(Changed: {platformName != customPlatformName})");

            shortcut.AppName = string.IsNullOrWhiteSpace(customPlatformName)
                ? gameName.Trim()
                : $"{gameName} ({customPlatformName})".Trim();
            Logger.Info($"App Name: '{shortcut.AppName}'");

            shortcut.ExePath = Utilities.GetAbsolutePath(exePath);
            Logger.Info($"Exe Path: '{shortcut.ExePath}'");

            shortcut.CommandLine = args;
            Logger.Info($"Arguments: '{shortcut.CommandLine}'");

            shortcut.StartDir = string.IsNullOrWhiteSpace(startDir) ? "" : Utilities.GetAbsolutePath(startDir);
            Logger.Info($"Start Directory: '{shortcut.StartDir}'");

            shortcut.IconPath = string.IsNullOrWhiteSpace(iconPath) ? "" : Utilities.GetAbsolutePath(iconPath);
            Logger.Info($"Icon Path: '{shortcut.IconPath}'");

            shortcut.AppId = CreateSteamLauncherShortcut(shortcut.AppName, 
                                                         shortcut.ExePath, 
                                                         shortcut.CommandLine, 
                                                         shortcut.StartDir, 
                                                         shortcut.IconPath);

            // Wait a moment to ensure Steam is done altering the shortcut (doubt this is needed.. verify and remove)
            System.Threading.Thread.Sleep(100);

            // Get the ShortcutID for this shortcut so we can construct a Steam shortcut URL
            
            // As of August 2022, temporary shortcuts properties no longer show any app name or EXE path (or any other
            // properties for that matter).. Therefore, to get a valid shortcut ID, you must call 'ResolveShortcutId'
            // with empty strings for EXE path and app name, i.e.: ResolveShortcutId("", "")

            shortcut.ShortcutId = ResolveShortcutId("", "");
            //shortcut.ShortcutId = ResolveShortcutId(shortcut.ExePathInDoubleQuotes, shortcut.AppName);
            
            Logger.Info($"'{shortcut.AppName}' ShortcutID resolved to: {shortcut.ShortcutId}");

            return shortcut;
        }

        public bool LaunchShortcut()
        {
            if (AppId <= 0)
            {
                Logger.Warning($"Cannot launch Steam shortcut for '{AppName}' because the appID is invalid.");
                return false;
            }

            Logger.Info($"Starting Steam shortcut: '{AppName}'");
            ClientShortcutsVTable.GetVtEntry("LaunchShortcut").Invoke(AppId);

            //if (string.IsNullOrEmpty(ShortcutUrl))
            //{
            //    Logger.Warning("Cannot launch the shortcut because the Steam shortcut URL is null or empty.");
            //    return false;
            //}

            //Logger.Info($"Starting Steam shortcut: {ShortcutUrl}");

            //var url = ShortcutUrl;

            //try
            //{
            //    // Start the shortcut
            //    Process.Start(new ProcessStartInfo() {FileName = url, UseShellExecute = true});
            //}
            //catch
            //{
            //    // hack because of this: https://github.com/dotnet/corefx/issues/10361
            //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //    {
            //        url = url.Replace("&", "^&");
            //        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            //    }
            //    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //    {
            //        Process.Start("xdg-open", url);
            //    }
            //    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            //    {
            //        Process.Start("open", url);
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}


            return true;
        }

        /// <summary>
        /// Creates a hidden temporary Steam shortcut with the user tag <see cref="DefaultShortcut.TAG"/>, 
        /// AllowOverlay set to True, and AllowDesktopConfig set to True.
        /// </summary>
        /// <param name="name">The name of the new shortcut.</param>
        /// <param name="exePath">The EXE path of the new shortcut.</param>
        /// <param name="cmdLine">The command line arguments of the new shortcut.</param>
        /// <param name="startDir">The start dir of the new shortcut.</param>
        /// <param name="iconPath">The path of a PNG to be used as the shortcut's icon (for no icon, use empty string).</param>
        /// <returns>The AppID of the newly created Steam shortcut.</returns>
        private static UInt32 CreateSteamLauncherShortcut(string name, string exePath, string cmdLine="", string startDir="", string iconPath="")
        {
            UInt32 appId = (UInt32)ClientShortcutsVTable.GetVtEntry("AddTemporaryShortcut").Invoke(name, 
                                                                                                   exePath.Trim('"'), 
                                                                                                   iconPath.Trim('"'));
            if (appId == 0)
            {
                const string errorMsg = "An unexpected problem occurred while trying to create a SteamLauncher shortcut.";
                Logger.Error(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            if (!string.IsNullOrWhiteSpace(startDir))
                ClientShortcutsVTable.GetVtEntry("SetShortcutStartDir").Invoke(appId, startDir.InDblQuotes());

            if (!string.IsNullOrWhiteSpace(cmdLine))
                    ClientShortcutsVTable.GetVtEntry("SetShortcutCommandLine").Invoke(appId, cmdLine);

            ClientShortcutsVTable.GetVtEntry("AddShortcutUserTag").Invoke(appId, DefaultShortcut.TAG);
            //ClientShortcutsVTable.GetVtEntry("SetShortcutHidden").Invoke(appId, true);
            ClientShortcutsVTable.GetVtEntry("SetAllowOverlay").Invoke(appId, true);
            ClientShortcutsVTable.GetVtEntry("SetAllowDesktopConfig").Invoke(appId, true);

            return appId;
        }

        ///// <summary>
        ///// Removes all existing SteamLauncher shortcuts from Steam (shortcuts with tag <see cref="DefaultShortcut.TAG"/>).
        ///// </summary>
        ///// <returns>The number of shortcuts removed.</returns>
        //private static int RemoveAllSteamLauncherShortcuts()
        //{
        //    // Note: With the new Steam library overhaul, this function can fail to remove the desired shortcuts at
        //    // times. It seems to happen whenever the Steam library UI crashes (window turns black), after which it
        //    // appears that shortcuts cannot be properly located via their tags. The only easy way to mitigate this 
        //    // issue is to delete all temporary shortcuts (instead of using this function).

        //    List<UInt32> steamLauncherShortcuts = GetSteamLauncherShortcuts().ToList();
        //    Logger.Info($"Removing all existing SteamLauncher shortcuts from Steam (total: {steamLauncherShortcuts.Count}).");
        //    steamLauncherShortcuts.ForEach(appId => ClientShortcutsVTable.GetVtEntry("RemoveShortcut").Invoke(appId));
        //    //steamLauncherShortcuts.ForEach(appId => ClientShortcuts.RemoveShortcut(appId));

        //    var newSteamLauncherShortcutsCount = GetSteamLauncherShortcuts().Count();
        //    var removedShortcutsCount = steamLauncherShortcuts.Count - newSteamLauncherShortcutsCount;
        //    if (removedShortcutsCount < steamLauncherShortcuts.Count)
        //        Logger.Warning($"Unable to remove all existing SteamLauncher shortcuts (removed: " +
        //                       $"{removedShortcutsCount} | remaining: {newSteamLauncherShortcutsCount}).");
            
        //    return removedShortcutsCount;
            
        //    //// Try forcibly removing remaining shortcuts by removing all temporary Steam shortcuts
        //    //ClientShortcuts.RemoveAllTemporaryShortcuts();

        //    //var updatedShortcutsCount = GetSteamLauncherShortcuts().Count();
        //    //removedShortcutsCount = steamLauncherShortcuts.Count - updatedShortcutsCount;

        //    //if (updatedShortcutsCount >= newSteamLauncherShortcutsCount)
        //    //{
        //    //    Logger.Warning($"Removing all temporary shortcuts failed to remove remaining shortcuts " +
        //    //                   $"(remaining: {updatedShortcutsCount}).");
        //    //}
        //    //else if (updatedShortcutsCount > 0)
        //    //{
        //    //    Logger.Warning($"Removing temporary shortcuts succeeded in removing part of the remaining " +
        //    //                   $"shortcuts (remaining: {updatedShortcutsCount}).");
        //    //}
        //    //else
        //    //{
        //    //    Logger.Warning($"Removing temporary shortcuts succeeded in removing all remaining shortcuts.");
        //    //}

        //    //return removedShortcutsCount;
        //}

        ///// <summary>
        ///// Iterates over all existing Steam shortcuts and returns all SteamLauncher shortcuts (shortcuts with tag <see cref="DefaultShortcut.TAG"/>).
        ///// </summary>
        ///// <returns></returns>
        //private static IEnumerable<UInt32> GetSteamLauncherShortcuts()
        //{
        //    UInt32 shortcutCount = (UInt32)ClientShortcutsVTable.GetVtEntry("GetShortcutCount").Invoke();
        //    //UInt32 shortcutCount = ClientShortcuts.GetShortcutCount();
        //    Logger.Info($"Searching {shortcutCount} shortcuts to find all SteamLauncher shortcuts.");

        //    for (UInt32 sc = 0; sc < shortcutCount; sc++)
        //    {
        //        UInt32 appId = (UInt32)ClientShortcutsVTable.GetVtEntry("GetShortcutAppIdByIndex").Invoke(sc);
        //        //UInt32 appId = ClientShortcuts.GetShortcutAppIdByIndex(sc);
        //        if (appId == 0)
        //            continue;

        //        // Check if the shortcut has the unique SteamLauncher user category tag
        //        //for (UInt32 tc = 0; tc < ClientShortcuts.GetShortcutUserTagCountByAppId(appId); tc++)
        //        for (UInt32 tc = 0; tc < (UInt32)ClientShortcutsVTable.GetVtEntry("GetShortcutUserTagCountByAppId").Invoke(appId); tc++)
        //        {
        //            //if (!ClientShortcuts.GetShortcutUserTagByAppId(appId, tc)
        //            if (!((string)ClientShortcutsVTable.GetVtEntry("GetShortcutUserTagByAppId").Invoke(appId, tc))
        //                    .EqualsIgnoreCase(DefaultShortcut.TAG, true))
        //                continue;

        //            Logger.Info($"SteamLauncher shortcut found: [{appId}] {(string)ClientShortcutsVTable.GetVtEntry("GetShortcutAppNameByAppId").Invoke(appId)}");
        //            //Logger.Info($"SteamLauncher shortcut found: [{appId}] {ClientShortcuts.GetShortcutAppNameByAppId(appId)}");
        //            yield return appId;
        //            break;
        //        }
        //    }
        //}


        ///// <summary>
        ///// Iterates over all existing Steam shortcuts and returns all temporary shortcuts.
        ///// </summary>
        ///// <returns>An enumerable list of temporary Steam shortcuts.</returns>
        //private static IEnumerable<UInt32> GetTemporaryShortcuts()
        //{
        //    UInt32 shortcutCount = (UInt32)ClientShortcutsVTable.GetVtEntry("GetShortcutCount").Invoke();
        //    //var shortcutCount = ClientShortcuts.GetShortcutCount();
        //    Logger.Info($"Searching {shortcutCount} shortcuts to find all temporary Steam shortcuts.");

        //    for (UInt32 i = 0; i < shortcutCount; i++)
        //    {
        //        //if (ClientShortcuts.BIsTemporaryShortcutByIndex(i))
        //        if ((bool)ClientShortcutsVTable.GetVtEntry("BIsTemporaryShortcutByIndex").Invoke(i))
        //        {
        //            var appId = (UInt32)ClientShortcutsVTable.GetVtEntry("GetShortcutAppIdByIndex").Invoke(i);
        //            //var appId = ClientShortcuts.GetShortcutAppIdByIndex(i);
        //            if (appId == 0)
        //                continue;

        //            Logger.Info($"Temporary Steam shortcut found: [{appId}] {(string)ClientShortcutsVTable.GetVtEntry("GetShortcutAppNameByAppId").Invoke(appId)}");
        //            //Logger.Info($"Temporary Steam shortcut found: [{appId}] {ClientShortcuts.GetShortcutAppNameByAppId(appId)}");
        //            yield return appId;
        //        }
        //    }
        //}


        ///// <summary>
        ///// Resolves a Steam shortcut index from the provided AppID.
        ///// </summary>
        ///// <param name="appId">The AppID used to lookup the Steam shortcut.</param>
        ///// <returns>The index of the Steam shortcut found.</returns>
        //public static UInt32 ResolveShortcutIndexFromAppId(UInt32 appId)
        //{
        //    UInt32 shortcutCount = (UInt32)ClientShortcutsVTable.GetVtEntry("GetShortcutCount").Invoke();
        //    //UInt32 shortcutCount = ClientShortcuts.GetShortcutCount();
        //    for (UInt32 i = 0; i < shortcutCount; i++)
        //    {
        //        //if (ClientShortcuts.GetShortcutAppIdByIndex(i) == appId)
        //        if ((UInt32)ClientShortcutsVTable.GetVtEntry("GetShortcutAppIdByIndex").Invoke(i) == appId)
        //            return i;
        //    }
        //    throw new ArgumentException($"Could not find a shortcut with the AppID '{appId}'.", nameof(appId));
        //}

        /// <summary>
        /// ShortcutID is a reference to a steam game shortcut.. the same that is used when you create a shortcut on 
        /// your Desktop of a Steam game -> steam://rungameid/XXXXXXXXXXXXXXXXXXX (NOT the same as an AppID)
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SteamLauncher.Logging;
using SteamLauncher.DataStore;

namespace SteamLauncher.Tools
{
    public static class Info
    {
        #region DOSBox

        /// <summary>
        /// The full path to the DOSBox directory located within the LaunchBox directory.
        /// </summary>
        /// <returns>The path of the DOSBox directory.</returns>
        public static string DosBoxDir => Path.GetDirectoryName(DosBoxExePath);

        /// <summary>
        /// The full path to the DOSBox executable that comes packaged with LaunchBox.
        /// </summary>
        /// <returns>The path of the DOSBox executable.</returns>
        public static string DosBoxExePath =>
            Utilities.GetAbsolutePath(Environment.ExpandEnvironmentVariables(Settings.Config.DosBoxExePath));

        /// <summary>
        /// The filename of the original DOSBox exe that has been temporarily renamed for use with 
        /// SteamLauncherProxy.
        /// </summary>
        public static string DosBoxExeBackupName =>
            $"{Path.GetFileNameWithoutExtension(DosBoxExePath)}.SL-Original.exe";

        /// <summary>
        /// The full path to the DOSBox executable that has been temporarily renamed for use with 
        /// SteamLauncherProxy.
        /// </summary>
        /// <returns>The renamed DOSBox exe path.</returns>
        public static string DosBoxExeBackupPath => Path.Combine(DosBoxDir, DosBoxExeBackupName);

        /// <summary>
        /// The filename of the original DOSBox exe that was copied for the purposes of making a permanent backup 
        /// of the exe.
        /// </summary>
        public static string DosBoxExePermanentBackupName =>
            $"{Path.GetFileNameWithoutExtension(DosBoxExePath)}.Original-Backup.exe";

        /// <summary>
        /// The full path to the original DOSBox exe that was copied for the purposes of making a permanent backup 
        /// of the exe.
        /// </summary>
        public static string DosBoxExePermanentBackupPath => Path.Combine(DosBoxDir, DosBoxExePermanentBackupName);

        /// <summary>
        /// The filename of the proxy exe, previously used as a stand-in for the DOSBox exe, which has now 
        /// been queued for deletion on the next check.
        /// </summary>
        public static string DosBoxExeQueuedToDeleteName =>
            $"{Path.GetFileNameWithoutExtension(DosBoxExePath)}.SL-Delete.exe";

        /// <summary>
        /// The full path to the proxy exe, previously used as a stand-in for the DOSBox exe, which has now 
        /// been queued for deletion on the next check.
        /// </summary>
        public static string DosBoxExeQueuedToDeletePath => Path.Combine(DosBoxDir, DosBoxExeQueuedToDeleteName);

        #endregion

        #region ScummVM

        /// <summary>
        /// The full path to the ScummVm directory located within the LaunchBox directory.
        /// </summary>
        /// <returns>The path of the ScummVm directory.</returns>
        public static string ScummVmDir => Path.GetDirectoryName(ScummVmExePath);

        /// <summary>
        /// The full path to the ScummVm executable that comes packaged with LaunchBox.
        /// </summary>
        /// <returns>The path of the ScummVm executable.</returns>
        public static string ScummVmExePath =>
            Utilities.GetAbsolutePath(Environment.ExpandEnvironmentVariables(Settings.Config.ScummVmExePath));

        /// <summary>
        /// The filename of the original ScummVM exe that has been temporarily renamed for use with 
        /// SteamLauncherProxy.
        /// </summary>
        public static string ScummVmExeBackupName =>
            $"{Path.GetFileNameWithoutExtension(ScummVmExePath)}.SL-Original.exe";

        /// <summary>
        /// The full path to the ScummVM executable that has been temporarily renamed for use with 
        /// SteamLauncherProxy.
        /// </summary>
        /// <returns>The renamed ScummVm exe path.</returns>
        public static string ScummVmExeBackupPath => Path.Combine(ScummVmDir, ScummVmExeBackupName);

        /// <summary>
        /// The filename of the original ScummVM exe that was copied for the purposes of making a permanent backup 
        /// of the exe.
        /// </summary>
        public static string ScummVmExePermanentBackupName =>
            $"{Path.GetFileNameWithoutExtension(ScummVmExePath)}.Original-Backup.exe";

        /// <summary>
        /// The full path to the original ScummVM exe that was copied for the purposes of making a permanent backup 
        /// of the exe.
        /// </summary>
        public static string ScummVmExePermanentBackupPath => Path.Combine(ScummVmDir, ScummVmExePermanentBackupName);

        /// <summary>
        /// The filename of the proxy exe, previously used as a stand-in for the ScummVM exe, which has now 
        /// been queued for deletion on the next check.
        /// </summary>
        public static string ScummVmExeQueuedToDeleteName =>
            $"{Path.GetFileNameWithoutExtension(ScummVmExePath)}.SL-Delete.exe";

        /// <summary>
        /// The full path to the proxy exe, previously used as a stand-in for the ScummVM exe, which 
        /// has now been queued for deletion on the next check.
        /// </summary>
        public static string ScummVmExeQueuedToDeletePath => Path.Combine(ScummVmDir, ScummVmExeQueuedToDeleteName);

        #endregion

        #region SteamLauncherProxy
        /// <summary>
        /// The name of the SteamLauncherProxy executable and the caption of its main window.
        /// </summary>
        public static string ProxyEmulatorTitle => "SteamLauncherProxy";

        /// <summary>
        /// The full path to the SteamLauncherProxy executable file.
        /// </summary>
        public static string SteamLauncherProxyExePath =>
            Path.Combine(SteamLauncherDir, $"{ProxyEmulatorTitle}.exe");

        /// <summary>
        /// The relative path from the LaunchBox directory to the SteamLauncherProxy executable file.
        /// </summary>
        public static string SteamLauncherProxyExeRelativePath =>
            new DirectoryInfo(LaunchBoxDir).GetRelativePathTo(new FileInfo(SteamLauncherProxyExePath));

        #endregion

        #region SteamLauncher

        /// <summary>
        /// Gets the currently loaded SteamLauncher assembly.
        /// </summary>
        /// <returns>A reference to the currently loaded SteamLauncher assembly.</returns>
        public static Assembly GetLoadedSteamLauncherAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        private static string _steamLauncherDir;

        /// <summary>
        /// Retrieves the full path to the directory where the plugin is currently being executed from.
        /// </summary>
        public static string SteamLauncherDir
        {
            get
            {
                if (_steamLauncherDir != null)
                    return _steamLauncherDir;

                _steamLauncherDir = Path.GetDirectoryName(Path.GetFullPath(GetLoadedSteamLauncherAssembly().Location));
                return _steamLauncherDir;
            }
        }

        private static string _steamLauncherPath;

        /// <summary>
        /// Retrieves the full path to the currently executing SteamLauncher DLL.
        /// </summary>
        public static string SteamLauncherPath
        {
            get
            {
                if (_steamLauncherPath != null)
                    return _steamLauncherPath;

                _steamLauncherPath = Path.GetFullPath(GetLoadedSteamLauncherAssembly().Location);
                return _steamLauncherPath;
            }
        }

        /// <summary>
        /// Gets the version string of the currently loaded SteamLauncher DLL.
        /// </summary>
        /// <returns>The SteamLauncher version.</returns>
        public static string GetSteamLauncherVersion()
        {
            var version = GetLoadedSteamLauncherAssembly()
                ?.GetCustomAttributes(typeof(AssemblyFileVersionAttribute))
                ?.Cast<AssemblyFileVersionAttribute>()
                ?.FirstOrDefault()
                ?.Version;

            if (version == null)
                throw new Exception("Could not resolve SteamLauncher version.");

            return version;
        }

        /// <summary>
        /// Checks if the SteamLauncher plugin has been loaded more than once which would indicate that a user 
        /// erroneously left more than one copy of the plugin somewhere within the LaunchBox directory structure.
        /// </summary>
        /// <returns>True if SteamLauncher is loaded into the AppDomain more than once; otherwise, False.</returns>
        public static bool IsSteamLauncherAlreadyLoaded()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Count(x => x.FullName.ToLower().Contains("steamlauncher")) >=
                   2;
        }

        /// <summary>
        /// Gets a list of strings which contain the location and version of each loaded SteamLauncher assembly. If 
        /// the returned list contains more than 1 item, this indicates that the user erroneously left more than 1 
        /// copy of the plugin somewhere with the LaunchBox directory structure.
        /// </summary>
        /// <returns>A list of strings containing info about the loaded SteamLauncher assemblies.</returns>
        public static List<string> GetLoadedSteamLauncherModulesInfo()
        {
            var steamLauncherAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName.ToLower().Contains("steamlauncher"));
            var moduleInfo = new List<string>();
            foreach (var assembly in steamLauncherAssemblies)
            {
                var version = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute))
                    ?.Cast<AssemblyFileVersionAttribute>()
                    ?.FirstOrDefault()
                    ?.Version;

                moduleInfo.Add($"Location: '{assembly.Location}'; Version: {version}");
            }

            return moduleInfo;
        }

        #endregion

        #region LaunchBox

        private static Process _launchBoxProcess = null;

        /// <summary>
        /// Retrieves the plugin's hosting process (LaunchBox or BigBox).
        /// </summary>
        public static Process LaunchBoxProcess
        {
            get
            {
                if (_launchBoxProcess != null)
                    return _launchBoxProcess;

                _launchBoxProcess = Process.GetCurrentProcess();
                return _launchBoxProcess;
            }
        }

        private static string _launchBoxDir;
        /// <summary>
        /// Provides the full path to the base LaunchBox directory.
        /// </summary>
        public static string LaunchBoxDir
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_launchBoxDir))
                    return _launchBoxDir;

                var cd = Environment.CurrentDirectory;
                var diCd = new DirectoryInfo(cd);

                if (diCd.Name.ToLower() == "launchbox" &&
                    diCd.EnumerateFiles().Any(x => x.Name.ToLower() == "launchbox.exe"))
                {
                    LaunchBoxDir = cd;
                    return _launchBoxDir;
                }

                if (diCd.Name.ToLower() == "core" &&
                    diCd.Parent != null &&
                    diCd.Parent.Name.ToLower() == "launchbox" &&
                    diCd.Parent.EnumerateFiles().Any(x => x.Name.ToLower() == "launchbox.exe"))
                {
                    LaunchBoxDir = diCd.Parent.FullName;
                    return _launchBoxDir;
                }

                Logger.Warning("It appears that a LB update may have introduced new problems with obtaining the " +
                               "LaunchBox directory (current working directory). Attempting to get the LaunchBox " +
                               "directory via alternative means.");

                if (diCd.EnumerateFiles().Any(x => x.Name.ToLower() == "launchbox.exe"))
                {
                    if (diCd.Parent != null &&
                        diCd.Parent.EnumerateFiles().Any(x => x.Name.ToLower() == "launchbox.exe") &&
                        diCd.Parent.EnumerateDirectories().Any(x => x.Name.ToLower() == "plugins"))
                    {
                        LaunchBoxDir = diCd.Parent.FullName;
                        return _launchBoxDir;
                    }

                    if (diCd.EnumerateDirectories().Any(x => x.Name.ToLower() == "plugins"))
                    {
                        LaunchBoxDir = diCd.FullName;
                        return _launchBoxDir;
                    }
                }

                var di = new DirectoryInfo(Info.SteamLauncherDir);
                while (di.Parent != null)
                {
                    if (di.Parent.EnumerateFiles().Any(x => x.Name.ToLower() == "launchbox.exe") &&
                        (di.Parent.Name.ToLower() == "launchbox" || di.Name.ToLower() == "plugins"))
                    {
                        LaunchBoxDir = di.Parent.FullName;
                        return _launchBoxDir;
                    }

                    di = di.Parent;
                }

                Logger.Warning("There was almost certainly a problem obtaining the LaunchBox directory, most likely due " +
                               "to a recent LB update. Using LaunchBox.exe's current working directory as a last resort.");


                LaunchBoxDir = cd;
                return _launchBoxDir;
            }
            private set
            {
                Logger.Info($"LaunchBox directory resolved to '{value}'.");
                _launchBoxDir = value;
            }
        }

        /// <summary>
        /// Provides the full path to the 'LaunchBox\Core' directory.
        /// </summary>
        public static string LaunchBoxCoreDir => Path.Join(LaunchBoxDir, "Core");

        #endregion
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SteamLauncher.Tools
{
    public static class Utilities
    {
        private static Process _launchBoxProcess = null;

        /// <summary>
        /// Retrieves the full path where this library is currently being executed from (excluding file name).
        /// </summary>
        /// <returns>The path of this library.</returns>
        public static string GetSteamLauncherDllPath()
        {
            return Path.GetDirectoryName(Path.GetFullPath(System.Reflection.Assembly.GetExecutingAssembly().Location));
        }

        /// <summary>
        /// Retrieves the full path to the directory which the LaunchBox executable resides in.
        /// </summary>
        /// <returns>The path of the LaunchBox executable.</returns>
        public static string GetLaunchBoxPath()
        {
            return Path.GetDirectoryName(Path.GetFullPath(System.Reflection.Assembly.GetEntryAssembly().Location));
        }

        public static Process GetLaunchBoxProcess()
        {
            if (_launchBoxProcess != null)
                return _launchBoxProcess;

            _launchBoxProcess = Process.GetCurrentProcess();
            return _launchBoxProcess;
        }

        /// <summary>
        /// Specifies whether the provided path is an absolute path including root directory information (as opposed to a relative path).
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the provided path is an absolute path; otherwise, false.</returns>
        public static bool IsFullPath(string path)
        {
            return !string.IsNullOrWhiteSpace(path)
                && path.IndexOfAny(Path.GetInvalidPathChars().ToArray()) == -1
                && Path.IsPathRooted(path)
                && !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
        }
    }
}

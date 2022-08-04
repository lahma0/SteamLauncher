using System.IO;
using System.Linq;
using System.Xml.Serialization;
using SteamLauncher.Logging;
using SteamLauncher.Tools;

namespace SteamLauncher.DataStore
{
    /// <summary>
    /// When Universal Steam launching is enabled, this class enables users to define a correlation 
    /// between launcher processes and their primary (target) executables. This allows the plugin to 
    /// determine a game/emulator's "actual" exe name when a launcher exe is chosen as its application 
    /// path inside of LaunchBox. This information allows the plugin to correctly determine when a 
    /// game/emulator process starts and ends, which is important for many aspects of the plugin's 
    /// functionality (startup/shutdown screens being one of the most obvious).
    /// </summary>
    public class LauncherToExe
    {
        // To-Do: Consider adding the ability to differentiate the launcher exe, not just by the EXE name/path, but
        // also by the launch arguments used at run-time. For example, a property such as, 'LauncherArgsContain', could
        // be added to this class. If this property is populated, the plugin would check if the launcher EXE's
        // arguments contained this string during run-time, and if it did, the plugin would know to associate that
        // launcher instance with the defined TargetFilename.

        /// <summary>
        /// Filename or full path of the launcher executable used to launch a long-running primary executable.
        /// Launchers can be used for many things: game launcher/config utilities (GTAVLauncher.exe), game
        /// client/stores (Epic Games, Battle.net, Uplay), loaders/memory-patchers (ppatcher), virtualized/portable
        /// apps (Turbo Studio, Cameyo, ThinApp), sandboxes (Sandboxie - Start.exe), etc. This value can be set to
        /// either the filename (GTAVLauncher.exe) or to the full path (C:\GTAV\GTAVLauncher.exe) depending on how
        /// specific the user wishes for the relationship to be.
        /// </summary>
        [XmlAttribute]
        public string LauncherFilename { get; set; }

        /// <summary>
        /// Filename or full path of the long-running target executable which is started by the launcher. This value
        /// can be set to either the filename (GTA5.exe) or to the full path (C:\GTAV\GTA5.exe) depending on how
        /// specific the user wishes for the relationship to be.
        /// </summary>
        [XmlAttribute]
        public string TargetFilename { get; set; }

        /// <summary>
        /// Defines whether this entry is enabled or not.
        /// </summary>
        [XmlAttribute]
        public bool Enable { get; set; }

        /// <summary>
        /// Matches the provided source file to a corresponding target file in the config's 
        /// LauncherToExeDefinitions list. If no match is found, the source file info is returned.
        /// </summary>
        /// <param name="sourceFileInfo">The source file to match against.</param>
        /// <returns>If a match is found, the target file info; otherwise, the source file info.</returns>
        public static FileInfo ResolveRelationship(FileInfo sourceFileInfo)
        {
            // Get enabled definitions whose filename (not including dir info) matches that of the source
            var launcherToExeInitialMatches = Settings.Config.LauncherToExeDefinitions.Where(
                x => x.Enable && Path.GetFileName(x.LauncherFilename).EqualsIgnoreCase(sourceFileInfo.Name));

            LauncherToExe preciseMatch = null;
            LauncherToExe generalMatch = null;

            // Used to narrow down matches to 'precise' (has dir info and matches) and 'general' (no supplied dir info)
            foreach (var launcherToExe in launcherToExeInitialMatches)
            {
                var launcherFilename = launcherToExe.LauncherFilename;

                // If no directory is defined, it is considered a general match
                if (!Path.IsPathRooted(launcherFilename))
                {
                    // Keep only the 1st general match that is found
                    generalMatch ??= launcherToExe;

                    continue;
                }

                // If we get here, the path is rooted (directory info is present)

                // If it defines a directory and it matches the source directory, it is considered a precise match
                if (Path.GetDirectoryName(launcherFilename).EqualsIgnoreCase(sourceFileInfo.DirectoryName))
                {
                    // Keep only the 1st precise match that is found
                    preciseMatch ??= launcherToExe;
                }
            }

            // Use precise match if available, otherwise use general match
            var launcherToExeMatch = preciseMatch ?? generalMatch;

            // If a match was found, use it
            if (launcherToExeMatch != null)
            {
                var targetFileInfo = new FileInfo(launcherToExeMatch.TargetFilename);
                Logger.Info($"Launcher-to-Exe relationship found! Source: '{sourceFileInfo.Name}' - " +
                            $"Target: '{targetFileInfo.Name}'");

                return targetFileInfo;
            }

            Logger.Info($"No Launcher-to-Exe relationship found for file '{sourceFileInfo.FullName}'.");

            // If no matches are found, return the source file info
            return sourceFileInfo;
        }
    }
}

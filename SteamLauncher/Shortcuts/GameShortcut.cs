using System.IO;
using SteamLauncher.Logging;
using SteamLauncher.SteamClient;
using SteamLauncher.Tools;
using System.Collections.Generic;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace SteamLauncher.Shortcuts
{
    public abstract class GameShortcut
    {
        protected GameShortcut(IGame iGame)
        {
            LaunchBoxGame = iGame;
            AdditionalApplications = iGame.GetAllAdditionalApplications();
            Platform = PluginHelper.DataManager.GetPlatformByName(LaunchBoxGame.Platform);
            Logger.Info($"Creating Game Shortcut - Platform: '{iGame.Platform}'; Title: '{iGame.Title}'");
        }

        public static GameShortcut CreateGameShortcut(IGame iGame)
        {
            if (iGame.UseDosBox)
                return new DosBoxShortcut(iGame);

            if (iGame.UseScummVm)
                return new ScummVmShortcut(iGame);

            if (!string.IsNullOrEmpty(iGame.EmulatorId))
                return new EmulatorShortcut(iGame);

            return new WindowsShortcut(iGame);
        }

        public IAdditionalApplication[] AdditionalApplications { get; }

        public IGame LaunchBoxGame { get; }

        public IPlatform Platform { get; }

        public string GameTitle => LaunchBoxGame.Title;

        public string PlatformName => LaunchBoxGame.Platform;

        public virtual string LaunchExePath => Utilities.GetAbsolutePath(LaunchBoxGame.ApplicationPath.Trim('"'));

        private string _resolvedLaunchExePath;

        public string ResolvedLaunchExePath => _resolvedLaunchExePath;

        public virtual string LaunchArguments => LaunchBoxGame.CommandLine;

        public virtual string OverrideLaunchArguments { get; set; }

        private string _resolvedLaunchArguments;

        public string ResolvedLaunchArguments => _resolvedLaunchArguments;

        public Dictionary<string, string> CustomFields = new Dictionary<string, string>();

        public virtual string RootFolder
        {
            get
            {
                if (!string.IsNullOrEmpty(LaunchBoxGame.RootFolder.Trim('"')))
                    return Utilities.GetAbsolutePath(LaunchBoxGame.RootFolder.Trim('"'));

                return "";
            }
        }

        private string _resolvedRootFolder;

        public string ResolvedRootFolder => _resolvedRootFolder;

        ///// <summary>
        ///// Keeps track of whether the "Resolve" method has already been run.
        ///// </summary>
        //private bool _isResolved;

        /// <summary>
        /// Populates all of the "Resolved" property values.
        /// </summary>
        public void Resolve()
        {
            //if (_isResolved)
            //    return;

            // Set all "Resolved" properties to their default non-resolved values (and then modify below if need-be)
            _resolvedLaunchExePath = LaunchExePath;
            _resolvedLaunchArguments = OverrideLaunchArguments ?? LaunchArguments;
            _resolvedRootFolder = RootFolder;

            // Check if LaunchExePath is a Windows .lnk shortcut
            if (Path.GetExtension(_resolvedLaunchExePath).EqualsIgnoreCase(".lnk"))
            {
                // Resolve the .lnk file path to its target file path
                var lnkShortcut = new WindowsLnkFile(_resolvedLaunchExePath);
                _resolvedLaunchExePath = lnkShortcut.TargetPath;

                // If the .lnk file has arguments, combine those with any existing arguments
                if (!string.IsNullOrWhiteSpace(lnkShortcut.Arguments))
                    _resolvedLaunchArguments = lnkShortcut.Arguments.Trim() + " " + _resolvedLaunchArguments;

                // 2020-10-26 - Changed this to ignore the LB game's 'Root Folder' property whenever it is set to the
                // directory containing the .LNK file and instead set it to the 'Start in' directory defined within the
                // shortcut itself. I did this because whenever you add a game to LB using a .LNK file, it sets the
                // 'Root Folder' to the shortcut file's directory instead of setting it to the 'Start in' directory
                // defined in the shortcut (or the directory of the target EXE itself). This consequently causes most
                // games to either crash/malfunction or not work at all.
                //if (Path.GetDirectoryName(LaunchExePath).ToLower() == RootFolder.ToLower() && !string.IsNullOrWhiteSpace(lnkShortcut.WorkingDirectory))
                if (!string.IsNullOrWhiteSpace(lnkShortcut.WorkingDirectory))
                    _resolvedRootFolder = lnkShortcut.WorkingDirectory;
                else if (string.IsNullOrWhiteSpace(RootFolder))
                    _resolvedRootFolder = Path.GetDirectoryName(_resolvedLaunchExePath);
            }

            //_isResolved = true;
        }

        public SteamShortcutManager GenerateSteamShortcut()
        {
            // Update all "Resolved" properties before creating Steam shortcut
            Resolve();

            return SteamShortcutManager.GenerateShortcut(GameTitle, PlatformName, ResolvedLaunchExePath, ResolvedRootFolder, ResolvedLaunchArguments);
        }
    }
}

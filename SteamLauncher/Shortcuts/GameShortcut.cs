using System.IO;
using SteamLauncher.Logging;
using SteamLauncher.SteamClient;
using SteamLauncher.Tools;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace SteamLauncher.Shortcuts
{
    public abstract class GameShortcut
    {
        protected GameShortcut(IGame iGame)
        {
            LaunchBoxGame = iGame;
            Platform = PluginHelper.DataManager.GetPlatformByName(LaunchBoxGame.Platform);
            Logger.Info($"Creating Game Shortcut - Platform: '{iGame.Platform}'; Title: '{iGame.SortTitleOrTitle}'");
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

        public IGame LaunchBoxGame { get; private set; }

        public IPlatform Platform { get; private set; }

        public string GameTitle => LaunchBoxGame.SortTitleOrTitle;

        public string PlatformName => LaunchBoxGame.Platform;

        public virtual string LaunchExePath
        {
            get
            {
                var exePath = LaunchBoxGame.ApplicationPath;
                if (!Utilities.IsFullPath(exePath))
                    exePath = Path.GetFullPath(Path.Combine(Utilities.GetLaunchBoxPath(), exePath));

                return exePath;
            }
        }

        public virtual string LaunchArguments => LaunchBoxGame.CommandLine;

        //public string RootFolder => LaunchBoxGame.RootFolder;

        public string RootFolder
        {
            get
            {
                if (string.IsNullOrEmpty(LaunchBoxGame.RootFolder))
                    return Path.GetDirectoryName(LaunchExePath);

                var rootFolder = LaunchBoxGame.RootFolder;
                if (!Utilities.IsFullPath(rootFolder))
                    rootFolder = Path.GetFullPath(Path.Combine(Utilities.GetLaunchBoxPath(), rootFolder));

                return rootFolder;
            }
        }

        public SteamShortcutManager GenerateSteamShortcut()
        {
            return SteamShortcutManager.GenerateShortcut(GameTitle, PlatformName, LaunchExePath, RootFolder, LaunchArguments);
        }
    }
}

using SteamLauncher.DataStore;
using SteamLauncher.Tools;
using Unbroken.LaunchBox.Plugins.Data;

namespace SteamLauncher.Shortcuts
{
    public class DosBoxShortcut : GameShortcut
    {
        public DosBoxShortcut(IGame iGame) : base(iGame)
        {
            Mounts = iGame.GetAllMounts();
        }

        public IMount[] Mounts { get; }

        public override string LaunchExePath => Utilities.GetAbsolutePath(Settings.Config.DosBoxExePath);

        public string DosBoxConfigPath => LaunchBoxGame.DosBoxConfigurationPath;

        public string DosBoxGamePath => Utilities.GetAbsolutePath(LaunchBoxGame.ApplicationPath);

        public override string LaunchArguments
        {
            get
            {
                var args = $"{DosBoxGamePath.InDblQuotes()}".Trim(' ');
                if (!string.IsNullOrEmpty(DosBoxConfigPath))
                    args = $"{args} -userconf -conf {DosBoxConfigPath.InDblQuotes()}".Trim(' ');

                foreach (var mount in Mounts)
                    args = $"{args} -c \"MOUNT {mount.DriveLetter} '{mount.Path}'\"".Trim(' ');

                if (!string.IsNullOrEmpty(LaunchBoxGame.CommandLine))
                    args = $"{args} {LaunchBoxGame.CommandLine}".Trim(' ');

                return args;
            }
        }
    }
}

using System.IO;
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

        public IMount[] Mounts { get; private set; }

        public override string LaunchExePath => Path.Combine(Utilities.GetLaunchBoxPath(), "DOSBox\\DOSBox.exe");

        public string DosBoxConfigPath => LaunchBoxGame.DosBoxConfigurationPath;

        public string DosBoxGamePath
        {
            get
            {
                var gamePath = LaunchBoxGame.ApplicationPath;
                if (!Utilities.IsFullPath(gamePath))
                    gamePath = Path.GetFullPath(Path.Combine(Utilities.GetLaunchBoxPath(), gamePath));

                return gamePath;
            }
        }

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

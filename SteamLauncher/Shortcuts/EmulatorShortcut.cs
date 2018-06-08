using System.IO;
using System.Linq;
using SteamLauncher.Tools;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace SteamLauncher.Shortcuts
{
    public class EmulatorShortcut : GameShortcut
    {
        public EmulatorShortcut(IGame iGame) : base(iGame)
        {

        }

        public IEmulator Emulator => PluginHelper.DataManager.GetEmulatorById(LaunchBoxGame.EmulatorId);

        public IEmulatorPlatform EmulatorPlatform => Emulator.GetAllEmulatorPlatforms().FirstOrDefault(p => p.Platform == PlatformName);

        public string RomPath
        {
            get
            {
                var romPath = LaunchBoxGame.ApplicationPath;
                if (!Utilities.IsFullPath(romPath))
                    romPath = Path.GetFullPath(Path.Combine(Utilities.GetLaunchBoxPath(), romPath));

                return romPath;
            }
        }

        public override string LaunchExePath
        {
            get
            {
                var exePath = Emulator.ApplicationPath;
                if (!Utilities.IsFullPath(exePath))
                    exePath = Path.GetFullPath(Path.Combine(Utilities.GetLaunchBoxPath(), exePath));

                return exePath;
            }
        }

        public override string LaunchArguments
        {
            get
            {
                var args = "";
                if (!string.IsNullOrEmpty(EmulatorPlatform?.CommandLine))
                    args = $"{args} {EmulatorPlatform.CommandLine}".Trim(' ');

                if (!string.IsNullOrEmpty(Emulator.CommandLine))
                    args = $"{args} {Emulator.CommandLine}".Trim(' ');

                return $"{args} {RomPath.InDblQuotes()}".Trim(' ');
            }
        }
    }
}

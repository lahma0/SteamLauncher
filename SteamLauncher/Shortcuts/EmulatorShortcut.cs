using System.Linq;
using SteamLauncher.Tools;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace SteamLauncher.Shortcuts
{
    public class EmulatorShortcut : GameShortcut
    {
        public EmulatorShortcut(IGame iGame, IEmulator iEmulator = null, string overrideRomPath = null) : base(iGame)
        {
            _emulator = iEmulator;
            OverrideRomPath = overrideRomPath;
        }

        private readonly IEmulator _emulator;
        public IEmulator Emulator
        {
            get
            {
                if (_emulator == null)
                    return PluginHelper.DataManager.GetEmulatorById(LaunchBoxGame.EmulatorId);

                return _emulator;
            }
        }

        public IEmulatorPlatform EmulatorPlatform => Emulator.GetAllEmulatorPlatforms().FirstOrDefault(p => p.Platform == PlatformName);

        public string RomPath
        {
            get
            {
                var romPath = LaunchBoxGame.ApplicationPath;
                if (!string.IsNullOrEmpty(OverrideRomPath))
                    romPath = OverrideRomPath;

                return Utilities.GetAbsolutePath(romPath);
            }
        }

        public string OverrideRomPath { get; }

        public override string LaunchExePath => Utilities.GetAbsolutePath(Emulator.ApplicationPath);

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

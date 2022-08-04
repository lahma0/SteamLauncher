﻿using System.IO;
using SteamLauncher.Tools;
using Unbroken.LaunchBox.Plugins.Data;

namespace SteamLauncher.Shortcuts
{
    public class ScummVmShortcut : GameShortcut
    {
        public ScummVmShortcut(IGame iGame) : base(iGame)
        {
            
        }

        public string ScummVmGameDataPath => Utilities.GetAbsolutePath(LaunchBoxGame.ScummVmGameDataFolderPath);

        public string ScummVmGameType => LaunchBoxGame.ScummVmGameType;

        public override string LaunchExePath => Path.Combine(Info.LaunchBoxDir, "ThirdParty\\ScummVM\\scummvm.exe");

        public override string LaunchArguments
        {
            get
            {
                var args = "";
                if (LaunchBoxGame.ScummVmAspectCorrection)
                    args = $"{args} --aspect-ratio".Trim(' ');

                if (LaunchBoxGame.ScummVmFullscreen)
                    args = $"{args} --fullscreen".Trim(' ');

                if (!string.IsNullOrEmpty(ScummVmGameDataPath))
                    args = $"{args} --path={ScummVmGameDataPath.InDblQuotes()}".Trim(' ');

                if (!string.IsNullOrEmpty(ScummVmGameType))
                    args = $"{args} {ScummVmGameType}".Trim(' ');

                return args.Trim(' ');
            }
        }
    }
}

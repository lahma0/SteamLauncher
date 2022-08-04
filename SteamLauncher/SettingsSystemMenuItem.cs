using System.Drawing;
using SteamLauncher.Logging;
using SteamLauncher.Tools;
using SteamLauncher.UI.Views;
using Unbroken.LaunchBox.Plugins;

namespace SteamLauncher
{
    public class SettingsSystemMenuItem : ISystemMenuItemPlugin
    {
        public SettingsSystemMenuItem()
        {
            Logger.Info($"{nameof(SettingsSystemMenuItem)} loaded.");
            
            SLInit.Init();
        }

        public void OnSelected()
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        public string Caption => "SteamLauncher Settings";

        public Image IconImage => Resources.Logo2_32_Image;
        //public Image IconImage => Image.FromStream(Resources.Logo2_32?.Stream);

        public bool ShowInLaunchBox { get; set; } = true;
        public bool ShowInBigBox { get; set; } = true;
        public bool AllowInBigBoxWhenLocked { get; set; } = true;
    }
}

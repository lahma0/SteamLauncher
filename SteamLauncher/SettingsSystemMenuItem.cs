using System.Drawing;
using SteamLauncher.DataStore;
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
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        public string Caption => PluginHelper.StateManager.IsBigBox ? "SteamLauncher Settings (Req KB/Mouse)" : "SteamLauncher Settings";

        public Image IconImage => Resources.Logo2_32_Image;
        //public Image IconImage => Image.FromStream(Resources.Logo2_32?.Stream);

        public bool ShowInLaunchBox => Settings.Config.SystemMenuItems.Settings.ShowInLaunchBox;
        public bool ShowInBigBox => Settings.Config.SystemMenuItems.Settings.ShowInBigBox;
        public bool AllowInBigBoxWhenLocked => Settings.Config.SystemMenuItems.Settings.AllowInBigBoxWhenLocked;
    }
}

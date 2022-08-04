using System.Drawing;
using SteamLauncher.Logging;
using SteamLauncher.Proxy;
using SteamLauncher.DataStore;
using SteamLauncher.Tools;
using Unbroken.LaunchBox.Plugins;

namespace SteamLauncher
{
    public class UseSteamLauncherSystemMenuItem : ISystemMenuItemPlugin
    {
        public UseSteamLauncherSystemMenuItem()
        {
            Logger.Info($"{nameof(UseSteamLauncherSystemMenuItem)} loaded - '{nameof(Settings.Config.UniversalSteamLaunching)}' " + 
                        $"is set to {Settings.Config.UniversalSteamLaunching}.");

            SLInit.Init();
        }

        public void OnSelected()
        {
            Settings.Config.UniversalSteamLaunching = !Settings.Config.UniversalSteamLaunching;

            Logger.Info($"Universal Steam Launching was toggled " + 
                        $"{(Settings.Config.UniversalSteamLaunching ? "ON" : "OFF")}");

            // Run Proxy emu cleanup in case universal launching is disabled in the middle of interception
            if (!Settings.Config.UniversalSteamLaunching)
            {
                ProxyEmulator.KillProxyProcesses();
                ProxyEmulator.RestoreAppPaths();
            }

            // Workaround to get the menu item text to update in BigBox.
            // Note: The dynamic type is REALLY useful to access unexposed properties like this.
            dynamic bigBoxMainViewModel = PluginHelper.BigBoxMainViewModel;
            if (bigBoxMainViewModel != null)
                bigBoxMainViewModel.SystemViewModel.SelectedMenuItem.Text = Caption;
        }

        public void ShowMenuItem()
        {
            ShowInLaunchBox = true;
            ShowInBigBox = true;
        }

        public void HideMenuItem()
        {
            ShowInLaunchBox = false;
            ShowInBigBox = false;
        }

        public string Caption => Settings.Config.UniversalSteamLaunching ? "Use SteamLauncher (ON)" : "Use SteamLauncher (OFF)";

        public Image IconImage => Resources.Logo2_32_Image;
        //public Image IconImage => Image.FromStream(Resources.Logo2_32?.Stream);
        public bool ShowInLaunchBox { get; set; } = true;
        public bool ShowInBigBox { get; set; } = true;
        public bool AllowInBigBoxWhenLocked => true;
    }
}

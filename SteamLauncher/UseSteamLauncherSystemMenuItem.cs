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
            Logger.Info($"{nameof(UseSteamLauncherSystemMenuItem)} loaded - " + 
                        $"'{nameof(Settings.Config.UniversalSteamLaunching)}' " + 
                        $"is set to {Settings.Config.UniversalSteamLaunching}.");
            
            SLInit.Init();
        }

        public void OnSelected()
        {
            Settings.Config.UniversalSteamLaunching = !Settings.Config.UniversalSteamLaunching;

            Logger.Info($"{nameof(Settings.Config.UniversalSteamLaunching)} was toggled " + 
                        $"{(Settings.Config.UniversalSteamLaunching ? "ON" : "OFF")}");

            // Run Proxy emu cleanup in case universal launching is disabled in the middle of interception
            if (!Settings.Config.UniversalSteamLaunching)
            {
                ProxyEmulator.KillProxyProcesses();
                ProxyEmulator.RestoreAppPaths();
            }

            UpdateBigBoxCaption();
        }

        public void UpdateBigBoxCaption()
        {
            if (!PluginHelper.StateManager.IsBigBox)
                return;

            try
            {
                // Workaround to get the menu item text to update in BigBox.
                // Note: The dynamic type is REALLY useful to access unexposed properties like this.
                dynamic bigBoxMainViewModel = PluginHelper.BigBoxMainViewModel;
                
                // Even though 'ActiveViewModel' appears to be empty/invalid in Locals when debugging, 
                // it does indeed point to a valid instance of 'SystemViewModel'. You can access its 
                // properties/methods using the Immediate Window.
                if (bigBoxMainViewModel != null)
                    bigBoxMainViewModel.ActiveViewModel.SelectedMenuItem.Text = Caption;
                    
                // This is how it used to be accessed in an older version of BigBox.
                //bigBoxMainViewModel.SystemViewModel.SelectedMenuItem.Text = Caption;
            }
            catch
            {
                Logger.Warning($"Could not update {nameof(UseSteamLauncherSystemMenuItem)} text in BigBox.");
            }
        }

        public void UpdateLaunchBoxCaption()
        {
            if (PluginHelper.StateManager.IsBigBox)
                return;

            try
            {
                //dynamic launchBoxToolsMenu = Unbroken.LaunchBox.Windows.Desktop.ContextMenus.ToolsMenu.Instance;

                dynamic launchBoxMainViewModel = PluginHelper.LaunchBoxMainViewModel;
                if (launchBoxMainViewModel != null)
                {
                    var selectedItems = launchBoxMainViewModel.SelectedItems;
                    if (selectedItems != null)
                    {
                        var firstItem = selectedItems.FirstOrDefault();
                        if (firstItem != null)
                        {
                            firstItem.Text = Caption;
                        }
                    }
                    
                    //launchBoxMainViewModel.SelectedItems.FirstOrDefault().Text = Caption;
                }
            }
            catch
            {
                Logger.Warning($"Could not update {nameof(UseSteamLauncherSystemMenuItem)} text in LaunchBox.");
            }
        }

        public string Caption => $"{(Settings.Config.UniversalSteamLaunching ? "Disable" : "Enable")} SteamLauncher";
        //public string Caption => Settings.Config.UniversalSteamLaunching ? "Use SteamLauncher (ON)" : "Use SteamLauncher (OFF)";

        public Image IconImage => Resources.Logo2_32_Image;
        //public Image IconImage => Image.FromStream(Resources.Logo2_32?.Stream);
        public bool ShowInLaunchBox => Settings.Config.SystemMenuItems.EnableSteamLauncher.ShowInLaunchBox;
        public bool ShowInBigBox => Settings.Config.SystemMenuItems.EnableSteamLauncher.ShowInBigBox;
        public bool AllowInBigBoxWhenLocked => Settings.Config.SystemMenuItems.EnableSteamLauncher.AllowInBigBoxWhenLocked;
    }
}

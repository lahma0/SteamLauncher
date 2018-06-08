using System;
using System.Drawing;
using System.IO;
using SteamLauncher.Logging;
using SteamLauncher.Settings;
using SteamLauncher.Shortcuts;
using SteamLauncher.SteamClient;
using SteamLauncher.SteamClient.Native;
using SteamLauncher.Tools;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace SteamLauncher
{
    /// <summary>
    /// Shows the 'Launch via Steam' custom menu item inside LaunchBox's game context menus.
    /// </summary>
    public class LaunchViaSteamMenuItem : IGameMenuItemPlugin
    {
        /// <summary>
        /// Contains DateTime When LaunchBox process was last brought into foreground
        /// </summary>
        private DateTime _lastTimeLbInForeground;

        private readonly ActiveWindow _activeWindow;

        public LaunchViaSteamMenuItem()
        {
            var defaultConfigLoaded = false;

            try
            {
                // Try to load the plugin's configuration file
                Config.Load();
            }
            catch (FileNotFoundException ex)
            {
                // Load default values if no configuration file is found
                Config.Default();
                defaultConfigLoaded = true;
            }

            // Enable debug logging if enabled in the config
            if (Config.Instance.DebugLogEnabled)
            {
                try
                {
                    Logger.EnableDebugLog(Config.LOG_PATH);
                }
                catch (Exception ex)
                {
                    Logger.Error("An error occurred while attempting to enable debug logging to file. " + 
                                 $"Debug messages will only be output to console. Exception: {ex.Message}");
                }
                
            }
            
            if (defaultConfigLoaded)
                Logger.Warning($"The configuration file '{Config.CONFIG_FILENAME}' could not be found. Loading defaults.");
            
            Logger.Info($"SteamLauncher plugin loaded ({(SysNative.Is64Bit() ? "64-bit" : "32-bit")} mode).");

            _activeWindow = new ActiveWindow();
        }

        ~LaunchViaSteamMenuItem()
        {
            Config.Instance.Save();
        }

        private void EnableActiveWindowHook()
        {
            Logger.Info("Enabling ActiveWindow hook.");
            _activeWindow.EnableHook();
            _activeWindow.ActiveWindowChanged += OnActiveWindowChanged;
        }

        private void DisableActiveWindowHook()
        {
            Logger.Info("Disabling ActiveWindow hook.");
            _activeWindow.DisableHook();
            _activeWindow.ActiveWindowChanged -= OnActiveWindowChanged;
        }

        /// <summary>
        /// Event handler that monitors Windows foreground window changes in order to prevent the annoying problem of 
        /// Steam stealing focus from LB after some games exit (at which time LB/BB should be the active Window). 
        /// This is only enabled if the 'PreventSteamFocusStealing' setting is enabled. 
        /// </summary>
        /// <param name="sender">The object who issued the event.</param>
        /// <param name="windowHeader">The title of the newly activated window.</param>
        /// <param name="hwnd">The handle of the newly activated window.</param>
        private void OnActiveWindowChanged(object sender, string windowHeader, IntPtr hwnd)
        {
            if (hwnd == Utilities.GetLaunchBoxProcess().MainWindowHandle)
            {
                _lastTimeLbInForeground = DateTime.Now;
                return;
            }

            if (hwnd == SteamProcessInfo.SteamProcess.MainWindowHandle)
            {
                var timeSinceLbGotFocus = DateTime.Now.Subtract(_lastTimeLbInForeground);
                if (timeSinceLbGotFocus >= TimeSpan.FromSeconds(Config.Instance.TotalSecondsToPreventSteamFocus))
                    return;

                Logger.Info("Forcing LaunchBox window into the foreground because Steam window stole focus.");
                ActiveWindow.SetForegroundWindow(Utilities.GetLaunchBoxProcess().MainWindowHandle);
                DisableActiveWindowHook();
            }
        }

        /// <summary>
        /// Indicates whether this custom menu item supports multiple games being selected simultaneously.
        /// </summary>
        public bool SupportsMultipleGames => false;

        /// <summary>
        /// Determines if this custom menu item should be enabled in BigBox mode.
        /// </summary>
        public bool ShowInBigBox => true;

        /// <summary>
        /// Determines if this custom menu item should be enabled in the normal LaunchBox interface.
        /// </summary>
        public bool ShowInLaunchBox => true;

        /// <summary>
        /// The icon shown next to the text label for this custom menu item.
        /// </summary>
        public Image IconImage => Properties.Resources.SteamIcon;

        /// <summary>
        /// The text label for this custom menu item.
        /// </summary>
        public string Caption => "Launch via Steam";

        /// <summary>
        /// Executes when a single game is selected and decides whether this custom menu item should be enabled.
        /// </summary>
        /// <param name="selectedGame">The currently selected game's class info.</param>
        /// <returns></returns>
        public bool GetIsValidForGame(IGame selectedGame)
        {
            return true;
        }

        /// <summary>
        /// Executes when multiple games are selected simultaneously and decides whether this custom menu item should be enabled.
        /// </summary>
        /// <param name="selectedGames">An array of the currently selected game's class info.</param>
        /// <returns>A bool value which determines whether this custom menu item should be enabled.</returns>
        public bool GetIsValidForGames(IGame[] selectedGames) { throw new NotImplementedException(); }

        /// <summary>
        /// This callback executes whenever a single game is selected and this custom menu item is clicked .
        /// </summary>
        /// <param name="selectedGame">The currently selected game's class info.</param>
        public void OnSelected(IGame selectedGame)
        {
            if (selectedGame == null)
                return;

            var gameShortcut = GameShortcut.CreateGameShortcut(selectedGame);
            var steamShortcut = gameShortcut.GenerateSteamShortcut();
            steamShortcut.LaunchShortcut();

            if (Config.Instance.PreventSteamFocusStealing)
            {
                EnableActiveWindowHook();
            }
        }

        /// <summary>
        /// This callback executes whenever there are multiple games selected simultaneously and this custom menu item is clicked.
        /// </summary>
        /// <param name="selectedGames">An array of the currently selected games' class info.</param>
        public void OnSelected(IGame[] selectedGames) { throw new NotImplementedException(); }
    }
}

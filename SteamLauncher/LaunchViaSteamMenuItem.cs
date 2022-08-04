using System;
using System.Drawing;
using SteamLauncher.Logging;
using SteamLauncher.DataStore;
using SteamLauncher.Shortcuts;
using SteamLauncher.SteamClient;
using SteamLauncher.Tools;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace SteamLauncher
{
    ///// <summary>
    ///// Shows the 'Launch via Steam' custom menu item inside LaunchBox's game context menus.
    ///// </summary>
    //public class LaunchViaSteamMenuItem : IGameMenuItemPlugin
    //{
    //    /// <summary>
    //    /// Contains DateTime When LaunchBox process was last brought into foreground
    //    /// </summary>
    //    private DateTime _lastTimeLbInForeground = DateTime.Now;

    //    private IntPtr _lastForegroundWindowHandle = IntPtr.Zero;

    //    private readonly ActiveWindow _activeWindow;

    //    public bool IsWindowHookEnabled { get; private set; }

    //    public LaunchViaSteamMenuItem()
    //    {
    //        Logger.Info($"{nameof(LaunchViaSteamMenuItem)} loaded.");

    //        SLInit.Init();

    //        //var steamLauncherPath = Info.SteamLauncherPath;
    //        //Logger.Info($"SteamLauncher v{Info.GetSteamLauncherVersion()} loaded " + 
    //        //            $"({(SysNative.Is64Bit() ? "64-bit" : "32-bit")} mode) from '{steamLauncherPath}'.");

    //        //if (!steamLauncherPath.ToLower().EndsWith(@"launchbox\plugins\steamlauncher\steamlauncher.dll"))
    //        //{
    //        //    Logger.Warning($"It appears that SteamLauncher may be executing from an unexpected location which " + 
    //        //                   "could indicate that multiple copies of the plugin are present within the LaunchBox " +
    //        //                   "directory structure. LaunchBox will load the plugin, regardless of its name (as " + 
    //        //                   "long as it has a .dll extension), if it is ANYWHERE within the LaunchBox " + 
    //        //                   "directory structure, including its root directory or ANY subdirectories. Be " + 
    //        //                   "sure that the only instance of the plugin is located at " + 
    //        //                   "'LaunchBox\\Plugins\\SteamLauncher\\SteamLauncher.dll' and then restart " + 
    //        //                   "LaunchBox. Unexpected behavior will likely occur until this is resolved.");
    //        //}

    //        _activeWindow = ActiveWindow.Instance;
    //    }

    //    ~LaunchViaSteamMenuItem()
    //    {
            
    //    }

    //    private void EnableActiveWindowHook()
    //    {
    //        if (IsWindowHookEnabled)
    //            return;

    //        Logger.Info($"{nameof(LaunchViaSteamMenuItem)} is subscribing to {nameof(ActiveWindow.ActiveWindowChanged)}.");
    //        _activeWindow.ActiveWindowChanged += OnActiveWindowChanged;
    //        IsWindowHookEnabled = true;
    //    }

    //    private void DisableActiveWindowHook()
    //    {
    //        if (!IsWindowHookEnabled)
    //            return;

    //        Logger.Info($"{nameof(LaunchViaSteamMenuItem)} is unsubscribing from {nameof(ActiveWindow.ActiveWindowChanged)}.");
    //        _activeWindow.ActiveWindowChanged -= OnActiveWindowChanged;
    //        IsWindowHookEnabled = false;
    //    }

    //    /// <summary>
    //    /// Event handler that monitors Windows foreground window changes in order to prevent the annoying problem of 
    //    /// Steam stealing focus from LB after some games exit (at which time LB/BB should be the active Window). 
    //    /// This is only enabled if the 'PreventSteamFocusStealing' setting is enabled. 
    //    /// </summary>
    //    /// <param name="sender">The object who issued the event.</param>
    //    /// <param name="windowHeader">The title of the newly activated window.</param>
    //    /// <param name="hwnd">The handle of the newly activated window.</param>
    //    private void OnActiveWindowChanged(object sender, string windowHeader, IntPtr hwnd)
    //    {
    //        var lbMainWindowHandle = Info.LaunchBoxProcess.MainWindowHandle;

    //        if (hwnd == lbMainWindowHandle)
    //        {
    //            _lastTimeLbInForeground = DateTime.Now;
    //        }
    //        else if (hwnd == SteamProcessInfo.SteamProcess.MainWindowHandle)
    //        {
    //            var timeSinceLbGotFocus = DateTime.Now.Subtract(_lastTimeLbInForeground);

    //            if (timeSinceLbGotFocus <= TimeSpan.FromSeconds(Settings.Config.TotalSecondsToPreventSteamFocus))
    //            {
    //                Logger.Info("Forcing LaunchBox window into the foreground because Steam window stole focus.");
    //                WindowMgmt.SetForegroundWindow(lbMainWindowHandle);
    //                DisableActiveWindowHook();
    //            }
    //        }

    //        _lastForegroundWindowHandle = hwnd;
    //    }

    //    /// <summary>
    //    /// Forces Steam window to the background (bottom of Z-order).
    //    /// </summary>
    //    private static void SendSteamWindowToBackground()
    //    {
    //        Logger.Info($"Forcing Steam window into the background because config item '{nameof(Settings.Config.PreventSteamFocusStealing)}' is enabled.");

    //        var steamWindowHandle = SteamProcessInfo.SteamProcess.MainWindowHandle;
    //        if (steamWindowHandle == IntPtr.Zero)
    //            return;

    //        Logger.Info($"Calling '{nameof(WindowMgmt.SetWindowPos)}' on window handle '{steamWindowHandle.ToInt64():X}'.");

    //        WindowMgmt.SetWindowPos(
    //            steamWindowHandle,
    //            WindowMgmt.HWND_BOTTOM,
    //            0, 0, 0, 0,
    //            WindowMgmt.SetWindowPosFlags.SWP_NOMOVE |
    //            WindowMgmt.SetWindowPosFlags.SWP_NOSIZE |
    //            WindowMgmt.SetWindowPosFlags.SWP_NOACTIVATE |
    //            WindowMgmt.SetWindowPosFlags.SWP_ASYNCWINDOWPOS);
    //    }

    //    /// <summary>
    //    /// Indicates whether this custom menu item supports multiple games being selected simultaneously.
    //    /// </summary>
    //    public bool SupportsMultipleGames => false;

    //    /// <summary>
    //    /// Determines if this custom menu item should be enabled in BigBox mode.
    //    /// </summary>
    //    public bool ShowInBigBox => false;
    //    //public bool ShowInBigBox => !Settings.Config.UniversalSteamLaunching;

    //    /// <summary>
    //    /// Determines if this custom menu item should be enabled in the normal LaunchBox interface.
    //    /// </summary>
    //    public bool ShowInLaunchBox => false;
    //    //public bool ShowInLaunchBox => !Settings.Config.UniversalSteamLaunching;

    //    /// <summary>
    //    /// The icon shown next to the text label for this custom menu item.
    //    /// </summary>
    //    public Image IconImage => Resources.Logo2_32_Image;
    //    //public Image IconImage => Image.FromStream(Resources.Logo2_32?.Stream);

    //    /// <summary>
    //    /// The text label for this custom menu item.
    //    /// </summary>
    //    public string Caption => "Launch via Steam";

    //    /// <summary>
    //    /// Executes when a single game is selected and decides whether this custom menu item should be enabled.
    //    /// </summary>
    //    /// <param name="selectedGame">The currently selected game's class info.</param>
    //    /// <returns></returns>
    //    public bool GetIsValidForGame(IGame selectedGame)
    //    {
    //        return true;
    //    }

    //    /// <summary>
    //    /// Executes when multiple games are selected simultaneously and decides whether this custom menu item should be enabled.
    //    /// </summary>
    //    /// <param name="selectedGames">An array of the currently selected game's class info.</param>
    //    /// <returns>A bool value which determines whether this custom menu item should be enabled.</returns>
    //    public bool GetIsValidForGames(IGame[] selectedGames) { throw new NotImplementedException(); }

    //    /// <summary>
    //    /// This callback executes whenever a single game is selected and this custom menu item is clicked .
    //    /// </summary>
    //    /// <param name="selectedGame">The currently selected game's class info.</param>
    //    public void OnSelected(IGame selectedGame)
    //    {
    //        if (selectedGame == null)
    //            return;

    //        GameShortcut gameShortcut = null;
    //        SteamShortcutManager steamShortcut = null;

    //        try
    //        {
    //            gameShortcut = GameShortcut.CreateGameShortcut(selectedGame);
    //            steamShortcut = gameShortcut.GenerateSteamShortcut();
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.Error($"An error occurred while generating the Steam shortcut: {ex.Message}");
    //        }

    //        if (gameShortcut == null || steamShortcut == null)
    //            return;

    //        IEmulator emulator = null;
    //        if (gameShortcut.GetType() == typeof(EmulatorShortcut))
    //        {
    //            emulator = ((EmulatorShortcut)gameShortcut).Emulator;
    //            emulator.ApplicationPath = SteamProcessInfo.SteamExePath;
    //        }

    //        var overrideCmdLine = steamShortcut.ShortcutUrl;

    //        try
    //        {
    //            PluginHelper.LaunchBoxMainViewModel.PlayGame(gameShortcut.LaunchBoxGame, null, emulator, overrideCmdLine);
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.Error($"An error occurred while launching the Steam shortcut: {ex.Message}");
    //            return;
    //        }

    //        if (Settings.Config.PreventSteamFocusStealing && Settings.Config.TotalSecondsToPreventSteamFocus > 0)
    //        {
    //            EnableActiveWindowHook();
    //        }
    //    }

    //    /// <summary>
    //    /// This callback executes whenever there are multiple games selected simultaneously and this custom menu item is clicked.
    //    /// </summary>
    //    /// <param name="selectedGames">An array of the currently selected games' class info.</param>
    //    public void OnSelected(IGame[] selectedGames) { throw new NotImplementedException(); }
    //}
}

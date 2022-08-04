using System;
using System.Runtime.CompilerServices;
using SteamLauncher.Logging;
using SteamLauncher.Proxy;
using SteamLauncher.DataStore;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace SteamLauncher
{
    /// <summary>
    /// This class is responsible for "hooking" the launch process of any game/emulator by LaunchBox/BigBox.
    /// Unfortunately, LB's plugin API does not provide a way to modify the game launch path/parameters within
    /// 'OnBeforeGameLaunching', so in order to "hijack" the launch process, we have to get pretty creative, and work
    /// outside the bounds of "supported" behavior. Here a simple overview of the actions performed within this class.
    /// Inside of 'OnBeforeGameLaunching', we change the exe path of the LB game/emulator to point to our generic "proxy"
    /// exe (there is of course a very rigorous process of backing up the original path and restoring it, even if LB
    /// crashes). LB will think the proxy exe is actually the real game exe, so it will proceed to launch that as the
    /// actual game process. The hidden proxy exe will run alongside the game exe for the entire duration that the game
    /// is running. Inside of 'ProxyEmulator', we pull the command line and parameters from the proxy exe and use that to
    /// run the "real" exe through Steam. We also monitor the proxy exe and detect whenever LB tries to bring it into the
    /// foreground, so that we can do the same with the "real" exe (this maintains compatibility with LB's
    /// startup/shutdown screens). Immediately after LB launches the proxy exe, the 'OnAfterGameLaunched' method will be
    /// called, inside of which we can swap the game/emulators exe path back to the original. There are a lot of
    /// "gotchas" and workarounds that can be explored by looking at the code, but on a basic level, this is how the
    /// process works. Most of the implementation code is in 'ProxyEmulator'. This class is mainly used to detect the
    /// "state" of the game launching process.
    /// </summary>
    public class GameLaunchingPlugin : IGameLaunchingPlugin
    {
        private ProxyEmulator _proxyEmu;

        private ProxyEmulator ProxyEmu
        {
            get => _proxyEmu;
            set
            {
                _proxyEmu?.DisposeProxyEmulator();
                _proxyEmu = value;
            }
        }

        private ActivityState InterceptLaunchState { get; set; }

        public GameLaunchingPlugin()
        {
            Logger.Info($"{nameof(GameLaunchingPlugin)} loaded.");
            
            SLInit.Init();

            // Close any instances of SteamLauncherProxy.exe that are still running
            // as a result of a hard crash of LB/BB
            ProxyEmulator.KillProxyProcesses();

            // Check if cleanup is needed due to improper shutdown of LB/BB
            ProxyEmulator.RestoreAppPaths();
        }

        ~GameLaunchingPlugin()
        {
            if (ProxyEmu != null)
            {
                ProxyEmu.DisposeProxyEmulator();
            }
            else
            {
                ProxyEmulator.KillProxyProcesses();
                ProxyEmulator.RestoreAppPaths();
            }
        }

        /// <summary>
        /// LB Plugin API function called before a game has launched.
        /// </summary>
        /// <param name="game">Game to be launched.</param>
        /// <param name="app">AdditionalApplication to be run before/after game is launched.</param>
        /// <param name="emulator">Emulator being used to launch the game.</param>
        public void OnBeforeGameLaunching(IGame game, IAdditionalApplication app, IEmulator emulator)
        {
            // Preventative measure to try to prevent some strange behavior 
            ProxyEmulator.RestoreAppPaths();

            // Log game launching info
            LogGameLaunchingInfo(game, app, emulator);
            
            // Reset 'Intercept Launch' state
            InterceptLaunchState = new ActivityState("Intercept Game Launch", "Before");
            
            // If universal Steam launching is disabled, cancel the launch interception
            if (!Settings.Config.UniversalSteamLaunching)
            {
                InterceptLaunchState.Cancel("Universal Steam Launching is disabled");
                return;
            }

            var doEnableSLForGameLaunch = Settings.Config.Filtering.IsSLEnabledForGameLaunch(game, app, emulator, out var reason);
            if (!doEnableSLForGameLaunch)
            {
                InterceptLaunchState.Cancel($"Steam Launcher will NOT intercept this game launch. Reason: {reason}");
                return;
            }

            Logger.Info($"Steam Launcher WILL intercept this game launch. Reason: {reason}");

            // If game.EmulatorId has a value (uses emulation) but emulator is null, this means LB cannot find the 
            // required emu (prob deleted by user and game hasn't been assigned a different emu yet). LB will 
            // show a warning msg but the plugin will get confused, so we detect this early and cancel the launch.
            if (!string.IsNullOrEmpty(game.EmulatorId) && emulator == null)
            {
                InterceptLaunchState.Cancel("The game uses emulation but the designated emulator does not exist.");
                return;
            }

            ProxyEmu = new ProxyEmulator(game, emulator);
            
            try
            {
                ProxyEmu.SetLauncherExeToProxy();
            }
            catch (Exception ex)
            {
                // Swapping the app path failed; put why in the CancelledReason msg (logged in OnAfterGameLaunched)
                InterceptLaunchState.Cancel($"Failed to swap launcher application path with proxy - {ex.Message}");
            }
        }

        /// <summary>
        /// LB Plugin API function called after a game is launched.
        /// </summary>
        /// <param name="game">Game that was launched.</param>
        /// <param name="app">AdditionalApplication to be run before/after the game.</param>
        /// <param name="emulator">Emulator that was used to launch the game.</param>
        public void OnAfterGameLaunched(IGame game, IAdditionalApplication app, IEmulator emulator)
        {
            // Log game launching info
            LogGameLaunchingInfo(game, app, emulator);

            // Update 'Intercept Launch' state
            InterceptLaunchState.CurrentState = "After";

            // If the game launch intercept was cancelled, log the reason and cleanup
            if (InterceptLaunchState.IsCancelled)
            {
                // Log the reason the intercept was cancelled only if 'Universal Steam Launching' is enabled
                // (we don't need a log entry on every launch if the feature is disabled)
                if (Settings.Config.UniversalSteamLaunching)
                {
                    Logger.Info($"The game launch intercept was cancelled - Reason: {InterceptLaunchState.CancelledReason}");
                }

                ProxyEmu?.DisposeProxyEmulator();
                ProxyEmu = null;
                return;
            }

            // Above statement should cover this (here as fallback on the off chance the methods are run out of order)
            if (!Settings.Config.UniversalSteamLaunching)
                return;

            // Restore correct exe path BEFORE creating/launching the Steam shortcut
            ProxyEmulator.RestoreAppPaths();

            try
            {
                // Create/launch the Steam shortcut
                ProxyEmu.CreateLaunchSteamShortcut();
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while creating/launching the Steam shortcut: {ex.Message}");
                ProxyEmu?.DisposeProxyEmulator();
                ProxyEmu = null;
            }
        }

        public void OnGameExited()
        {
            // Update 'Intercept Launch' state
            InterceptLaunchState.CurrentState = "Exited";

            // If Universal Steam Launching is enabled, this will not be called until ProxyEmulator.KillProxyProcesses() is called
            Logger.Info("GameLaunchingPlugin:OnGameExited()");
            ProxyEmu?.DisposeProxyEmulator();
            ProxyEmu = null;
        }

        /// <summary>
        /// Helper function for outputting game launching info to the log. Used exclusively for 'OnBeforeGameLaunching' and 'OnAfterGameLaunched'.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="app"></param>
        /// <param name="emulator"></param>
        /// <param name="memberName"></param>
        private static void LogGameLaunchingInfo(IGame game, IAdditionalApplication app, IEmulator emulator,
                                                 [CallerMemberName] string memberName = null)
        {
            if (memberName == null)
                return;

            var output = $"{nameof(GameLaunchingPlugin)}:{memberName}()";
            if (game != null)
                output += $" - Game: {game.Title} '{game.ApplicationPath}'";
            if (emulator != null)
                output += $" - Emulator: {emulator.Title} '{emulator.ApplicationPath}'";
            if (app != null)
                output += $" - AdditionalApp: {app.Name} '{app.ApplicationPath}'";

            Logger.Info(output, memberName: memberName);
        }
    }
}

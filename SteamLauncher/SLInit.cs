using System;
using SteamLauncher.Logging;
using SteamLauncher.DataStore;
using SteamLauncher.SteamClient;
using SteamLauncher.SteamClient.Native;
using SteamLauncher.Tools;

namespace SteamLauncher
{
    public static class SLInit
    {
        static SLInit()
        {
            Logger.Info($"SteamLauncher v{Info.GetSteamLauncherVersion()} loaded " +
                        $"({(SysNative.Is64Bit() ? "64-bit" : "32-bit")} mode) from '{Info.SteamLauncherPath}'.");

            if (!Info.SteamLauncherPath.ToLower().EndsWith(@"launchbox\plugins\steamlauncher\steamlauncher.dll"))
            {
                Logger.Warning($"It appears that SteamLauncher may be executing from an unexpected location which " +
                               "could indicate that multiple copies of the plugin are present within the LaunchBox " +
                               "directory structure. LaunchBox will load the plugin, regardless of its name (as " +
                               "long as it has a .dll extension), if it is ANYWHERE within the LaunchBox " +
                               "directory structure, including its root directory or ANY subdirectories. Be " +
                               "sure that the only instance of the plugin is located at " +
                               "'LaunchBox\\Plugins\\SteamLauncher\\SteamLauncher.dll' and then restart " +
                               "LaunchBox. Unexpected behavior will likely occur until this is resolved.");
            }

            // Disabled after switching over to 'Microsoft.Management.Infrastructure'
            //// Setup resolver for assemblies that cannot be found (primarily for locating System.Management.dll)
            //AssemblyLoadContext.Default.Resolving += Utilities.ResolvingHandler;

            // Disabled after switching over to 'Microsoft.Management.Infrastructure'
            //// Go ahead and forcibly load 'System.Management.dll' if it exists in the specified location and is not
            //// already loaded; otherwise, the resolver set up above will automatically locate the assembly and load the
            //// correct one.
            //if (AssemblyLoadContext.Default.Assemblies.All(x => x.GetName().Name?.ToLower() != "system.management"))
            //{
            //    var fileInfo = new FileInfo(Path.Join(Info.LaunchBoxCoreDir,
            //                                          "runtimes",
            //                                          "win",
            //                                          "lib",
            //                                          "netcoreapp2.0",
            //                                          "System.Management.dll"));
            //    if (fileInfo.Exists)
            //        Utilities.LoadAssembly(fileInfo.FullName);
            //}

            AppDomain.CurrentDomain.ProcessExit += ProcessExit;

            //// Ensure VTableInfo has a chance to do an online update prior to initializing Steam
            //var vti = VtFile.Instance;
            //vti.Save();

            // Ensure that Steam is running (and if it isn't, start it)
            SteamProcessInfo.StartSteamAsync();
        }

        /// <summary>
        /// There is no way of knowing which class LaunchBox will instantiate first when loading SteamLauncher, so we
        /// call this method (which will cause the constructor [SLInit()] to be run) from each plugin class
        /// (GameLaunchingPlugin / LaunchViaSteamMenuItem / SettingsSystemMenuItem / UseSteamLauncherSystemMenuItem)
        /// initializer to ensure this class is instantiated as early as possible.
        /// </summary>
        public static void Init()
        {
            
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            Settings.Config.Save();
            Settings.Repairs.Save();
            Settings.VTables.Save();
            SteamContext.Instance.Dispose();
        }
    }
}

using SteamLauncher.Attributes;
using SteamLauncher.DataStore.SelectiveUse;
using SteamLauncher.DataStore.VTablesStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using SteamLauncher.Logging;
using SteamLauncher.Tools;

namespace SteamLauncher.DataStore
{
    /// <summary>
    /// Saves all user preferences and configuration data.
    /// </summary>
    [XmlFilename("config.xml")]
    [XmlRoot("SteamLauncherConfig")]
    public class Config : XmlFile<Config>
    {
        #region Fields

        ///// <summary>
        ///// Name of the configuration file.
        ///// </summary>
        //[XmlIgnore]
        //public static readonly string CONFIG_FILENAME = "config.xml";

        ///// <summary>
        ///// The full path of the configuration file.
        ///// </summary>
        //[XmlIgnore]
        //public static readonly string CONFIG_PATH = Path.Combine(Info.SteamLauncherDir, CONFIG_FILENAME);

        /// <summary>
        /// Name of the debug log file.
        /// </summary>
        [XmlIgnore]
        public static readonly string LOG_FILENAME = "debug.log";

        /// <summary>
        /// The full path of the debug log file.
        /// </summary>
        [XmlIgnore]
        public static readonly string LOG_PATH = Path.Combine(Info.SteamLauncherDir, LOG_FILENAME);

        ///// <summary>
        ///// Name of the state data file.
        ///// </summary>
        //[XmlIgnore]
        //public static readonly string STATE_FILENAME = "state.xml";

        ///// <summary>
        ///// The full path of the state data file.
        ///// </summary>
        //[XmlIgnore]
        //public static readonly string STATE_PATH = Path.Combine(Info.SteamLauncherDir, STATE_FILENAME);

        #endregion

        #region Properties

        ///// <summary>
        ///// Defines whether the plugin should write debug messages to a log file for purposes of troubleshooting.
        ///// </summary>
        //public bool DebugLogEnabled { get; set; } = false;

        /// <summary>
        /// Defines the verbosity of log output.
        /// </summary>
        public TraceLevel LogLevel { get; set; } = TraceLevel.Error;

        /// <summary>
        /// Defines whether the plugin should intercept all game launches by default, redirecting them to launch 
        /// through Steam.
        /// </summary>
        public bool UniversalSteamLaunching { get; set; } = true;

        /// <summary>
        /// Whether to prevent Steam from stealing focus from LB/BB after a game exits.
        /// </summary>
        public bool PreventSteamFocusStealing { get; set; } = false;

        /// <summary>
        /// On most machines, this will not need to be enabled, but in rare cases, Steam will forcibly steal focus from
        /// LB/BB upon exit of a game. This typically happens about 3-4 seconds after the game window closes and the
        /// LB/BB window regains focus. If <see cref="PreventSteamFocusStealing"/> is enabled, this value defines the
        /// number of seconds Steam will be prevented from stealing focus. If set too high, this can prevent
        /// intentional activation of the Steam window.
        /// </summary>
        public int TotalSecondsToPreventSteamFocus { get; set; } = 6;

        /// <summary>
        /// Defines the valid range of values for the config property <see cref="TotalSecondsToPreventSteamFocus"/>.
        /// </summary>
        public static Range<int> TotalSecondsToPreventSteamFocusValidRange => new Range<int>(2, 15);

        /// <summary>
        /// When UniversalSteamLaunching is enabled, this defines how many seconds the plugin will wait for Steam  
        /// to launch a process before deciding that the action has timed out.
        /// </summary>
        public int ProcessStartTimeoutSec { get; set; } = 10;

        /// <summary>
        /// Defines the valid range of values for the config property <see cref="ProcessStartTimeoutSec"/>.
        /// </summary>
        public static Range<int> ProcessStartTimeoutSecValidRange => new Range<int>(5, 30);

        /// <summary>
        /// When UniversalSteamLaunching is enabled, this defines the interval between updates by ProcessWatcher, 
        /// a module used to monitor processes which are launched by Steam.
        /// </summary>
        public int ProcessWatcherPollingIntervalSec { get; set; } = 1;

        /// <summary>
        /// Defines the valid range of values for the config property <see cref="ProcessWatcherPollingIntervalSec"/>.
        /// </summary>
        public static Range<int> ProcessWatcherPollingIntervalSecValidRange => new Range<int>(1, 5);

        /// <summary>
        /// The DOSBox or ScummVM exe must be temporarily replaced with the plugin's proxy exe when using Universal
        /// Steam Launching. During this process, a thread performs several file operations (copies, renames, etc) and
        /// must wait briefly on the file system to complete these tasks before continuing. This sets the length of
        /// that delay in ms (occurs once per launch). In most cases, a value smaller than the default (400ms) would
        /// work fine, but the difference is so small as to be imperceptible by the user.
        /// </summary>
        public int DosBoxScummVmProxySleepMs { get; set; } = 400;

        /// <summary>
        /// Defines the valid range of values for the config property <see cref="DosBoxScummVmProxySleepMs"/>.
        /// </summary>
        public static Range<int> DosBoxScummVmProxySleepMsValidRange => new Range<int>(100, 2000);

        /// <summary>
        /// The relative path to the DOSBox exe included with LaunchBox. This should not need to be changed unless
        /// LaunchBox has changed the location of the DOSBox exe within its directory structure.
        /// </summary>
        public string DosBoxExePath { get; set; } = "ThirdParty\\DOSBox\\DOSBox.exe";

        /// <summary>
        /// The relative path to the ScummVM exe included with LaunchBox. This should not need to be changed unless
        /// LaunchBox has changed the location of the ScummVM exe within its directory structure.
        /// </summary>
        public string ScummVmExePath { get; set; } = "ThirdParty\\ScummVM\\scummvm.exe";

        /// <summary>
        /// User-defined list of custom platform names to replace predefined platform names provided by LaunchBox.
        /// Custom platform names will be seen in your 'Currently Playing' status in Steam. Example: SteamUser is
        /// currently playing 'Super Mario World (SNES)'."
        /// </summary>
        public List<Platform> CustomPlatformNames { get; set; } = new List<Platform>();

        /// <summary>
        /// A list of game launchers and the EXEs they are responsible for launching. For the plugin to operate, it
        /// must track when Steam games start/end. Some games use a launcher EXE to start the game EXE, after which the
        /// launcher EXE exits. In these cases, the plugin assumes the game exited since it only has knowledge of the
        /// launcher EXE. This list associates a game's launcher EXE with its main EXE so the plugin can correctly
        /// deduce when the game has exited. This functionality can be extended for more advanced tasks such as
        /// launching games through their client/store, loaders/patchers, virtualized/portable/sandboxed EXEs, etc.
        /// </summary>
        public List<LauncherToExe> LauncherToExeDefinitions { get; set; } = new List<LauncherToExe>();

        /// <summary>
        /// Allows users to dynamically enable/disable the plugin using a list of user-defined filters in either
        /// "whitelist" mode or "blacklist" mode. This feature is useful in cases where you want to disable
        /// SteamLauncher because specific games/emulators/launchers don't work properly with the Steam overlay. OFF:
        /// Disable this feature entirely. BLACKLIST: The plugin is always enabled unless a matching filter is found.
        /// WHITELIST: The plugin is always disabled unless a matching filter is found.
        /// </summary>
        public Filtering Filtering { get; set; } = new Filtering();

        /// <summary>
        /// Determines whether <see cref="VTables"/> will automatically update itself online.
        /// </summary>
        public bool AutoUpdateVTables { get; set; } = true;

        //public DateTime LastCheckForUpdatedVTables { get; set; } = DateTime.MinValue;

        #endregion

        //#region Singleton Constructor/Destructor

        //[XmlIgnore]
        //public static bool IsInitialized => Lazy.IsValueCreated;

        //[XmlIgnore]
        //private static readonly Lazy<Config> Lazy = new Lazy<Config>(LoadConfigOrDefaults);

        //[XmlIgnore]
        //public static Config Instance => Lazy.Value;

        //private Config()
        //{

        //}

        //~Config()
        //{
        //    // After moving to .NET Core, this destructor is no longer called when the app exits
        //    // File is now saved in SLInit.ProcessExit
        //}

        //#endregion

        #region Setup Defaults

        ///// <summary>
        ///// Gets a Config instance containing default values for all properties.
        ///// </summary>
        ///// <returns>A Config instance containing default values.</returns>
        //public static Config GetDefaultConfig()
        //{
        //    var config = new Config();
        //    config.CustomPlatformNames.AddRange(DefaultCustomPlatformNames);
        //    config.LauncherToExeDefinitions.AddRange(DefaultLauncherToExeDefinitions);
        //    return config;
        //}

        protected override void LoadDefaults()
        {
            CustomPlatformNames.AddRange(DefaultCustomPlatformNames);
            LauncherToExeDefinitions.AddRange(DefaultLauncherToExeDefinitions);
        }

        /// <summary>
        /// A list of default LauncherToExe definitions that will be used when a new config file is created.
        /// </summary>
        private static readonly List<LauncherToExe> DefaultLauncherToExeDefinitions = new List<LauncherToExe>()
        {
            new LauncherToExe()
            {
                Enable = true,
                LauncherFilename = "ScummVMLauncher.exe",
                TargetFilename = "scummvm.exe",
            },
        };

        /// <summary>
        /// A list of default Custom Platform Names that will be used when a new config file is created. 
        /// Most of these values are only for demonstrating the usage of the feature.
        /// </summary>
        private static readonly List<Platform> DefaultCustomPlatformNames = new List<Platform>()
        {
            new Platform() {Name = "Super Faketendo Example System", Custom = "SFES"},
            new Platform() {Name = "Faketendo 64", Custom = "F64"},
            // By leaving 'Custom' value empty here, Windows games won't include the platform name "(Windows)"
            new Platform() {Name = "Windows", Custom = ""},
        };

        #endregion

        //#region Load/Deserialize File

        ///// <summary>
        ///// Tries to load a Config from file. If that fails, a Config with default values is loaded instead.
        ///// </summary>
        ///// <returns>A Config instance with values either loaded from file or default values.</returns>
        //private static Config LoadConfigOrDefaults()
        //{
        //    try
        //    {
        //        // Try to load the plugin's configuration file
        //        return Config.LoadConfigFromFile();
        //    }
        //    catch (Exception ex)
        //    {
        //        switch (ex)
        //        {
        //            case FileNotFoundException _:
        //                // No config file found so load default configuration
        //                Logger.Warning($"The config file was not found. Loading defaults.");
        //                break;
        //            case XmlException _:
        //            case InvalidOperationException _:
        //                Logger.Error($"The config file contains invalid data. Renaming/backing up invalid config " +
        //                             $"and loading defaults.");
        //                RenameInvalidConfig();
        //                break;
        //            default:
        //                Logger.Error($"An unknown error occurred while loading the config: {ex.Message}");
        //                break;
        //        }
        //    }

        //    return GetDefaultConfig();
        //}

        ///// <summary>
        ///// Loads a configuration file and deserializes it.
        ///// </summary>
        ///// <returns>A Config instance with values loaded from a config file.</returns>
        //private static Config LoadConfigFromFile(string path = null)
        //{
        //    if (string.IsNullOrEmpty(path))
        //        path = Config.CONFIG_PATH;

        //    var serializer = new XmlSerializer(typeof(Config));

        //    using (var fStream = new FileStream(path, FileMode.Open))
        //        return (Config)serializer.Deserialize(fStream);
        //}

        //#endregion

        //#region Save/Serialize File

        ///// <summary>
        ///// Serializes Config class and saves it to a file.
        ///// </summary>
        //public void Save(string path = null)
        //{
        //    if (string.IsNullOrEmpty(path))
        //        path = Config.CONFIG_PATH;

        //    var ns = new XmlSerializerNamespaces();
        //    ns.Add("", "");

        //    var serializer = new XmlSerializer(typeof(Config));

        //    using (var fStream = new FileStream(path, FileMode.Create))
        //        serializer.Serialize(fStream, this, ns);

        //    Logger.Info("Config file saved.");
        //}

        //#endregion

        //#region Other Static Methods

        ///// <summary>
        ///// Used to rename/backup an existing config file in the event that it contains invalid data that cannot be 
        ///// parsed. By not deleting the bad config, a user can fix the issue and rename it back to its valid name.
        ///// </summary>
        //private static void RenameInvalidConfig()
        //{
        //    try
        //    {
        //        var configNameNoExt = Path.GetFileNameWithoutExtension(Config.CONFIG_PATH);
        //        var configExt = Path.GetExtension(Config.CONFIG_PATH);
        //        var configDirectory = Path.GetDirectoryName(Config.CONFIG_PATH) ?? "";
        //        string backupConfigPath = null;
        //        var count = 0;
        //        do
        //        {
        //            if (count > 99)
        //                throw new Exception("No unused filename could be found.");

        //            var backupConfigName = $"{configNameNoExt}.invalid-{count}{configExt}";
        //            backupConfigPath = Path.Combine(configDirectory, backupConfigName);
        //            count += 1;
        //        } while (File.Exists(backupConfigPath));

        //        File.Move(Config.CONFIG_PATH, backupConfigPath);
        //        Logger.Warning($"Successfully renamed invalid config to '{backupConfigPath}'.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error($"Failed to backup/rename invalid config file: {ex.Message}");
        //    }
        //}

        //#endregion
    }
}

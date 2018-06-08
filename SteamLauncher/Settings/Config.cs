using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using SteamLauncher.Logging;
using SteamLauncher.Tools;

namespace SteamLauncher.Settings
{
    [XmlRoot("SteamLauncherConfig")]
    public class Config
    {
        /// <summary>
        /// Name of the configuration file.
        /// </summary>
        [XmlIgnore]
        public const string CONFIG_FILENAME = "config.xml";

        /// <summary>
        /// The full path of the configuration file.
        /// </summary>
        [XmlIgnore]
        public static readonly string CONFIG_PATH = Path.Combine(Utilities.GetSteamLauncherDllPath(), CONFIG_FILENAME);

        /// <summary>
        /// Name of the debug log file.
        /// </summary>
        [XmlIgnore]
        public const string LOG_FILENAME = "debug.log";

        /// <summary>
        /// The full path of the debug log file.
        /// </summary>
        [XmlIgnore]
        public static readonly string LOG_PATH = Path.Combine(Utilities.GetSteamLauncherDllPath(), LOG_FILENAME);

        /// <summary>
        /// Defines whether the plugin should write debug messages to a log file for purposes of troubleshooting.
        /// </summary>
        public bool DebugLogEnabled { get; set; }

        /// <summary>
        /// Defines whether the plugin should prevent Steam from stealing focus from LaunchBox after a game exits.
        /// </summary>
        public bool PreventSteamFocusStealing { get; set; } = true;

        /// <summary>
        /// If <see cref="PreventSteamFocusStealing"/> is enabled, this value defines how many seconds Steam will 
        /// be prevented from taking focus after LB/BB is made the active Widnow. After a game exits, 
        /// LB/BB will regain focus for 3-4 seconds on average before Steam steals focus. The point of this value 
        /// is to only prevent Steam from gaining focus for a short time so intentional activation of the Steam 
        /// window won't be prevented.
        /// </summary>
        public int TotalSecondsToPreventSteamFocus { get; set; } = 6;

        /// <summary>
        /// User-defined list of custom plaform names to replace predefined platform names provided by LaunchBox.
        /// </summary>
        public List<Platform> CustomPlatformNames { get; set; }

        [XmlIgnore]
        public static Config Instance { get; private set; }

        public Config()
        {

        }

        /// <summary>
        /// Loads the default configuration if no config file exists or if there is an error loading it.
        /// </summary>
        public static void Default()
        {
            Logger.Info("Loading default configuration values.");
            Instance = new Config();
            Instance.SetDefaultValues();
        }

        /// <summary>
        /// Sets all properties to their default values.
        /// </summary>
        private void SetDefaultValues()
        {
            CustomPlatformNames = new List<Platform>
            {
                new Platform() { Name = "Super Faketendo Example System", Custom = "SFES" },
                new Platform() { Name = "Faketendo 64", Custom = "F64" }
            };
        }

        /// <summary>
        /// Loads configuration file and deserializes it.
        /// </summary>
        public static void Load()
        {
            var serializer = new XmlSerializer(typeof(Config));

            using (var fStream = new FileStream(Config.CONFIG_PATH, FileMode.Open))
                Config.Instance = (Config)serializer.Deserialize(fStream);
        }

        /// <summary>
        /// Serializes Config class and saves it to a file.
        /// </summary>
        public void Save()
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            var serializer = new XmlSerializer(typeof(Config));

            using (var fStream = new FileStream(Config.CONFIG_PATH, FileMode.Create))
                serializer.Serialize(fStream, this, ns);
        }
    }
}

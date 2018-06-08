using System.Xml.Serialization;

namespace SteamLauncher.Settings
{
    /// <summary>
    /// Class used by the SteamLauncher config to define each custom platform name.
    /// </summary>
    public class Platform
    {
        /// <summary>
        /// The platform's original name as defined by LaunchBox.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// The user-defined custom platform name to rename the platform to.
        /// </summary>
        [XmlAttribute]
        public string Custom { get; set; }

        public Platform()
        {

        }
    }
}

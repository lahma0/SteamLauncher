using System.Xml.Serialization;
using Unbroken.LaunchBox.Plugins;

namespace SteamLauncher.DataStore
{
    public enum IdType
    {
        Game,
        Emulator
    }

    /// <summary>
    /// When Universal Steam launching is enabled, this class is used to store backups of the 
    /// game/emulator paths that are replaced with the path to the SteamLauncherProxy 
    /// executable. Although these game/emulator paths are immediately restored after the 
    /// game is launched, they are stored in the config file just in case LB/BB crashes, so 
    /// they can be restored immediately on the next run. 
    /// </summary>
    public class RepairPath
    {
        /// <summary>
        /// The ID of the game/emulator that had its path modified.
        /// </summary>
        [XmlAttribute]
        public string Id { get; set; }

        /// <summary>
        /// Defines whether the Id value pertains to a LaunchBox game or emulator.
        /// </summary>
        [XmlAttribute]
        public IdType IdType { get; set; }

        /// <summary>
        /// The original executable path of the game/emulator that was modified.
        /// </summary>
        [XmlAttribute]
        public string Path { get; set; }

        public RepairPath()
        {

        }

        public dynamic GetEmuOrGameObj()
        {
            if (IdType == IdType.Game)
                return PluginHelper.DataManager.GetGameById(Id);

            return PluginHelper.DataManager.GetEmulatorById(Id);
        }
    }
}

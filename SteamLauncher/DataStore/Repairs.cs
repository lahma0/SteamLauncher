using SteamLauncher.Attributes;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SteamLauncher.DataStore
{
    /// <summary>
    /// Handles storage of repair-related data that may need to be saved in case of an application crash. The plugin can
    /// then use this data on the next application launch in order to perform repairs.
    /// </summary>
    [XmlFilename("repairs.xml")]
    [XmlRoot("SteamLauncherRepairs")]
    public class Repairs : XmlFile<Repairs>
    {
        #region Properties

        /// <summary>
        /// List of games/emulators that need their executable paths repaired as a result of being swapped with 
        /// the SteamLauncherProxy exe (only occurs when UniversalSteamLaunching is enabled). User should NOT 
        /// modify these values.
        /// </summary>
        public List<RepairPath> RepairPaths { get; set; } = new List<RepairPath>();

        /// <summary>
        /// This flag is set whenever the plugin renames the original DOSBox exe and replaces it with the proxy.
        /// </summary>
        public bool DosBoxExeNeedsRepair { get; set; } = false;

        /// <summary>
        /// This flag is set whenever the plugin renames the original ScummVM exe and replaces it with the proxy.
        /// </summary>
        public bool ScummVmExeNeedsRepair { get; set; } = false;

        #endregion

        //#region Singleton Constructor/Destructor

        //[XmlIgnore]
        //private static readonly Lazy<Repairs> Lazy = new Lazy<Repairs>(LoadXmlOrDefaults());

        //[XmlIgnore]
        //public static Repairs Instance => Lazy.Value;

        //protected Repairs()
        //{

        //}

        //~Repairs()
        //{
        //    // After moving to .NET Core, this destructor is no longer called when the app exits
        //    // File is now saved in SLInit.ProcessExit
        //}

        //#endregion
        
    }
}

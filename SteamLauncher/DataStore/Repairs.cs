using SteamLauncher.Attributes;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SteamLauncher.DataStore
{
    /// <summary>
    /// Handles storage of repair-related data that may need to be saved in case of an application crash. This data
    /// can then be repaired on the next application launch.
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

        /// <summary>
        /// Since changes in Steam started causing issues with temporary shortcuts, normal shortcuts are now being
        /// used instead. To ensure that SteamLauncher never has more than one non-Steam shortcut in the library at
        /// any one time, this value is used to keep track of the last non-Steam shortcut that was generated so it can
        /// always be deleted before a new one is created.
        /// </summary>
        public UInt32 LastGeneratedShortcutAppId { get; set; } = 0;

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

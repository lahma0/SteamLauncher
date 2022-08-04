using SteamLauncher.Attributes;
using SteamLauncher.Logging;
using SteamLauncher.SteamClient;
using SteamLauncher.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml;
using System.Xml.Serialization;

namespace SteamLauncher.DataStore.VTablesStore
{
    /// <summary>
    /// Handles the storage, loading, updating, and processing of Steam vtables.
    /// </summary>
    [XmlUrl("https://gist.githubusercontent.com/lahma0/bf21b0aa9e6fb0c060c027e956ec98c3/raw/vtables.xml")]
    [XmlFilename("vtables.xml")]
    [XmlRoot("SteamLauncherVTables")]
    public class VTables : XmlFile<VTables>
    {

        private VTables()
        {

        }

        /// <summary>
        /// A dictionary of all loaded vtables using the vtable's interface name as the key.
        /// </summary>
        [XmlIgnore]
        public List<VTable> List { get; private set; } = DefaultList.ToList();

        /// <summary>
        /// Used as a duplicate of <see cref="List"/> for XML serialization purposes. Specifically, the XML serializer
        /// always appends items to an existing list instead of just replacing the list (as one would expect). By using
        /// a dummy array property, this causes the desired behavior.
        /// </summary>
        [XmlArray(ElementName = "List")]
        public VTable[] DummyList
        {
            get => List.ToArray();
            set
            {
                if (value != null && value.Length > 0)
                    List = value.ToList();
            }
        }

        /// <summary>
        /// Provides the <see cref="OnlineDbLastUpdatedTimestamp"/> property converted to a local DateTime object.
        /// </summary>
        [XmlIgnore]
        public DateTime? OnlineDbLastUpdatedLocalDateTime
        {
            get
            {
                if (OnlineDbLastUpdatedTimestamp == 0)
                    return null;

                return DateTimeHelper.ConvertUnixTimeSecondsToLocalDateTime(OnlineDbLastUpdatedTimestamp);
            }
        }

        /// <summary>
        /// Stores the Unix timestamp (in seconds) of the last modification of the online DB.
        /// </summary>
        public long OnlineDbLastUpdatedTimestamp { get; set; } = 0;

        public override void OnLoaded()
        {
            OnlineUpdate();
        }

        /// <summary>
        /// Retrieves updated values from online db if updates are available.
        /// </summary>
        public override void OnlineUpdate(bool ignoreAutoUpdateSetting = false)
        {
            if (!ignoreAutoUpdateSetting && !Settings.Config.AutoUpdateVTables)
            {
                Logger.Info($"Aborted online update for '{GetType().Name}' because '{nameof(Settings.Config.AutoUpdateVTables)}' is disabled.");
                return;
            }

            //// Ensures update will not run again if it was already run in the last minute. Since 'OnlineUpdate()'
            //// is called from within 'OnDeserialized()' (which is run whenever the data from the online db is returned),
            //// an endless nested loop of online update checks would occur if this check wasn't implemented.
            //if ((DateTime.Now - Settings.Config.LastCheckForUpdatedVTables) < TimeSpan.FromMinutes(1))
            //    return;

            //Settings.Config.LastCheckForUpdatedVTables = DateTime.Now;

            Logger.Info($"Attempting to update '{GetType().Name}' via online db...");
            VTables onlineVTables;
            try
            {
                onlineVTables = LoadFromUrl(disableOnLoaded: true);
                if (onlineVTables == null)
                    throw new Exception($"The online updater returned a null value for '{GetType().Name}'.");
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case HttpRequestException _:
                        Logger.Info($"Could not retrieve '{GetType().Name}' data from url '{XmlUrl}' (check " +
                                    $"internet or firewall settings): {ex.Message}.");
                        break;
                    case XmlException _:
                    case InvalidOperationException _:
                        Logger.Warning($"The online '{GetType().Name}' data file contains invalid data.");
                        break;
                    default:
                        Logger.Error($"An unknown error occurred while loading the online '{GetType().Name}' data " +
                                     $"file: {ex.Message}");
                        break;
                }

                Logger.Warning($"Failed to auto update '{GetType().Name}' via online db.");
                return;
            }

            if (onlineVTables.OnlineDbLastUpdatedTimestamp > OnlineDbLastUpdatedTimestamp)
            {
                OnlineDbLastUpdatedTimestamp = onlineVTables.OnlineDbLastUpdatedTimestamp;
                List = onlineVTables.List;
                Logger.Info($"'{GetType().Name}' was updated with new data retrieved from the online database.");
                SteamContext.Instance.ResetInit();
            }
        }

        /// <summary>
        /// Gets a <see cref="VTable"/> instance with the specified <param name="interfaceName"/> from <see cref="List"/>.
        /// </summary>
        /// <param name="interfaceName">The interface name of the <see cref="VTable"/> entry to be retrieved.</param>
        /// <returns>An instance of <see cref="VTable"/> or null if no matching entry is found.</returns>
        public VTable GetVTable(string interfaceName)
        {
            return List.FirstOrDefault(v => v.InterfaceName.EqualsIgnoreCase(interfaceName));
        }

        /// <summary>
        /// Gets a 'VtEntry' instance with a matching name from the vtable with a matching interface name.
        /// </summary>
        /// <param name="interfaceName">Name of VTable.</param>
        /// <param name="entryName">Name of VtEntry.</param>
        /// <returns>A VtEntry instance or null if no matching entry is found.</returns>
        public VtEntry GetVtEntry(string interfaceName, string entryName)
        {
            return List.FirstOrDefault(x => x.InterfaceName.EqualsIgnoreCase(interfaceName))?.
                VtEntries.FirstOrDefault(y => y.Name.EqualsIgnoreCase(entryName));
        }

        /// <summary>
        /// The default <see cref="VTables"/> list used to create 'vtables.xml'. This list will be replaced and no
        /// longer used after the online updater retrieves the latest DB update.
        /// </summary>
        private static VTable[] DefaultList => new VTable[]
        {
            new VTable("IClientEngine",
                       "CLIENTENGINE_INTERFACE_VERSION005",
                       new VtEntry[]
                       {
                           new VtEntry("GetIClientShortcuts",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(int), "user"),
                                           new VtEntryParam(typeof(int), "pipe"),
                                           new VtEntryParam(typeof(string), "version", true),
                                       },
                                       typeof(IntPtr),
                                       58,
                                       58)
                       }),
            new VTable("IClientShortcuts",
                       "CLIENTSHORTCUTS_INTERFACE_VERSION001",
                       new VtEntry[]
                       {
                           // Steam update in August 2022 removed nearly half of the entries from IClientShortcuts
                           // and changed nearly all remaining offsets
                           new VtEntry("GetUniqueLocalAppId",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                       },
                                       typeof(UInt32),
                                       0,
                                       0),
                           new VtEntry("GetGameIDForAppID",
                                       new List<VtEntryParam>()
                                       { 
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt64), "retValue", isByRef: true),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                       },
                                       typeof(void),
                                       1,
                                       1),
                           new VtEntry("GetAppIDForGameID",
                                       new List<VtEntryParam>()
                                       { 
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt64), "unGameId"),
                                       },
                                       typeof(void),
                                       2,
                                       2),
                           new VtEntry("AddShortcut",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(string), "szAppName", true),
                                           new VtEntryParam(typeof(string), "szExePath", true),
                                           new VtEntryParam(typeof(string), "szIconPath", true),
                                           new VtEntryParam(typeof(string), "szProbStartDir", true),
                                           new VtEntryParam(typeof(string), "szCommandLine", true),
                                       },
                                       typeof(UInt32),
                                       7,
                                       7),
                           new VtEntry("AddTemporaryShortcut",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(string), "szAppName", true),
                                           new VtEntryParam(typeof(string), "szExePath", true),
                                           new VtEntryParam(typeof(string), "szIconPath", true),
                                       },
                                       typeof(UInt32),
                                       8,
                                       8),
                           new VtEntry("AddOpenVrShortcut",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(string), "szAppName", true),
                                           new VtEntryParam(typeof(string), "szExePath", true),
                                           new VtEntryParam(typeof(string), "szIconPath", true),
                                       },
                                       typeof(UInt32),
                                       9,
                                       9),
                           new VtEntry("SetShortcutFromFullPath",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                           new VtEntryParam(typeof(string), "szPath", true),
                                       },
                                       typeof(void),
                                       10,
                                       10),
                           new VtEntry("SetShortcutAppName",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                           new VtEntryParam(typeof(string), "szAppName", true),
                                       },
                                       typeof(void),
                                       11,
                                       11),
                           new VtEntry("SetShortcutExe",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                           new VtEntryParam(typeof(string), "szPath", true),
                                       },
                                       typeof(void),
                                       12,
                                       12),
                           new VtEntry("SetShortcutStartDir",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                           new VtEntryParam(typeof(string), "szPath", true),
                                       },
                                       typeof(void),
                                       13,
                                       13),
                           new VtEntry("SetShortcutIcon",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                           new VtEntryParam(typeof(string), "szIconPath", true),
                                       },
                                       typeof(void),
                                       14,
                                       14),
                           new VtEntry("SetShortcutCommandLine",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                           new VtEntryParam(typeof(string), "szCommandLine", true),
                                       },
                                       typeof(void),
                                       15,
                                       15),
                           new VtEntry("ClearShortcutUserTags",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),

                                       },
                                       typeof(void),
                                       16,
                                       16),
                           new VtEntry("AddShortcutUserTag",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                           new VtEntryParam(typeof(string), "szTag", true),
                                       },
                                       typeof(void),
                                       17,
                                       17),
                           new VtEntry("RemoveShortcutUserTag",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                           new VtEntryParam(typeof(string), "szTag", true),
                                       },
                                       typeof(void),
                                       18,
                                       18),
                           new VtEntry("SetShortcutHidden",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                           new VtEntryParam(typeof(bool), "arg1"),

                                       },
                                       typeof(void),
                                       20,
                                       20),
                           new VtEntry("SetAllowDesktopConfig",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                           new VtEntryParam(typeof(bool), "arg1"),

                                       },
                                       typeof(void),
                                       21,
                                       21),
                           new VtEntry("SetAllowOverlay",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                           new VtEntryParam(typeof(bool), "arg1"),

                                       },
                                       typeof(void),
                                       22,
                                       22),
                           new VtEntry("SetOpenVrShortcut",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                           new VtEntryParam(typeof(bool), "arg1"),

                                       },
                                       typeof(void),
                                       23,
                                       23),
                           new VtEntry("SetDevkitShortcut",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),
                                           new VtEntryParam(typeof(bool), "arg1"),

                                       },
                                       typeof(void),
                                       24,
                                       24),
                           
                           // Steam update on 2022-05-13 added new vtable entry at this index and 1 other (see
                           // comment following offset 23).

                           new VtEntry("RemoveShortcut",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),

                                       },
                                       typeof(void),
                                       26,
                                       26),
                           new VtEntry("RemoveAllTemporaryShortcuts",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),

                                       },
                                       typeof(void),
                                       27,
                                       27),
                           new VtEntry("LaunchShortcut",
                                       new List<VtEntryParam>()
                                       {
                                           new VtEntryParam(typeof(IntPtr), "thisPtr"),
                                           new VtEntryParam(typeof(UInt32), "unAppId"),

                                       },
                                       typeof(void),
                                       28,
                                       28),
                       })
        };
    }
}

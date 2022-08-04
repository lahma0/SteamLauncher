using SteamLauncher.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using SteamLauncher.Logging;
using SteamLauncher.Tools;
using System.Net.Http;
using System.Xml;

namespace SteamLauncher.DataStore.VTablesStore
{
    //[XmlFilename("vtables.xml")]
    //[XmlRoot("SteamLauncherVTables")]
    //public class VtFile : XmlFile<VtFile>
    //{
    //    #region Fields

    //    /// <summary>
    //    /// Name of the xml data file.
    //    /// </summary>
    //    [XmlIgnore]
    //    public static readonly string XML_FILENAME = "vtables.xml";

    //    /// <summary>
    //    /// The full path of the xml data file.
    //    /// </summary>
    //    [XmlIgnore]
    //    public static readonly string XML_PATH = Path.Combine(Info.SteamLauncherDir, XML_FILENAME);

    //    /// <summary>
    //    /// The online address where the automatic updater can find the updated database.
    //    /// </summary>
    //    [XmlIgnore]
    //    public static readonly string ONLINE_DB_URL = "https://gist.githubusercontent.com/lahma0/62a49d0813e10f45e42df41cfa262b68/raw/vtables.xml";

    //    #endregion

    //    #region Properties

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public override string SavePath => XML_PATH;

    //    private bool _autoUpdateOnline = true;
    //    /// <summary>
    //    /// If this setting is enabled, the plugin automatically updates any applicable vtable info using an online
    //    /// database maintained by the plugin author.
    //    /// </summary>
    //    public bool AutoUpdateOnline
    //    {
    //        get => _autoUpdateOnline;
    //        set
    //        {
    //            var doUpdate = _autoUpdateOnline == false && value;
    //            _autoUpdateOnline = value;
    //            if (doUpdate)
    //                UpdateViaOnlineDb();
    //        }
    //    }

    //    /// <summary>
    //    /// Provides the <see cref="OnlineDbLastModified"/> property converted to a local DateTime object.
    //    /// </summary>
    //    [XmlIgnore]
    //    public DateTime? OnlineDbLastModifiedLocalDateTime
    //    {
    //        get
    //        {
    //            if (string.IsNullOrWhiteSpace(OnlineDbLastModified))
    //                return null;

    //            var success = long.TryParse(OnlineDbLastModified, out var result);
    //            if (!success || result == 0)
    //                return null;

    //            return DateTimeHelper.ConvertUnixTimeSecondsToLocalDateTime(OnlineDbLastModified);
    //        }
    //    }

    //    /// <summary>
    //    /// Stores the Unix timestamp (in seconds) of the last modification of the online DB.
    //    /// </summary>
    //    public string OnlineDbLastModified { get; set; } = "";

    //    /// <summary>
    //    /// Stores the Unix timestamp (in seconds) of the last time the online DB was successfully checked for updates.
    //    /// </summary>
    //    public string LastCheckedForUpdates { get; set; } = "";

    //    public IEnumerable<VTable> VTables { get; set; } = new List<VTable>();

    //    /// <summary>
    //    /// IClientEngine vtable index for 'GetIClientShortcuts'. Previously this value was found automatically using
    //    /// some relatively simple logic but this method was recently broken due to some significant changes in the
    //    /// structure/layout of steamclient.dll. Until a new method is devised, a static value will instead be used.
    //    /// </summary>
    //    public int GetIClientShortcutsIndex { get; set; } = 58;

    //    /// <summary>
    //    /// Sometimes the beta version of the Steam client can have a different vtable index for
    //    /// 'GetIClientShortcuts'. This value is only used whenever the user is running the beta version of the Steam
    //    /// client.
    //    /// </summary>
    //    public int BetaGetIClientShortcutsIndex { get; set; } = 58;

    //    #endregion

    //    #region Singleton Constructor/Destructor

    //    [XmlIgnore]
    //    private static readonly Lazy<VtFile> Lazy = new Lazy<VtFile>(LoadXmlOrDefaults(XML_PATH));

    //    [XmlIgnore]
    //    public static VtFile Instance => Lazy.Value;

    //    protected VtFile()
    //    {

    //    }

    //    public override void RunAfterDeserialize()
    //    {
    //        UpdateViaOnlineDb();
    //        Save();
    //    }

    //    #endregion

    //    #region OnlineUpdater

    //    /// <summary>
    //    /// Updates vtable info from online db.
    //    /// </summary>
    //    private void UpdateViaOnlineDb()
    //    {
    //        if (!AutoUpdateOnline)
    //        {
    //            Logger.Info("Aborted vtable info online update because auto updates are disabled.");
    //            return;
    //        }

    //        Logger.Info("Attempting to update vtable info from online db...");
    //        VtFile newData;
    //        try
    //        {
    //            newData = LoadFromUrl(ONLINE_DB_URL);
    //        }
    //        catch (Exception ex)
    //        {
    //            switch (ex)
    //            {
    //                case HttpRequestException _:
    //                    Logger.Info($"Could not retrieve '{GetType().Name}' data from url '{ONLINE_DB_URL}' (check " +
    //                                $"internet or firewall settings): {ex.Message}.");
    //                    break;
    //                case XmlException _:
    //                case InvalidOperationException _:
    //                    Logger.Warning($"The online '{GetType().Name}' data file contains invalid data.");
    //                    break;
    //                default:
    //                    Logger.Error($"An unknown error occurred while loading the online '{GetType().Name}' data " +
    //                                 $"file: {ex.Message}");
    //                    break;
    //            }

    //            Logger.Warning($"Failed to auto update vtable info from online db.");
    //            return;
    //        }

    //        LastCheckedForUpdates = DateTimeHelper.GetCurrentUnixTimeSeconds().ToString();

    //        if (GetIClientShortcutsIndex != newData.GetIClientShortcutsIndex ||
    //            BetaGetIClientShortcutsIndex != newData.BetaGetIClientShortcutsIndex)
    //        {
    //            Logger.Info($"Online DB update is available! Updating now... (Online DB Timestamp: " + 
    //            $"{OnlineDbLastModifiedLocalDateTime})");
    //        }
    //        OnlineDbLastModified = newData.OnlineDbLastModified;
    //        GetIClientShortcutsIndex = newData.GetIClientShortcutsIndex;
    //        BetaGetIClientShortcutsIndex = newData.BetaGetIClientShortcutsIndex;
    //    }

    //    #endregion

    //    #region Defaults

    //    /// <summary>
    //    /// Gets a VTablesInfo instance containing default values for all properties.
    //    /// </summary>
    //    /// <returns>A VTablesInfo instance containing default values.</returns>
    //    public static VtFile GetDefaultInstance()
    //    {
    //        var instance = new VtFile();
    //        return instance;
    //    }

    //    public static IEnumerable<VTable> GetDefaultVTables()
    //    {
    //        return new List<VTable>()
    //        {
    //            //new VTable()
    //            //{
    //            //    InterfaceName = "IClientEngine",
    //            //    InterfaceVersion = "CLIENTENGINE_INTERFACE_VERSION005",
    //            //    VtEntries = new List<VtEntry>()
    //            //    {
    //            //        new VtEntry("GetIClientShortcuts", 58, 57)
    //            //    }
    //            //},
    //            //new VTable()
    //            //{
    //            //    InterfaceName = "IClientShortcuts",
    //            //    InterfaceVersion = "CLIENTSHORTCUTS_INTERFACE_VERSION001",
    //            //    VtEntries = new List<VtEntry>()
    //            //    {
    //            //        new VtEntry("GetUniqueLocalAppId", 0),
    //            //        new VtEntry("GetGameIdForAppId", 1),
    //            //        new VtEntry("GetAppIdForGameId", 2),
    //            //        new VtEntry("GetShortcutCount", 3),
    //            //        new VtEntry("GetShortcutAppIdByIndex", 4),
    //            //        new VtEntry("GetShortcutAppNameByIndex", 5),
    //            //        new VtEntry("GetShortcutExeByIndex", 6),
    //            //        new VtEntry("GetShortcutUserTagCountByIndex", 7),
    //            //        new VtEntry("GetShortcutUserTagByIndex", 8),
    //            //        new VtEntry("BIsShortcutRemoteByIndex", 9),
    //            //        new VtEntry("BIsTemporaryShortcutByIndex", 10),
    //            //        new VtEntry("BIsOpenVrShortcutByIndex", 11),
    //            //        new VtEntry("BIsDevkitShortcutByIndex", 12),
    //            //        new VtEntry("GetDevkitGameIdByIndex", 13),
    //            //        new VtEntry("GetDevkitAppIdByDevkitGameId", 14),
    //            //        new VtEntry("GetOverrideAppId", 15),
    //            //        new VtEntry("GetShortcutAppNameByAppId", 16),
    //            //        new VtEntry("GetShortcutExeByAppId", 17),
    //            //        new VtEntry("GetShortcutStartDirByAppId", 18),
    //            //        new VtEntry("GetShortcutIconByAppId", 19),
    //            //        new VtEntry("GetShortcutPathByAppId", 20),
    //            //        new VtEntry("GetShortcutCommandLine", 21),
    //            //        new VtEntry("GetShortcutUserTagCountByAppId", 22),
    //            //        new VtEntry("GetShortcutUserTagByAppId", 23),
    //            //        new VtEntry("BIsShortcutRemoteByAppId", 24),
    //            //        new VtEntry("BIsShortcutHiddenByAppId", 25),
    //            //        new VtEntry("BIsTemporaryShortcutByAppId", 26),
    //            //        new VtEntry("BIsOpenVrShortcutByAppId", 27),
    //            //        new VtEntry("BAllowDesktopConfigByAppId", 28),
    //            //        new VtEntry("BAllowOverlayByAppId", 29),
    //            //        new VtEntry("GetShortcutLastPlayedTime", 30),
    //            //        new VtEntry("AddShortcut", 31),
    //            //        new VtEntry("AddTemporaryShortcut", 32),
    //            //        new VtEntry("AddOpenVrShortcut", 33),
    //            //        new VtEntry("SetShortcutFromFullPath", 34),
    //            //        new VtEntry("SetShortcutAppName", 35),
    //            //        new VtEntry("SetShortcutExe", 36),
    //            //        new VtEntry("SetShortcutStartDir", 37),
    //            //        new VtEntry("SetShortcutIcon", 38),
    //            //        new VtEntry("SetShortcutCommandLine", 39),
    //            //        new VtEntry("ClearShortcutUserTags", 40),
    //            //        new VtEntry("AddShortcutUserTag", 41),
    //            //        new VtEntry("RemoveShortcutUserTag", 42),
    //            //        new VtEntry("ClearAndSetShortcutUserTags", 43),
    //            //        new VtEntry("SetShortcutHidden", 44),
    //            //        new VtEntry("SetAllowDesktopConfig", 45),
    //            //        new VtEntry("SetAllowOverlay", 46),
    //            //        new VtEntry("SetOpenVrShortcut", 47),
    //            //        new VtEntry("SetDevkitShortcut", 48),
    //            //        new VtEntry("RemoveShortcut", 49),
    //            //        new VtEntry("RemoveAllTemporaryShortcuts", 50),
    //            //        new VtEntry("LaunchShortcut", 51)
    //            //    }
    //            //}
    //        };
    //    }

    //    #endregion
    //}
}

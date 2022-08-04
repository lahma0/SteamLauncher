using SteamLauncher.DataStore.VTablesStore;

namespace SteamLauncher.DataStore
{
    /// <summary>
    /// Static class that retains instances of all data files needed for operation of the app.
    /// </summary>
    public static class Settings
    {
        static Settings()
        {
            Config = Config.LoadXmlOrDefaults();
            Repairs = Repairs.LoadXmlOrDefaults();
            VTables = VTables.LoadXmlOrDefaults();
        }

        /// <summary>
        /// User preferences and other configuration data.
        /// </summary>
        public static Config Config { get; } = null;

        /// <summary>
        /// Repair-related data saved and used in case of an application crash.
        /// </summary>
        public static Repairs Repairs { get; } = null;

        /// <summary>
        /// Steam vtable related data.
        /// </summary>
        public static VTables VTables { get; } = null;
    }
}

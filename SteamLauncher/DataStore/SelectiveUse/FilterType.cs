using SteamLauncher.Tools;
using System.ComponentModel;

namespace SteamLauncher.DataStore.SelectiveUse
{
    /// <summary>
    /// Defines the type of filter being used in the 'Selective Use' feature which allows users to enable/disable the
    /// plugin based on a list of user-supplied filters.
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// Filter by the game/rom's title.
        /// </summary>
        [Description("Game Title")]
        GameTitle = 0,

        /// <summary>
        /// Filter by the platform name (as defined in LaunchBox, not the user-defined custom platform names assigned in
        /// SteamLauncher's settings).
        /// </summary>
        [Description("Platform Name")]
        PlatformName,

        /// <summary>
        /// Filter by the emulator's name.
        /// </summary>
        [Description("Emulator Title")]
        EmulatorTitle,

        /// <summary>
        /// Filter by the launch executable's path.
        /// </summary>
        [Description("Exe Path")]
        ExePath,

        /// <summary>
        /// Filter by the additional application's name.
        /// </summary>
        [Description("Additional Application Name")]
        AdditionalApplicationName,

        [Description("Status")]
        Status,
    }
}

using SteamLauncher.Shortcuts;
using SteamLauncher.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Unbroken.LaunchBox.Plugins.Data;

namespace SteamLauncher.DataStore.SelectiveUse
{
    /// <summary>
    /// Allows the user to define specific conditions under which the plugin is enabled/disabled.
    /// </summary>
    public class Filtering
    {
        /// <summary>
        /// This is what a LaunchBox game's Custom Field's key must be named in order for it to be recognized by
        /// SteamLauncher. If SteamLauncher finds a game with a Custom Field key of this name, it will read that key's
        /// corresponding value and use it as a universal override of any other filtering behavior (with the only
        /// exception being if <see cref="IgnoreCustomFields"/> is set to true). A value of '1' will force SteamLauncher
        /// to be enabled for that title while a value of '0' will force SteamLauncher to be disabled for that title.
        /// Any other value will be ignored.
        /// </summary>
        [XmlIgnore]
        private static readonly string CustomFieldsSLEnabledKeyName = "SLEnabled";

        /// <summary>
        /// Defines how this feature is globally applied. OFF: Disable this feature entirely. BLACKLIST: The plugin is
        /// always enabled unless a matching filter is found. WHITELIST: The plugin is always disabled unless a matching
        /// filter is found. Note: The 'SLEnabled' value in any game's 'Custom Fields' entry supersedes the value of
        /// <see cref="FilterMode"/> (including 'OFF'), unless <see cref="IgnoreCustomFields"/> is enabled.
        /// </summary>
        [XmlAttribute]
        public FilterMode FilterMode { get; set; } = FilterMode.Off;

        /// <summary>
        /// List of user-defined filters applied in BLACKLIST or WHITELIST mode.
        /// </summary>
        public List<Filter> Filters { get; set; } = new List<Filter>();

        /// <summary>
        /// If enabled, this will ignore the value of 'SLEnabled' entries in any LaunchBox game's 'Custom Fields'
        /// entries.
        /// </summary>
        [XmlAttribute]
        public bool IgnoreCustomFields { get; set; }

        public Filtering()
        {
            
        }

        public Filtering(FilterMode filterMode, List<Filter> filters)
        {
            FilterMode = filterMode;
            Filters = filters;
        }

        public bool IsSLEnabledForGameLaunch(IGame game, IAdditionalApplication app, IEmulator emulator, out string reason)
        {
            reason = null;
            var matchReason = "";
            if (!IgnoreCustomFields)
            {
                var slEnabled = game.GetAllCustomFields().FirstOrDefault(x => x.Name == CustomFieldsSLEnabledKeyName);
                if (slEnabled != null && !string.IsNullOrWhiteSpace(slEnabled.Value) && (slEnabled.Value == "0" || slEnabled.Value == "1"))
                {
                    reason = $"'{CustomFieldsSLEnabledKeyName}' Custom Field value is set to '{slEnabled.Value}' " + 
                             $"on game '{game.Title} ({game.Platform})'.";

                    if (slEnabled.Value == "1")
                    {
                        return true;
                    }

                    if (slEnabled.Value == "0")
                        return false;
                }
            }

            var match = Filters.FirstOrDefault(filter => filter.IsMatch(game, app, emulator, out matchReason));

            switch (FilterMode)
            {
                case FilterMode.Whitelist:
                    if (match == null)
                    {
                        reason = $"'{nameof(FilterMode)}' is set to '{nameof(FilterMode.Whitelist)}' and no " +
                                 $"matching filter was found";
                        return false;
                    }

                    reason = $"'{nameof(FilterMode.Whitelist)}' Match: {matchReason}";
                    return true;

                case FilterMode.Blacklist:
                    if (match == null)
                    {
                        reason = $"'{nameof(FilterMode)}' is set to '{nameof(FilterMode.Blacklist)}' and no " +
                                 $"matching filter was found.";
                        return true;
                    }

                    reason = $"'{nameof(FilterMode.Blacklist)}' Match: {matchReason}";
                    return false;

                case FilterMode.Off:
                default:
                    reason = $"'{nameof(FilterMode)}' is set to '{nameof(FilterMode.Off)}'.";
                    return true;
            }
        }
    }
}

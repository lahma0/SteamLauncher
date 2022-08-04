using SteamLauncher.Tools;
using System.Xml.Serialization;
using Unbroken.LaunchBox.Plugins.Data;

namespace SteamLauncher.DataStore.SelectiveUse
{
    /// <summary>
    /// User defined entries defining specific parameters that dictate when the plugin should or should not be enabled
    /// (and therefore intercept and reroute game launches through Steam).
    /// </summary>
    public class Filter
    {
        public Filter()
        {

        }

        public Filter(string description, FilterType filterType, string filterString, bool enable = true)
        {
            Description = description;
            FilterType = filterType;
            FilterString = filterString;
            Enable = enable;
        }

        /// <summary>
        /// Describes/labels the filter for quick identification and is entirely optional.
        /// </summary>
        [XmlAttribute]
        public string Description { get; set; } = "";

        /// <summary>
        /// Enables/disables the filter.
        /// </summary>
        [XmlAttribute]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// The user-supplied wildcard-capable filter string.
        /// </summary>
        [XmlAttribute]
        public string FilterString { get; set; }

        [XmlAttribute]
        public FilterType FilterType { get; set; }

        ///// <summary>
        ///// If enabled, inverses the result of the filter. For example, if 'Inverse' is set, the filter for 'GameTitle'
        ///// is set to '*Mario*', and the game 'Super Mario World' is launched, the result will be NON-match.
        ///// </summary>
        //[XmlAttribute]
        //public bool Inverse { get; set; }

        ///// <summary>
        ///// Enables/disables regex support for all filter strings in this instance.
        ///// </summary>
        //[XmlAttribute]
        //public bool UseRegex { get; set; }

        public bool IsMatch(IGame game, IAdditionalApplication app, IEmulator emulator, out string reason)
        {
            reason = null;

            if (!Enable)
                return false;

            if (FilterType == FilterType.AdditionalApplicationName &&
                app != null &&
                app.Name.EqualsWildcard(FilterString, true))
            {
                reason = $"The filter string '{FilterString}' matched " + 
                              $"'{FilterType.ToString().SplitCamelCase()}' '{app.Name}'.";
                return true;
            }

            if (FilterType == FilterType.EmulatorTitle &&
                emulator != null &&
                emulator.Title.EqualsWildcard(FilterString, true))
            {
                reason = $"The filter string '{FilterString}' matched " + 
                              $"'{FilterType.ToString().SplitCamelCase()}' '{emulator.Title}'.";
                return true;
            }

            if (FilterType == FilterType.ExePath)
            {
                if (app != null && app.ApplicationPath.EqualsWildcard(FilterString, true))
                {
                    reason = $"The filter string '{FilterString}' matched " +
                                  $"'{FilterType.ToString().SplitCamelCase()}' '{app.ApplicationPath}'.";
                    return true;
                } 
                
                if (emulator != null && emulator.ApplicationPath.EqualsWildcard(FilterString, true))
                {
                    reason = $"The filter string '{FilterString}' matched " +
                                  $"'{FilterType.ToString().SplitCamelCase()}' '{emulator.ApplicationPath}'.";
                    return true;
                }
                
                if (game != null && game.ApplicationPath.EqualsWildcard(FilterString, true))
                {
                    reason = $"The filter string '{FilterString}' matched " +
                                  $"'{FilterType.ToString().SplitCamelCase()}' '{game.ApplicationPath}'.";
                    return true;
                }
            }

            if (FilterType == FilterType.GameTitle && 
                game != null && 
                game.Title.EqualsWildcard(FilterString, true))
            {
                reason = $"The filter string '{FilterString}' matched " +
                              $"'{FilterType.ToString().SplitCamelCase()}' '{game.Title}'.";
                return true;
            }

            if (FilterType == FilterType.PlatformName && 
                game != null && 
                game.Platform.EqualsWildcard(FilterString, true))
            {
                reason = $"The filter string '{FilterString}' matched " +
                              $"'{FilterType.ToString().SplitCamelCase()}' '{game.Platform}'.";
                return true;
            }

            if (FilterType == FilterType.Status && 
                game != null && 
                game.Status.EqualsWildcard(FilterString, true))
            {
                reason = $"The filter string '{FilterString}' matched " +
                         $"'{FilterType.ToString().SplitCamelCase()}' '{game.Status}'.";
                return true;
            }

            return false;
        } 
    }
}

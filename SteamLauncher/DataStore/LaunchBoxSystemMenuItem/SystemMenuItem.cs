using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SteamLauncher.DataStore.LaunchBoxSystemMenuItem
{
    public class SystemMenuItem
    {
        public SystemMenuItem()
        {

        }

        /// <summary>
        /// Determines if the menu item is shown in LaunchBox.
        /// </summary>
        [XmlAttribute]
        public bool ShowInLaunchBox { get; set; } = true;

        /// <summary>
        /// Determines if the menu item is shown in BigBox.
        /// </summary>
        [XmlAttribute]
        public bool ShowInBigBox { get; set; } = true;

        /// <summary>
        /// Determines if the menu item is shown in BigBox while the interface is locked.
        /// </summary>
        [XmlAttribute]
        public bool AllowInBigBoxWhenLocked { get; set; } = true;
    }
}

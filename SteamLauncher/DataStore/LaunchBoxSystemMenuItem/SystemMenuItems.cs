using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SteamLauncher.DataStore.LaunchBoxSystemMenuItem
{
    public class SystemMenuItems
    {
        public SystemMenuItems()
        {

        }
        
        public SystemMenuItem EnableSteamLauncher => new SystemMenuItem();

        public SystemMenuItem Settings => new SystemMenuItem();
    }
}

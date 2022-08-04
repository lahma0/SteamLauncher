using System;

namespace SteamLauncher.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class XmlUrlAttribute : Attribute
    {
        public XmlUrlAttribute(string url)
        {
            Url = url;
        }

        public string Url { get; set; }
    }
}

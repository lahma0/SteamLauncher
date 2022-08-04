using System;

namespace SteamLauncher.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class XmlFilenameAttribute : Attribute
    {
        public XmlFilenameAttribute(string filename)
        {
            Filename = filename;
        }

        public string Filename { get; set; }
    }
}

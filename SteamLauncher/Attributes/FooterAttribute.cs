using System;

namespace SteamLauncher.Attributes
{
    /// <summary>
    /// This attribute is used to display supplemental information in the footer of a setting's description in the
    /// Settings UI (displayed when the mouse hovers over the different settings).
    /// </summary>
    public class FooterAttribute : Attribute
    {
        public static readonly FooterAttribute Default = new FooterAttribute();

        public FooterAttribute() : this(string.Empty)
        {
        }

        public FooterAttribute(string footer)
        {
            FooterValue = footer;
        }

        public virtual string Footer => FooterValue;

        protected string FooterValue { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            return obj is FooterAttribute other && other.Footer == Footer;
        }

        public override int GetHashCode() => Footer?.GetHashCode() ?? 0;

        public override bool IsDefaultAttribute() => Equals(Default);
    }
}

using SteamLauncher.Tools;

namespace SteamLauncher.DataStore.SelectiveUse
{
    public enum FilterMode
    {
        Off,
        Blacklist,
        Whitelist,
    }

    //public class FilterMode : Enumeration
    //{
    //    public static readonly FilterMode OFF = new FilterMode(0, nameof(OFF));

    //    public static readonly FilterMode BLACKLIST = new FilterMode(1, nameof(BLACKLIST));

    //    public static readonly FilterMode WHITELIST = new FilterMode(2, nameof(WHITELIST));

    //    private FilterMode(int value, string displayName) : base(value, displayName) { }

    //    private FilterMode() : base(0, nameof(OFF))
    //    {

    //    }
    //}
}

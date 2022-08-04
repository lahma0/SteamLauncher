using System.IO;
using SteamLauncher.Tools;

namespace SteamLauncher.SteamClient
{
    //public class ShortcutsVdf
    //{
    //    /// <summary>
    //    /// The sequence of bytes used to terminate each shortcut entry in the shortcuts.vdf file.
    //    /// </summary>
    //    private static byte[] TerminateEntryBytes => new byte[] { 0x08, 0x08 };

    //    private static byte[] GetBytes()
    //    {
    //        var shortcutsVdfPath = SteamProcessInfo.ShortcutsVdfPath;

    //        if (!File.Exists(shortcutsVdfPath))
    //        {
    //            throw new FileNotFoundException(shortcutsVdfPath);
    //        }

    //        return File.ReadAllBytes(shortcutsVdfPath);
    //    }

    //    public static int GetShortcutsCount()
    //    {
    //        byte[] vdfBytes;

    //        try
    //        {
    //            vdfBytes = GetBytes();
    //        }
    //        catch (FileNotFoundException)
    //        {
    //            // A fresh Steam install won't have a shortcuts.vdf file until a shortcut is added.
    //            // So, if it can't be found, we can assume we have 0 shortcuts.
    //            return 0;
    //        }

    //        var count = vdfBytes.CountOccurrences(TerminateEntryBytes);
    //        if (count == 0)
    //        {
    //            // If shortcuts.vdf exists, there's at least 1 set of termination bytes, so we should never get here
    //            return 0;
    //        }

    //        // An empty shortcuts.vdf file has 1 set of termination bytes, so we need to subtract 1 from the total
    //        return count - 1;
    //    }
    //}
}

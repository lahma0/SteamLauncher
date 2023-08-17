using System.IO;
using SteamLauncher.Tools;
using System;

namespace SteamLauncher.SteamClient
{
    public class ShortcutsVdf
    {
        /// <summary>
        /// The sequence of bytes used to terminate each shortcut entry in the shortcuts.vdf file.
        /// </summary>
        private static byte[] TerminateEntryBytes => new byte[] { 0x08, 0x08 };

        private static byte[] GetBytes()
        {
            var shortcutsVdfPath = SteamProcessInfo.ShortcutsVdfPath;

            if (!File.Exists(shortcutsVdfPath))
            {
                throw new FileNotFoundException(shortcutsVdfPath);
            }

            return File.ReadAllBytes(shortcutsVdfPath);
        }

        public static int GetShortcutsCount()
        {
            byte[] vdfBytes;

            try
            {
                vdfBytes = GetBytes();
            }
            catch (FileNotFoundException)
            {
                // A fresh Steam install won't have a shortcuts.vdf file until a shortcut is added.
                // So, if it can't be found, we can assume we have 0 shortcuts.
                return 0;
            }

            var count = vdfBytes.CountOccurrences(TerminateEntryBytes);
            if (count == 0)
            {
                // If shortcuts.vdf exists, there's at least 1 set of termination bytes, so we should never get here
                return 0;
            }

            // An empty shortcuts.vdf file has 1 set of termination bytes, so we need to subtract 1 from the total
            return count - 1;
        }

        /// <summary>
        /// Find the last shortcut entry in the shortcuts.vdf file with a matching name and return its ShortcutID.
        /// </summary>
        /// <param name="shortcutName">The name of the non-Steam shortcut.</param>
        /// <returns>The ShortcutID of non-Steam shortcut matched within shortcuts.vdf. If not found, returns 0.</returns>
        public static UInt64 GetLastShortcutIdByName(string shortcutName)
        {
            var vdfBytes = GetBytes();
            var shortcutNameBytes = System.Text.Encoding.UTF8.GetBytes(shortcutName);
            
            // 41 70 70 4E 61 6D 65 = "AppName"
            var prefixBytes = new byte[] { 0x01, 0x41, 0x70, 0x70, 0x4E, 0x61, 0x6D, 0x65, 0x00 };
            var findBytes = new byte[prefixBytes.Length + shortcutNameBytes.Length];
            Buffer.BlockCopy(prefixBytes, 0, findBytes, 0, prefixBytes.Length);
            Buffer.BlockCopy(shortcutNameBytes, 0, findBytes, prefixBytes.Length, shortcutNameBytes.Length);
            var result = vdfBytes.LastIndexOf(findBytes);
            if (result == -1)
                return 0;
			
            var extractedBytes = new byte[4];
            Buffer.BlockCopy(vdfBytes, Convert.ToInt32(result - 4), extractedBytes, 0, 4);
            var appIdBytes = new byte[] { 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00 };
            Buffer.BlockCopy(extractedBytes, 0, appIdBytes, 4, 4);
            return BitConverter.ToUInt64(appIdBytes, 0);
        }
    }
}

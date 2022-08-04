using System;
using System.IO;

namespace SteamLauncher.Tools
{
    public class WindowsLnkFile
    {
        public string ShortcutPath { get; }
        
        public string TargetPath { get; private set; }

        public string Arguments { get; private set; }

        public string WorkingDirectory { get; private set; }

        public WindowsLnkFile(string shortcutPath)
        {
            if (shortcutPath == null)
                throw new ArgumentNullException(nameof(shortcutPath));

            if (!Path.GetExtension(shortcutPath).EqualsIgnoreCase(".lnk"))
                throw new ArgumentException("The provided path does not point to a valid Windows .lnk shortcut file.",
                                            nameof(shortcutPath));

            if (!System.IO.File.Exists(shortcutPath))
                throw new FileNotFoundException("The provided file path does not exist.", shortcutPath);

            ShortcutPath = shortcutPath;
            ResolveShortcut();
        }

        private void ResolveShortcut()
        {
            var path = ShortcutPath;
            ShellLnk lnkShortcut = null;
            var loopCount = 0;

            // This code should be unnecessary since Windows doesn't allow shortcuts pointing to shortcuts. However,
            // the .lnk format itself does allow it so an external utility could cause such a scenario.
            do
            {
                if (loopCount > 10)
                    throw new Exception("The provided shortcut links to an excessive number of other shortcut files.");

                lnkShortcut = new ShellLnk(path);
                if (string.IsNullOrWhiteSpace(lnkShortcut.TargetPath.Trim('"')))
                    throw new NullReferenceException(
                        "The shortcut file could not be resolved because the target path is null.");

                path = lnkShortcut.TargetPath.Trim('"');
                loopCount += 1;
            } while (Path.GetExtension(path).EqualsIgnoreCase(".lnk"));

            TargetPath = Environment.ExpandEnvironmentVariables(lnkShortcut.TargetPath).Trim('"');
            Arguments = Environment.ExpandEnvironmentVariables(lnkShortcut.Arguments);
            WorkingDirectory = Environment.ExpandEnvironmentVariables(lnkShortcut.WorkingDirectory).Trim('"');
        }

        #region Old code that used IWshRuntimeLibrary
        //private void ResolveShortcut()
        //{
        //    var shell = new WshShell();
        //    var path = ShortcutPath;
        //    IWshRuntimeLibrary.IWshShortcut lnkShortcut = null;
        //    var loopCount = 0;

        //    // This code should be entirely unnecessary since by default Windows does not allow shortcuts to point 
        //    // to other shortcuts. However, I believe that the .lnk file format itself does allow such a thing.
        //    do
        //    {
        //        if (loopCount > 10)
        //            throw new Exception("The provided shortcut links to an excessive number of other shortcut files.");

        //        lnkShortcut = (IWshRuntimeLibrary.IWshShortcut) shell.CreateShortcut(path);
        //        if (lnkShortcut == null)
        //            throw new NullReferenceException(
        //                "The shortcut file could not be resolved resulting in a null shortcut object.");

        //        path = lnkShortcut.TargetPath;
        //        loopCount += 1;
        //    } while (Path.GetExtension(path).EqualsIgnoreCase(".lnk"));

        //    TargetPath = lnkShortcut.TargetPath;
        //    Arguments = lnkShortcut.Arguments;
        //    WorkingDirectory = lnkShortcut.WorkingDirectory;
        //}
        #endregion
    }
}

using System.Windows;

namespace SteamLauncher.UI.Framework
{
    public interface IDialog
    {
        Window Owner { get; set; }

        string[] OpenFileDialog(object owner = null,
                                string filename = null,
                                string defaultExt = null,
                                string title = null,
                                string initialDirectory = null,
                                string filter = null,
                                int filterIndex = 0,
                                bool multiSelect = false,
                                bool checkFileExists = true,
                                bool checkPathExists = true,
                                bool dereferenceLinks = true,
                                bool addExtension = true,
                                bool validateNames = true);


    }
}

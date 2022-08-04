using System.Windows;
using Microsoft.Win32;

namespace SteamLauncher.UI.Framework
{
    public class DialogWindow : IDialog
    {
        public DialogWindow(Window owner)
        {
            Owner = owner;
        }

        public DialogWindow()
        {
            Owner = null;
        }


        public Window Owner { get; set; }

        public string[] OpenFileDialog(object owner = null,
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
                                       bool validateNames = true)
        {
            owner ??= Owner;

            var dialog = new OpenFileDialog()
            {
                FileName = filename ?? string.Empty,
                DefaultExt = defaultExt ?? string.Empty,
                Title = title ?? string.Empty,
                InitialDirectory = initialDirectory ?? string.Empty,
                Filter = filter ?? string.Empty,
                FilterIndex = filterIndex,
                Multiselect = multiSelect,
                CheckFileExists = checkFileExists,
                CheckPathExists = checkPathExists,
                DereferenceLinks = dereferenceLinks,
                AddExtension = addExtension,
                ValidateNames = validateNames,
            };

            return dialog.ShowDialog(owner as Window) == true ? dialog.FileNames : null;
        }
    }
}

using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
// ReSharper disable InconsistentNaming

namespace SteamLauncher.UI.Framework
{
    public class DialogHelper : DependencyObject
    {
        public ViewModelFramework ViewModel
        {
            get => (ViewModelFramework)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ViewModelFramework), typeof(DialogHelper),
            new UIPropertyMetadata(ViewModelProperty_Changed));

        private static void ViewModelProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (ViewModelProperty == null) 
                return;

            var myBinding = new Binding(nameof(Filename)) {Source = e.NewValue, Mode = BindingMode.OneWayToSource};
            BindingOperations.SetBinding(d, FilenameProperty, myBinding);
        }

        private string Filename
        {
            get => (string)GetValue(FilenameProperty);
            set => SetValue(FilenameProperty, value);
        }

        private static readonly DependencyProperty FilenameProperty =
            DependencyProperty.Register("Filename", typeof(string), typeof(DialogHelper),
            new UIPropertyMetadata(FilenameProperty_Changed));

        private static void FilenameProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine($"{nameof(DialogHelper)}.{nameof(Filename)} = {e.NewValue}");
        }

        private ICommand _openFile;


        public ICommand OpenFile => _openFile ??= new CommandHandler<object>(OpenFileAction, o => true);

        //public ICommand OpenFile { get; private set; }

        public DialogHelper()
        {
            
        }

        private void OpenFileAction(object obj = null)
        {
            var dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == true)
            {
                Filename = dlg.FileName;
            }
        }
    }
}

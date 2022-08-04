using System.Windows;
using SteamLauncher.UI.Framework;

namespace SteamLauncher.UI.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, ICloseable
    {
        public DialogWindow DialogWindow { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();

            DialogWindow = new DialogWindow(this);
        }

        //private void BtnCancel_Click(object sender, RoutedEventArgs e)
        //{
        //    var settingsViewModel = MainGrid.DataContext as SettingsViewModel;
        //    if (settingsViewModel.ProcessStartTimeoutSec != 1)
        //        settingsViewModel.ProcessStartTimeoutSec = 1;
        //    else
        //        settingsViewModel.ProcessStartTimeoutSec = 10;
        //}
    }
}

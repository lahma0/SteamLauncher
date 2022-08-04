using System;
using System.Diagnostics;
using System.Windows;

namespace SteamLauncherProxy_Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            //var hwnd = new WindowInteropHelper(this).Handle;
            //var extendedStyle = WindowMgmt.GetWindowLongPtr(hwnd, GwlIndex.GWL_EXSTYLE);
            //WindowMgmt.SetWindowLongPtr(hwnd, 
            //                            GwlIndex.GWL_EXSTYLE, 
            //                            new IntPtr((int)extendedStyle | WindowStylesExtended.WS_EX_TRANSPARENT.Value));
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            //Debug.WriteLine("SteamLauncherProxy window was activated!");
            //WindowState = WindowState.Minimized;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            //if (WindowState == WindowState.Normal || WindowState == WindowState.Maximized)
            //{
            //    Debug.WriteLine("SteamLauncherProxy window is in the foreground!");
            //    WindowState = WindowState.Minimized;
            //}
        }
    }
}

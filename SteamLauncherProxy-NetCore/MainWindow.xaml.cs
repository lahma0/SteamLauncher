using System;
using System.Windows;

namespace SteamLauncherProxy_NetCore
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
    }
}

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Resources;

namespace SteamLauncher.Tools
{
    public static class Resources
    {
        /// <summary>
        /// Large logo to be used in places where a more detailed, high quality icon is appropriate. This logo's cube
        /// has been enlarged to almost completely fill the circle it is contained within.
        /// </summary>
        public static StreamResourceInfo Logo1_Enlarged_256 =>
            Application.GetResourceStream(new Uri("/SteamLauncher;component/Resources/logo1_enlarged_256.png",
                                                  UriKind.Relative));

        /// <summary>
        /// Large logo to be used in places where a more detailed, high quality icon is appropriate. This is
        /// ChippiHeppu's original design (the cube's proportions have not been altered).
        /// </summary>
        public static StreamResourceInfo Logo1_256 =>
            Application.GetResourceStream(new Uri("/SteamLauncher;component/Resources/logo1_256.png",
                                                  UriKind.Relative));

        /// <summary>
        /// ICO file containing logo2 in sizes 16x16, 32x32, 48x48, and 256x256.
        /// </summary>
        public static StreamResourceInfo Logo2_Icon =>
            Application.GetResourceStream(new Uri("/SteamLauncher;component/Resources/logo2_icon.ico",
                                                  UriKind.Relative));

        /// <summary>
        /// 32x32 bitmap extracted from <see cref="Logo2_Icon"/>.
        /// </summary>
        public static Image Logo2_32_Image => new Icon(Resources.Logo2_Icon?.Stream).ToBitmap();
    }
}

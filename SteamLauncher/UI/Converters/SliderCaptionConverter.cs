using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SteamLauncher.UI.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class SliderCaptionConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var interval = "Second";
            if (parameter != null)
                interval = (string)parameter;

            var sliderValue = System.Convert.ToInt32(value);

            if (sliderValue <= 0)
                return "Disabled";

            return sliderValue == 1 ? $"{sliderValue} {interval}" : $"{sliderValue} {interval}s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

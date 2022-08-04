using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace SteamLauncher.UI.Converters
{
    //public class PropertyToDescriptionConverter : MarkupExtension, IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value == null)
    //            return Binding.DoNothing;

    //        var propertyName = parameter as string;
    //        if (string.IsNullOrEmpty(propertyName))
    //            return new ArgumentNullException(nameof(parameter)).ToString();

    //        var type = value.GetType();

    //        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
    //        if (property == null)
    //            return new ArgumentOutOfRangeException(nameof(parameter), parameter, 
    //                $"Property '{propertyName}' not found in type '{type.Name}'.").ToString();

    //        if (!property.IsDefined(typeof(DescriptionAttribute), true))
    //            return new ArgumentOutOfRangeException(nameof(parameter), parameter, 
    //                $"Property '{propertyName}' of type '{type.Name}' has no associated Description attribute.").ToString();

    //        return ((DescriptionAttribute)property.GetCustomAttributes(typeof(DescriptionAttribute), true)[0]).Description;
    //    }
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //    public override object ProvideValue(IServiceProvider serviceProvider)
    //    {
    //        return this;
    //    }
    //}
}

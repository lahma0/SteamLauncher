using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SteamLauncher.UI.Converters
{
    public class NameOfExtension : MarkupExtension
    {
        private readonly PropertyPath _propertyPath;

        public NameOfExtension(Binding binding)
        {
            _propertyPath = binding.Path;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var indexOfLastVariableName = _propertyPath.Path.LastIndexOf('.');
            return _propertyPath.Path.Substring(indexOfLastVariableName + 1);
        }
    }
}
